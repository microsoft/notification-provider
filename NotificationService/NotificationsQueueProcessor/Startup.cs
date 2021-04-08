// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(NotificationsQueueProcessor.Startup))]

namespace NotificationsQueueProcessor
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Data;
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

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var azureFuncConfig = builder?.Services?.BuildServiceProvider()?.GetService<IConfiguration>();
            var configBuilder = new ConfigurationBuilder();
            _ = configBuilder.AddConfiguration(azureFuncConfig);
            var configFolder = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent?.FullName;
            _ = configBuilder.SetBasePath(configFolder);
            _ = configBuilder.AddJsonFile("functionSettings.json");
            _ = configBuilder.AddEnvironmentVariables();

            var configuration = configBuilder.Build();
            MaxDequeueCount = configuration.GetSection(ConfigConstants.MaxDequeueCountConfigKey);

            AzureKeyVaultConfigurationOptions azureKeyVaultConfigurationOptions = new AzureKeyVaultConfigurationOptions(configuration[ConfigConstants.KeyVaultUrlConfigKey])
            {
                ReloadInterval = TimeSpan.FromSeconds(double.Parse(configuration[Constants.KeyVaultConfigRefreshDurationSeconds])),
            };
            _ = configBuilder.AddAzureKeyVault(azureKeyVaultConfigurationOptions);
            configuration = configBuilder.Build();
            _ = configBuilder.AddAzureAppConfiguration(options =>
            {
                var settings = options.Connect(configuration[ConfigConstants.AzureAppConfigConnectionstringConfigKey])
                 .Select(KeyFilter.Any, "Common").Select(KeyFilter.Any, "QueueProcessor");
                _ = settings.ConfigureRefresh(refreshOptions =>
                {
                    _ = refreshOptions.Register(key: configuration[ConfigConstants.ForceRefreshConfigKey], refreshAll: true, label: LabelFilter.Null);
                });
            });

            configuration = configBuilder.Build();

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
            _ = builder.Services.Configure<StorageAccountSetting>(s => s.ConnectionString = configuration[ConfigConstants.StorageAccountConnectionStringConfigKey]);
            _ = builder.Services.AddSingleton<IConfiguration>(configuration);
            _ = builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            _ = builder.Services.AddScoped<TableStorageEmailRepository>();
            _ = builder.Services.AddScoped<IEmailNotificationRepository, TableStorageEmailRepository>(s => s.GetService<TableStorageEmailRepository>());
            _ = builder.Services.AddScoped<ITableStorageClient, TableStorageClient>();
            _ = builder.Services.AddHttpClient<IHttpClientHelper, HttpClientHelper>();
        }
    }
}
