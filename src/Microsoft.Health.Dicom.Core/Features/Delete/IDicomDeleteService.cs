﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Dicom.Core.Features.Delete
{
    public interface IDicomDeleteService
    {
        Task DeleteStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default);

        Task DeleteSeriesAsync(string studyInstanceUid, string seriesInstanceUid, CancellationToken cancellationToken = default);

        Task DeleteInstanceAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, CancellationToken cancellationToken = default);

        Task CleanupDeletedInstancesAsync(CancellationToken cancellationToken = default);
    }
}
