// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;
using Common;
using Common.ServiceBus;
using Dicom;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Client;

namespace QidoFunctionApp
{
    public static class Qido
    {
        private static IDicomWebClient client;

        [FunctionName("Qido")]
        public static void Run([ServiceBusTrigger(KnownTopics.Qido, KnownSubscriptions.S1, Connection = "ServiceBusConnectionString")]byte[] message, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {Encoding.UTF8.GetString(message)}");
            SetupDicomWebClient();

            try
            {
                ProcessMessageWithQueryUrl(message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        private static void ProcessMessageWithQueryUrl(byte[] message)
        {
            DicomWebAsyncEnumerableResponse<DicomDataset> response = client.QueryAsync(Encoding.UTF8.GetString(message)).Result;
        }

        private static void SetupDicomWebClient()
        {
            Uri baseAddress = new Uri(KnownApplicationUrls.DicomServerUrl);

            client = new DicomWebClient(baseAddress);
        }
    }
}
