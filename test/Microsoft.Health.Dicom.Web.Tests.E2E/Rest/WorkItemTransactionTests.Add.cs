// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FellowOakDicom;
using EnsureThat;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Tests.Common;
using Xunit;
using Microsoft.Health.Dicom.Core.Models;

namespace Microsoft.Health.Dicom.Web.Tests.E2E.Rest
{
    public partial class WorkItemTransactionTests : IClassFixture<HttpIntegrationTestFixture<Startup>>
    {
        private readonly IDicomWebClient _client;

        public WorkItemTransactionTests(HttpIntegrationTestFixture<Startup> fixture)
        {
            EnsureArg.IsNotNull(fixture, nameof(fixture));
            _client = fixture.GetDicomWebClient();
        }

        [Fact]
        public async Task WhenAddingWorkitem_TheServerShouldCreateWorkitemSuccessfully()
        {
            DicomDataset dicomDataset = Samples.CreateRandomWorkitemInstanceDataset(ProcedureStepState.Scheduled);
            var workitemUid = dicomDataset.GetSingleValue<string>(DicomTag.AffectedSOPInstanceUID);

            using DicomWebResponse response = await _client.AddWorkitemAsync(Enumerable.Repeat(dicomDataset, 1), workitemUid);

            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
