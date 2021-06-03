// -------------------------------------------------------------------------------------------------
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
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Indexing;
using Microsoft.Health.Dicom.Functions.Indexing.Models;

namespace Microsoft.Health.Dicom.Functions.Indexing
{
    /// <summary>
    /// Represents the Azure Durable Functions that perform the re-indexing of previously added DICOM instances
    /// based on new tags configured by the user.
    /// </summary>
    public partial class ReindexOperation
    {
        /// <summary>
        /// The activity to complete operation.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="log">The log.</param>
        /// <returns>The task.</returns>
        [FunctionName(nameof(CompleteOperationAsync))]
        public Task CompleteOperationAsync([ActivityTrigger] string operationId, ILogger log)
        {
            EnsureArg.IsNotNull(log, nameof(log));

            log.LogInformation("Completing Reindex operation on {operationId}", operationId);
            return _tagOperationStore.CompleteOperationAsync(operationId);
        }

        /// <summary>
        /// The activity to start reindex operation.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="log">The log.</param>
        /// <returns>The task.</returns>
        [FunctionName(nameof(StartOperationAsync))]
        public Task StartOperationAsync([ActivityTrigger] StartOperationInput input, ILogger log)
        {
            EnsureArg.IsNotNull(log, nameof(log));
            EnsureArg.IsNotNull(input, nameof(input));

            log.LogInformation("Starting reindex operation with input {input}", input);
            return _tagOperationStore.StartOperationAsync(input.OperationId, input.TagStoreEntries);
        }

        /// <summary>
        ///  The activity to add extended query tags.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="log">The log.</param>
        /// <returns>The store entries.</returns>
        [FunctionName(nameof(AddExtendedQueryTagsAsync))]
        public async Task<Core.Features.Indexing.ReindexOperation> AddExtendedQueryTagsAsync([ActivityTrigger] AddExtendedQueryTagsInput input, ILogger log)
        {
            EnsureArg.IsNotNull(input, nameof(input));
            EnsureArg.IsNotNull(log, nameof(log));
            log.LogInformation("Adding extended query tags with input {input}", input);
            return (await _addExtendedQueryTagService.AddExtendedQueryTagAsync(input.ExtendedQueryTagEntries, input.OperationId)).Operation;
        }

        /// <summary>
        /// The activity to get extended query tags on opoeration.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="log">The log.</param>
        /// <returns>Extended query tag store entries.</returns>
        [FunctionName(nameof(GetExtendedQueryTagsOfOperationAsync))]
        public async Task<IReadOnlyList<ExtendedQueryTagStoreEntry>> GetExtendedQueryTagsOfOperationAsync([ActivityTrigger] string operationId, ILogger log)
        {
            EnsureArg.IsNotNull(log, nameof(log));

            log.LogInformation("Getting extended query tags of {operationId}", operationId);
            var entries = await _tagOperationStore.GetEntriesOfOperationAsync(operationId);
            // only process tags which is on Processing
            return entries.Where(x => x.Status == TagOperationStatus.Processing).Select(y => y.TagStoreEntry).ToList();
        }

        /// <summary>
        ///  The activity to update end watermark of an operation.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="log">The log</param>
        /// <returns>The task.</returns>
        [FunctionName(nameof(UpdateOperationProgress))]
        public Task UpdateOperationProgress([ActivityTrigger] UpdateOperationProgressInput input, ILogger log)
        {
            EnsureArg.IsNotNull(input, nameof(input));
            EnsureArg.IsNotNull(log, nameof(log));

            log.LogInformation($"Updating end watermark of operation: {input.OperationId}"); ;
            return _tagOperationStore.UpdateEndWatermarkOfOperationAsync(input.OperationId, input.EndWatermark);
        }

        /// <summary>
        /// The activity to reindex  Dicom instance.
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="logger">The log.</param>
        /// <returns>The task</returns>
        [FunctionName(nameof(ReindexInstanceAsync))]
        public async Task ReindexInstanceAsync([ActivityTrigger] ReindexInstanceInput input, ILogger logger)
        {
            EnsureArg.IsNotNull(input, nameof(input));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var watermarks = await _tagOperationStore.GetWatermarksAsync(input.StartWatermark, input.EndWatermark);

            logger.LogInformation("Reindexing with {input}", input);
            var tasks = new List<Task>();
            foreach (var watermark in watermarks)
            {
                tasks.Add(_instanceReindexer.ReindexInstanceAsync(input.TagEntries, watermark));
            }

            await Task.WhenAll(tasks);
        }
    }
}
