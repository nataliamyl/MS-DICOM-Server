﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Models.Duplicate;
using Microsoft.Health.Dicom.Core.Models.Operations;
using Microsoft.Health.Dicom.Functions.Duplicate.Models;
using Microsoft.Health.Dicom.Functions.Indexing.Models;
using Microsoft.Health.Operations.Functions.DurableTask;

namespace Microsoft.Health.Dicom.Functions.Duplicate;

public partial class DuplicateDurableFunction
{
    /// <summary>
    /// Asynchronously creates an index for the provided query tags over the previously added data.
    /// </summary>
    /// <remarks>
    /// Durable functions are reliable, and their implementations will be executed repeatedly over the lifetime of
    /// a single instance.
    /// </remarks>
    /// <param name="context">The context for the orchestration instance.</param>
    /// <param name="logger">A diagnostic logger.</param>
    /// <returns>A task representing the <see cref="DuplicateInstancesAsync"/> operation.</returns>
    [FunctionName(nameof(DuplicateInstancesAsync))]
    public async Task DuplicateInstancesAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger logger)
    {
        // The ID should be a GUID as generated by the trigger, but we'll assert here just to make sure!
        EnsureArg.IsNotNull(context, nameof(context)).ThrowIfInvalidOperationId();

        logger = context.CreateReplaySafeLogger(logger);
        DuplicateCheckpoint input = context.GetInput<DuplicateCheckpoint>();

        // Backfill batching options
        input.Batching ??= new BatchingOptions
        {
            MaxParallelCount = _options.MaxParallelBatches,
            Size = _options.BatchSize,
        };


        IReadOnlyList<WatermarkRange> batches = await context.CallActivityWithRetryAsync<IReadOnlyList<WatermarkRange>>(
            nameof(GetDuplicateInstanceBatchesAsync),
            _options.RetryOptions,
            new BatchCreationArguments(input.Completed?.Start - 1, input.Batching.Size, input.Batching.MaxParallelCount));

        if (batches.Count > 0)
        {
            // Note that batches are in reverse order because we start from the highest watermark
            var batchRange = new WatermarkRange(batches[^1].Start, batches[0].End);

            logger.LogInformation("Beginning to duplicate the range {Range}.", batchRange);
            await Task.WhenAll(batches
                .Select(x => context.CallActivityWithRetryAsync(
                    nameof(DuplicateBatchAsync),
                    _options.RetryOptions,
                    DuplicateBatchArguments.FromOptions(x, _options))));

            // Create a new orchestration with the same instance ID to process the remaining data
            logger.LogInformation("Completed duplicating the range {Range}. Continuing with new execution...", batchRange);

            WatermarkRange completed = input.Completed.HasValue
                ? new WatermarkRange(batchRange.Start, input.Completed.Value.End)
                : batchRange;

            context.ContinueAsNew(
                new DuplicateCheckpoint
                {
                    Completed = completed,
                    CreatedTime = input.CreatedTime ?? await context.GetCreatedTimeAsync(_options.RetryOptions),
                });
        }
        else
        {
            await context.CallActivityWithRetryAsync(nameof(CompleteDuplicateAsync), _options.RetryOptions, null);

            logger.LogInformation("Completed duplication.");
        }

    }


}
