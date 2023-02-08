// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Health.DicomCast.Core.Features.Worker;
using Microsoft.Health.DicomCast.Core.Modules;
using Microsoft.Health.DicomCast.TableStorage;
using Microsoft.Health.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Microsoft.Health.DicomCast.Hosting;

public static class Program
{
    public static void Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, builder) =>
            {
                IConfiguration builtConfig = builder.Build();

                // TODO: Use Azure SDK directly for settings
                string keyVaultEndpoint = builtConfig["KeyVault:Endpoint"];
                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {
                    builder.AddAzureKeyVault(
                        new SecretClient(new Uri(keyVaultEndpoint), new DefaultAzureCredential()),
                        new AzureKeyVaultConfigurationOptions());
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration configuration = hostContext.Configuration;

                services.RegisterAssemblyModules(typeof(WorkerModule).Assembly, configuration);

                services.AddTableStorageDataStore(configuration);

                services.AddHostedService<DicomCastBackgroundService>();

                AddOpenTelemetry(services, configuration);
            })
            .Build();

        host.Run();
    }

    /// <summary>
    /// Adds Open Telemetry exporter for Azure Monitor.
    /// </summary>
    private static void AddOpenTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DicomCastMeter>();

        string instrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];

        if (!string.IsNullOrWhiteSpace(instrumentationKey))
        {
            var connectionString = $"InstrumentationKey={instrumentationKey}";

            services.AddSingleton(Sdk.CreateMeterProviderBuilder()
                .AddMeter("Microsoft.Health.DicomCast")
                .AddAzureMonitorMetricExporter(o => o.ConnectionString = connectionString)
                .Build());
        }
    }
}
