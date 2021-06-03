﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Core.Features.Indexing;

namespace Microsoft.Health.Dicom.Core.Messages.ExtendedQueryTag
{
    public class AddExtendedQueryTagResponse
    {
        public ReindexOperation Operation { get; set; }
    }
}
