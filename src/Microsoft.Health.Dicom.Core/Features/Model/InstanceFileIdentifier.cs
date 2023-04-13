// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core.Features.Model;

public class InstanceFileIdentifier
{
    public long Version { get; init; }

    public long? OriginalVersion { get; init; }

    public long? NewVersion { get; init; }
}
