﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Export;
using Microsoft.Health.Dicom.Core.Features.Operations;
using Microsoft.Health.Dicom.Core.Features.Partition;
using Microsoft.Health.Dicom.Core.Models;
using Microsoft.Health.Dicom.Core.Models.Export;
using Microsoft.Health.Operations;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Export;

public class ExportServiceTests
{
    private const ExportSourceType SourceType = ExportSourceType.Identifiers;
    private const ExportDestinationType DestinationType = ExportDestinationType.AzureBlob;

    private readonly IExportSourceProvider _sourceProvider;
    private readonly IExportSinkProvider _sinkProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGuidFactory _guidFactory;
    private readonly IDicomOperationsClient _client;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _sourceProvider = Substitute.For<IExportSourceProvider>();
        _sourceProvider.Type.Returns(SourceType);
        _sinkProvider = Substitute.For<IExportSinkProvider>();
        _sinkProvider.Type.Returns(DestinationType);
        _client = Substitute.For<IDicomOperationsClient>();
        _guidFactory = Substitute.For<IGuidFactory>();
        _serviceProvider = Substitute.For<IServiceProvider>();
        _service = new ExportService(
            new ExportSourceFactory(_serviceProvider, new IExportSourceProvider[] { _sourceProvider }),
            new ExportSinkFactory(_serviceProvider, new IExportSinkProvider[] { _sinkProvider }),
            _guidFactory,
            _client);
    }

    [Fact]
    public void GivenNullArgument_WhenConstructing_ThenThrowArgumentNullException()
    {
        var source = new ExportSourceFactory(_serviceProvider, new IExportSourceProvider[] { _sourceProvider });
        var sink = new ExportSinkFactory(_serviceProvider, new IExportSinkProvider[] { _sinkProvider });

        Assert.Throws<ArgumentNullException>(() => new ExportService(null, sink, _guidFactory, _client));
        Assert.Throws<ArgumentNullException>(() => new ExportService(source, null, _guidFactory, _client));
        Assert.Throws<ArgumentNullException>(() => new ExportService(source, sink, null, _client));
        Assert.Throws<ArgumentNullException>(() => new ExportService(source, sink, _guidFactory, null));
    }

    [Fact]
    public async Task GivenSpecification_WhenStartingExport_ThenValidateBeforeStarting()
    {
        using var tokenSource = new CancellationTokenSource();

        var operationId = Guid.NewGuid();
        IConfiguration originalSource = Substitute.For<IConfiguration>();
        IConfiguration originalDestination = Substitute.For<IConfiguration>();
        IConfiguration validatedSource = Substitute.For<IConfiguration>();
        IConfiguration validatedDestination = Substitute.For<IConfiguration>();
        var spec = new ExportSpecification
        {
            Destination = new TypedConfiguration<ExportDestinationType> { Type = DestinationType, Configuration = originalDestination },
            Source = new TypedConfiguration<ExportSourceType> { Type = SourceType, Configuration = originalSource },
        };
        var partition = new PartitionEntry(123, "test");
        var expected = new OperationReference(operationId, new Uri("http://test/export"));

        _guidFactory.Create().Returns(operationId);
        _sourceProvider.ValidateAsync(originalSource, tokenSource.Token).Returns(validatedSource);
        _sinkProvider.ValidateAsync(originalDestination, operationId, tokenSource.Token).Returns(validatedDestination);
        _client
            .StartExportAsync(
                operationId,
                Arg.Is<ExportSpecification>(x => ReferenceEquals(validatedSource, x.Source.Configuration)
                    && ReferenceEquals(validatedDestination, x.Destination.Configuration)),
                partition,
                tokenSource.Token)
            .Returns(expected);

        Assert.Same(expected, await _service.StartExportAsync(spec, partition, tokenSource.Token));

        _guidFactory.Received(1).Create();
        await _sourceProvider.Received(1).ValidateAsync(originalSource, tokenSource.Token);
        await _sinkProvider.Received(1).ValidateAsync(originalDestination, operationId, tokenSource.Token);
        await _client
            .Received(1)
            .StartExportAsync(
                operationId,
                Arg.Is<ExportSpecification>(x => ReferenceEquals(validatedSource, x.Source.Configuration)
                    && ReferenceEquals(validatedDestination, x.Destination.Configuration)),
                partition,
                tokenSource.Token);
    }
}
