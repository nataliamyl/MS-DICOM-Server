﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using MediatR;

namespace Microsoft.Health.Dicom.Core.Messages.Operations
{
    public class OperationStateRequest : IRequest<OperationStateResponse>
    {
        public OperationStateRequest(string id)
        {
            EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));
            Id = id;
        }

        public string Id { get; set; }
    }
}
