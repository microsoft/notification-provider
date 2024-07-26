﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(NotificationsQueueProcessor.Startup))]

namespace NotificationsQueueProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Azure.Extensions.AspNetCore.Configuration.Secrets;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.DependencyInjection;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Data;
    using NotificationService.Data.Helper;
    using NotificationService.Data.Interfaces;
    using NotificationService.Data.Repositories;

    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Gets the maxDequeueCount from host.json.
        /// </summary>
        public static IConfigurationSection MaxDequeueCount { get; private set; }

        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {

            var configuration = builder?.Services?.BuildServiceProvider()?.GetService<IConfiguration>();

            MaxDequeueCount = configuration.GetSection(ConfigConstants.MaxDequeueCountConfigKey);

            _ = builder.Services.AddAzureAppConfiguration();

            ITelemetryInitializer[] itm = new ITelemetryInitializer[1];
            var envInitializer = new EnvironmentInitializer
            {
                Service = configuration[AIConstants.ServiceConfigName],
                ServiceLine = configuration[AIConstants.ServiceLineConfigName],
                ServiceOffering = configuration[AIConstants.ServiceOfferingConfigName],
                ComponentId = configuration[AIConstants.ComponentIdConfigName],
                ComponentName = configuration[AIConstants.ComponentNameConfigName],
                EnvironmentName = configuration[AIConstants.EnvironmentName],
                IctoId = "IctoId",
            };
            itm[0] = envInitializer;
            LoggingConfiguration loggingConfiguration = new LoggingConfiguration
            {
                IsTraceEnabled = true,
                TraceLevel = (SeverityLevel)Enum.Parse(typeof(SeverityLevel), configuration[ConfigConstants.AITraceLelelConfigKey]),
                EnvironmentName = configuration[AIConstants.EnvironmentName],
            };

            var tconfig = TelemetryConfiguration.CreateDefault();
            tconfig.InstrumentationKey = configuration[ConfigConstants.AIInsrumentationConfigKey];

            DependencyTrackingTelemetryModule depModule = new DependencyTrackingTelemetryModule();
            depModule.Initialize(tconfig);

            RequestTrackingTelemetryModule requestTrackingTelemetryModule = new RequestTrackingTelemetryModule();
            requestTrackingTelemetryModule.Initialize(tconfig);

            _ = builder.Services.AddSingleton<ILogger>(_ => new AILogger(loggingConfiguration, tconfig, itm));

            StorageType storageType = (StorageType)Enum.Parse(typeof(StorageType), configuration?[ConfigConstants.StorageType]);
            if (storageType == StorageType.DocumentDB)
            {
                _ = builder.Services.Configure<CosmosDBSetting>(configuration.GetSection(ConfigConstants.CosmosDBConfigSectionKey));
                _ = builder.Services.Configure<CosmosDBSetting>(s => s.Key = configuration[ConfigConstants.CosmosDBKeyConfigKey]);
                _ = builder.Services.Configure<CosmosDBSetting>(s => s.Uri = configuration[ConfigConstants.CosmosDBURIConfigKey]);
                _ = builder.Services.AddScoped<ICosmosLinqQuery, CustomCosmosLinqQuery>();
                _ = builder.Services.AddSingleton<ICosmosDBQueryClient, CosmosDBQueryClient>();
                _ = builder.Services.AddScoped<EmailNotificationRepository>();
                _ = builder.Services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>(s => s.GetService<EmailNotificationRepository>());
            }

            _ = builder.Services.Configure<StorageAccountSetting>(configuration.GetSection(ConfigConstants.StorageAccountConfigSectionKey));
            _ = builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            _ = builder.Services.AddScoped<TableStorageEmailRepository>();
            _ = builder.Services.AddScoped<IEmailNotificationRepository, TableStorageEmailRepository>(s => s.GetService<TableStorageEmailRepository>());
            _ = builder.Services.AddScoped<ITableStorageClient, TableStorageClient>();
            _ = builder.Services.AddHttpClient<IHttpClientHelper, HttpClientHelper>();

            _ = builder.Services.BuildServiceProvider();
        }

        /// <inheritdoc/>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {

            var configBuilder = builder.ConfigurationBuilder;

            var configFolder = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent?.FullName;
            _ = configBuilder.SetBasePath(configFolder);
            _ = configBuilder.AddJsonFile("functionSettings.json");
            _ = configBuilder.AddEnvironmentVariables();

            var configuration = configBuilder.Build();
            _ = configBuilder.AddAzureKeyVault(
                new SecretClient(
                    new Uri(configuration[ConfigConstants.KeyVaultUrlConfigKey]),
                    new DefaultAzureCredential()),
                new AzureKeyVaultConfigurationOptions()
                {
                    ReloadInterval = TimeSpan.FromSeconds(double.Parse(configuration[ConfigConstants.KeyVaultConfigRefreshDurationSeconds], CultureInfo.InvariantCulture)),
                });
            configuration = configBuilder.Build();
            IConfigurationRefresher configurationRefresher = null;

            _ = configBuilder.AddAzureAppConfiguration((options) =>
              {
                  _ = options.Connect(new Uri(configuration[ConfigConstants.AzureAppConfigEndPoint]), AzureCredentialHelper.AzureCredentials);
                  _ = options.ConfigureRefresh(refreshOptions =>
                    {
                        _ = refreshOptions.Register(ConfigConstants.ForceRefreshConfigKey, "Common", refreshAll: true);
                    })
                  .Select(KeyFilter.Any, "Common").Select(KeyFilter.Any, "QueueProcessor");
                  configurationRefresher = options.GetRefresher();
              });
        }
    }
}
