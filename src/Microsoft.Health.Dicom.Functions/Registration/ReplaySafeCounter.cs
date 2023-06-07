// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Diagnostics.Metrics;
using EnsureThat;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Microsoft.Health.Dicom.Functions.Registration;

/// <summary>
/// Replay-safe counter that can emits the metric only when the orchestrator is not replaying. Other
/// methods in the Counter class can be added as required.
/// </summary>
public class ReplaySafeCounter
{
    private readonly IDurableOrchestrationContext _context;

    private readonly Counter<int> _counter;

    internal ReplaySafeCounter(IDurableOrchestrationContext context, Counter<int> counter)
    {
        _context = EnsureArg.IsNotNull(context, nameof(context));
        _counter = EnsureArg.IsNotNull(counter, nameof(counter));
    }

    public void Add(int count)
    {
        if (!_context.IsReplaying)
        {
            _counter.Add(count);
        }
    }
}
