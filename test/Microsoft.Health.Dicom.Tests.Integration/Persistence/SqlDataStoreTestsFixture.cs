﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.SqlServer.Features.Retrieve;
using Microsoft.Health.Dicom.SqlServer.Features.Schema;
using Microsoft.Health.Dicom.SqlServer.Features.Storage;
using Microsoft.Health.Dicom.SqlServer.Features.Store;
using Microsoft.Health.SqlServer;
using Microsoft.Health.SqlServer.Configs;
using Microsoft.Health.SqlServer.Features.Client;
using Microsoft.Health.SqlServer.Features.Schema;
using Microsoft.Health.SqlServer.Features.Storage;
using NSubstitute;
using Polly;
using Xunit;

namespace Microsoft.Health.Dicom.Tests.Integration.Persistence
{
    public class SqlDataStoreTestsFixture : IAsyncLifetime
    {
        private const string LocalConnectionString = "server=(local);Integrated Security=true";

        private readonly string _masterConnectionString;
        private readonly string _databaseName;
        private readonly SchemaInitializer _schemaInitializer;
        private static readonly object _locker = new object();

        public SqlDataStoreTestsFixture()
        {
            string initialConnectionString = Environment.GetEnvironmentVariable("SqlServer:ConnectionString") ?? LocalConnectionString;

            _databaseName = $"DICOMINTEGRATIONTEST_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{BigInteger.Abs(new BigInteger(Guid.NewGuid().ToByteArray()))}";
            _masterConnectionString = new SqlConnectionStringBuilder(initialConnectionString) { InitialCatalog = "master" }.ToString();
            TestConnectionString = new SqlConnectionStringBuilder(initialConnectionString) { InitialCatalog = _databaseName }.ToString();

            var config = new SqlServerDataStoreConfiguration
            {
                ConnectionString = TestConnectionString,
                Initialize = true,
                SchemaOptions = new SqlServerSchemaOptions
                {
                    AutomaticUpdatesEnabled = true,
                },
            };

            var scriptProvider = new ScriptProvider<SchemaVersion>();

            var baseScriptProvider = new BaseScriptProvider();

            var mediator = Substitute.For<IMediator>();

            var sqlConnectionFactory = new DefaultSqlConnectionFactory(config);

            var schemaUpgradeRunner = new SchemaUpgradeRunner(scriptProvider, baseScriptProvider, mediator, NullLogger<SchemaUpgradeRunner>.Instance, sqlConnectionFactory);

            var schemaInformation = new SchemaInformation((int)SchemaVersion.V1, (int)SchemaVersion.V1);

            _schemaInitializer = new SchemaInitializer(config, schemaUpgradeRunner, schemaInformation, sqlConnectionFactory, NullLogger<SchemaInitializer>.Instance);

            var dicomSqlIndexSchema = new SqlIndexSchema(schemaInformation, NullLogger<SqlIndexSchema>.Instance);

            SqlTransactionHandler = new SqlTransactionHandler();

            SqlConnectionWrapperFactory = new SqlConnectionWrapperFactory(SqlTransactionHandler, new SqlCommandWrapperFactory(), sqlConnectionFactory);

            IndexDataStore = new SqlIndexDataStore(
                dicomSqlIndexSchema,
                SqlConnectionWrapperFactory);

            InstanceStore = new SqlInstanceStore(SqlConnectionWrapperFactory);

            TestHelper = new SqlIndexDataStoreTestHelper(TestConnectionString);
        }

        public SqlTransactionHandler SqlTransactionHandler { get; }

        public SqlConnectionWrapperFactory SqlConnectionWrapperFactory { get; }

        public string TestConnectionString { get; }

        public IIndexDataStore IndexDataStore { get; }

        public IInstanceStore InstanceStore { get; }

        public SqlIndexDataStoreTestHelper TestHelper { get; }

        private void LogInfo(string message)
        {
            string finalMsg = $"[penche-Console] [{DateTime.Now}] [TID:{Thread.CurrentThread.ManagedThreadId}]" + message;
            Console.WriteLine(finalMsg);
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() => { });
            lock (_locker)
            {
                LogInfo($"Start Create Database: {_masterConnectionString}");

                // Create the database
                using (var sqlConnection = new SqlConnection(_masterConnectionString))
                {
                    sqlConnection.OpenAsync().Wait();

                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandTimeout = 600;
                        command.CommandText = $"CREATE DATABASE {_databaseName}";
                        command.ExecuteNonQueryAsync().Wait();
                    }
                }

                LogInfo($"Complete Create Database: {_databaseName}");

                LogInfo($"Start Verify able to connect to database");

                // verify that we can connect to the new database. This sometimes does not work right away with Azure SQL.
                Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 7,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                    .ExecuteAsync(async () =>
                    {
                        using (var sqlConnection = new SqlConnection(TestConnectionString))
                        {
                            await sqlConnection.OpenAsync();
                            using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                            {
                                sqlCommand.CommandText = "SELECT 1";
                                await sqlCommand.ExecuteScalarAsync();
                            }
                        }
                    }).Wait();
                LogInfo($"Complete Verify able to connect to database");
                LogInfo($"Start Schema init");
                _schemaInitializer.Start();
                LogInfo($"Complete Schema init");
            }
        }

        public async Task DisposeAsync()
        {
            LogInfo($"Start delete database {_databaseName}");
            using (var sqlConnection = new SqlConnection(_masterConnectionString))
            {
                await sqlConnection.OpenAsync();
                SqlConnection.ClearAllPools();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandTimeout = 600;
                    sqlCommand.CommandText = $"DROP DATABASE IF EXISTS {_databaseName}";
                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }

            LogInfo($"Complete delete database {_databaseName}");
        }
    }
}
