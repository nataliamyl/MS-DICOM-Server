﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using EnsureThat;
using Microsoft.Extensions.Options;
using Microsoft.Health.Blob.Configs;
using Microsoft.Health.Dicom.Core.Configs;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Model;

namespace Microsoft.Health.Dicom.Blob.Features.Storage;

/// <summary>
/// Provides functionality for managing the DICOM files using the Azure Blob storage.
/// </summary>
public class BlobFileStore : IFileStore
{
    private readonly BlobContainerClient _container;
    private readonly BlobOperationOptions _options;
    private readonly bool _enableDualWrite;
    private readonly bool _supportNewBlobFormatForNewService;
    private readonly DicomFileNameWithUID _fileNameWithUID;
    private readonly DicomFileNameWithPrefix _nameWithPrefix;

    public BlobFileStore(
        BlobServiceClient client,
        DicomFileNameWithUID fileNameWithUID,
        DicomFileNameWithPrefix nameWithPrefix,
        IOptionsMonitor<BlobContainerConfiguration> namedBlobContainerConfigurationAccessor,
        IOptions<BlobOperationOptions> options,
        IOptions<FeatureConfiguration> featureConfiguration)
    {
        EnsureArg.IsNotNull(client, nameof(client));
        EnsureArg.IsNotNull(fileNameWithUID, nameof(fileNameWithUID));
        EnsureArg.IsNotNull(nameWithPrefix, nameof(nameWithPrefix));
        EnsureArg.IsNotNull(namedBlobContainerConfigurationAccessor, nameof(namedBlobContainerConfigurationAccessor));
        EnsureArg.IsNotNull(options?.Value, nameof(options));
        EnsureArg.IsNotNull(featureConfiguration, nameof(featureConfiguration));

        BlobContainerConfiguration containerConfiguration = namedBlobContainerConfigurationAccessor
            .Get(Constants.BlobContainerConfigurationName);

        _container = client.GetBlobContainerClient(containerConfiguration.ContainerName);
        _options = options.Value;
        _fileNameWithUID = fileNameWithUID;
        _nameWithPrefix = nameWithPrefix;
        _enableDualWrite = featureConfiguration.Value.EnableDualWrite;
        _supportNewBlobFormatForNewService = featureConfiguration.Value.SupportNewBlobFormatForNewService;
    }

    /// <inheritdoc />
    public async Task<Uri> StoreFileAsync(
        VersionedInstanceIdentifier versionedInstanceIdentifier,
        Stream stream,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(versionedInstanceIdentifier, nameof(versionedInstanceIdentifier));
        EnsureArg.IsNotNull(stream, nameof(stream));

        BlockBlobClient[] blobs = GetInstanceBlockBlobs(versionedInstanceIdentifier);
        stream.Seek(0, SeekOrigin.Begin);

        var blobUploadOptions = new BlobUploadOptions { TransferOptions = _options.Upload };

        try
        {
            await Task.WhenAll(blobs.Select(blob => blob.UploadAsync(stream, blobUploadOptions, cancellationToken)));
            return blobs[0].Uri;
        }
        catch (Exception ex)
        {
            throw new DataStoreException(ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteFileIfExistsAsync(
        VersionedInstanceIdentifier versionedInstanceIdentifier,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(versionedInstanceIdentifier, nameof(versionedInstanceIdentifier));

        BlockBlobClient[] blobs = GetInstanceBlockBlobs(versionedInstanceIdentifier);

        await Task.WhenAll(blobs.Select(blob => ExecuteAsync(() => blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, conditions: null, cancellationToken))));
    }

    /// <inheritdoc />
    public async Task<Stream> GetFileAsync(
        VersionedInstanceIdentifier versionedInstanceIdentifier,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(versionedInstanceIdentifier, nameof(versionedInstanceIdentifier));

        BlockBlobClient blob = GetInstanceBlockBlob(versionedInstanceIdentifier);

        Stream stream = null;
        var blobOpenReadOptions = new BlobOpenReadOptions(allowModifications: false);

        await ExecuteAsync(async () =>
        {
            stream = await blob.OpenReadAsync(blobOpenReadOptions, cancellationToken);
        });

        return stream;
    }

    public async Task<FileProperties> GetFilePropertiesAsync(
        VersionedInstanceIdentifier versionedInstanceIdentifier,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(versionedInstanceIdentifier, nameof(versionedInstanceIdentifier));

        BlockBlobClient blob = GetInstanceBlockBlob(versionedInstanceIdentifier);
        FileProperties fileProperties = null;

        await ExecuteAsync(async () =>
        {
            var response = await blob.GetPropertiesAsync(conditions: null, cancellationToken);
            fileProperties = response.Value.ToFileProperties();
        });

        return fileProperties;
    }

    // TODO: This should removed once we migrate everything and the global flag is turned on
    private BlockBlobClient GetInstanceBlockBlob(VersionedInstanceIdentifier versionedInstanceIdentifier)
    {
        string blobName;
        if (_supportNewBlobFormatForNewService)
        {
            blobName = _nameWithPrefix.GetInstanceFileName(versionedInstanceIdentifier);
        }
        else
        {
            blobName = _fileNameWithUID.GetInstanceFileName(versionedInstanceIdentifier);
        }

        return _container.GetBlockBlobClient(blobName);
    }

    private BlockBlobClient[] GetInstanceBlockBlobs(VersionedInstanceIdentifier versionedInstanceIdentifier)
    {
        var clients = new List<BlockBlobClient>();

        string blobName;

        if (_supportNewBlobFormatForNewService)
        {
            blobName = _nameWithPrefix.GetInstanceFileName(versionedInstanceIdentifier);
            clients.Add(_container.GetBlockBlobClient(blobName));
        }
        else if (_enableDualWrite)
        {
            blobName = _fileNameWithUID.GetInstanceFileName(versionedInstanceIdentifier);
            clients.Add(_container.GetBlockBlobClient(blobName));

            blobName = _nameWithPrefix.GetInstanceFileName(versionedInstanceIdentifier);
            clients.Add(_container.GetBlockBlobClient(blobName));
        }
        else
        {
            blobName = _fileNameWithUID.GetInstanceFileName(versionedInstanceIdentifier);
            clients.Add(_container.GetBlockBlobClient(blobName));
        }

        return clients.ToArray();
    }

    private static async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            throw new ItemNotFoundException(ex);
        }
        catch (Exception ex)
        {
            throw new DataStoreException(ex);
        }
    }
}
