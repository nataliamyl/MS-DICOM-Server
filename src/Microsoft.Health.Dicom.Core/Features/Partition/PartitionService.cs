﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Partition;

namespace Microsoft.Health.Dicom.Core.Features.ChangeFeed
{
    public class PartitionService : IPartitionService
    {
        private readonly IPartitionStore _partitionStore;

        public PartitionService(IPartitionStore partitionStore)
        {
            EnsureArg.IsNotNull(partitionStore, nameof(partitionStore));

            _partitionStore = partitionStore;
        }

        public Task<PartitionEntry> AddPartition(string partitionName, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<PartitionEntry> GetPartition(string partitionName, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<PartitionEntry>> GetPartitions(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
