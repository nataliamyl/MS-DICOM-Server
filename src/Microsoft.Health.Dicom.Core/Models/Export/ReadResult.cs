﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Model;

namespace Microsoft.Health.Dicom.Core.Features.Export;

/// <summary>
/// Represents the result of reading a DICOM study, series, or SOP instance during an export operation.
/// </summary>
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Identifiers are not equatable.")]
public readonly struct ReadResult
{
    /// <summary>
    /// Gets the resolved instance identifier.
    /// </summary>
    /// <remarks>
    /// This value is non-<see langword="null"/> if the read was successful.
    /// </remarks>
    /// <value>The successfully read identifier, if successful; otherwise <see langword="null"/>.</value>
    public VersionedInstanceIdentifier Identifier { get; }

    /// <summary>
    /// Gets the failure associated with the read.
    /// </summary>
    /// <remarks>
    /// This value is non-<see langword="null"/> if the read was unsuccessful.
    /// </remarks>
    /// <value>The failure that caused the read to fail, if unsuccessful; otherwise <see langword="null"/>.</value>
    public ReadFailureEventArgs Failure { get; }

    private ReadResult(VersionedInstanceIdentifier identifier, ReadFailureEventArgs failure)
    {
        Identifier = identifier;
        Failure = failure;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ReadResult"/> class for an instance identifier.
    /// </summary>
    /// <param name="identifier">A DICOM SOP instance identifier.</param>
    /// <returns>A successful instance of the <see cref="ReadResult"/> structure.</returns>
    public static ReadResult ForIdentifier(VersionedInstanceIdentifier identifier)
        => new ReadResult(EnsureArg.IsNotNull(identifier, nameof(identifier)), null);

    /// <summary>
    /// Creates a new instance of the <see cref="ReadResult"/> class for a read failure.
    /// </summary>
    /// <param name="args">The event arguments for a read failure event.</param>
    /// <returns>An unsuccessful instance of the <see cref="ReadResult"/> structure.</returns>
    public static ReadResult ForFailure(ReadFailureEventArgs args)
        => new ReadResult(null, args);
}
