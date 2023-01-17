// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FellowOakDicom;

namespace Microsoft.Health.Dicom.Core.Features.Store;

public sealed class StoreValidationResult
{
    public StoreValidationResult(
        IReadOnlyCollection<string> warnings,
        ValidationWarnings validationWarnings,
        Exception firstException,
        IReadOnlyDictionary<DicomTag, StoreErrorResult> invalidTagErrors)
    {
        Warnings = warnings;
        WarningCodes = validationWarnings;
        FirstException = firstException;
        InvalidTagErrors = invalidTagErrors;
    }

    public IReadOnlyCollection<string> Warnings { get; }

    public ValidationWarnings WarningCodes { get; }

    // TODO: Remove this during the cleanup. *** Hack to support the existing validator behavior ***
    public Exception FirstException { get; }

    public IReadOnlyDictionary<DicomTag, StoreErrorResult> InvalidTagErrors { get; }
}
