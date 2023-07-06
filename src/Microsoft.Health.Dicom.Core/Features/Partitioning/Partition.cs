﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.Json.Serialization;
using EnsureThat;

namespace Microsoft.Health.Dicom.Core.Features.Partitioning;

public class Partition
{
    public const string DefaultName = "Microsoft.Default";

    public const int DefaultKey = 1;

    [JsonPropertyName("partitionKey")]
    public int Key { get; }

    [JsonPropertyName("partitionName")]
    public string Name { get; }

    public DateTimeOffset CreatedDate { get; set; }

    public static Partition Default { get; } = new(DefaultKey, DefaultName);

    public Partition(int key, string name, DateTimeOffset createdDate = default)
    {
        Key = key;
        Name = EnsureArg.IsNotNull(name, nameof(name));
        CreatedDate = createdDate;
    }
}