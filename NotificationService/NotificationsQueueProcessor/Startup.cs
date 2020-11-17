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
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using NotificationService.Common;
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
            MaxDequeueCount = configuration.GetSection("AzureFunctionsJobHost:extensions:queues:maxDequeueCount");
            ITelemetryInitializer[] itm = new ITelemetryInitializer[1];
            var envInitializer = new EnvironmentInitializer
            {
                Service = configuration[NotificationService.Common.Constants.ServiceConfigName],
                ServiceLine = configuration[NotificationService.Common.Constants.ServiceLineConfigName],
                ServiceOffering = configuration[NotificationService.Common.Constants.ServiceOfferingConfigName],
                ComponentId = configuration[NotificationService.Common.Constants.ComponentIdConfigName],
                ComponentName = configuration[NotificationService.Common.Constants.ComponentNameConfigName],
                EnvironmentName = configuration[NotificationService.Common.Constants.EnvironmentName],
                IctoId = "IctoId",
            };
            itm[0] = envInitializer;
            AzureKeyVaultConfigurationOptions azureKeyVaultConfigurationOptions = new AzureKeyVaultConfigurationOptions(
               configuration["KeyVault:SecretUri"],
               configuration["KeyVault:ClientId"],
               configuration["KeyVault:ClientSecret"])
            {
                ReloadInterval = TimeSpan.FromSeconds(double.Parse(configuration[Constants.KeyVaultConfigRefreshDurationSeconds])),
            };
            _ = configBuilder.AddAzureKeyVault(azureKeyVaultConfigurationOptions);
            configuration = configBuilder.Build();
            LoggingConfiguration loggingConfiguration = new LoggingConfiguration
            {
                IsTraceEnabled = true,
                TraceLevel = (SeverityLevel)Enum.Parse(typeof(SeverityLevel), configuration["ApplicationInsights:TraceLevel"]),
                EnvironmentName = configuration[NotificationService.Common.Constants.EnvironmentName],
            };

            var tconfig = TelemetryConfiguration.CreateDefault();
            tconfig.InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];

            DependencyTrackingTelemetryModule depModule = new DependencyTrackingTelemetryModule();
            depModule.Initialize(tconfig);

            RequestTrackingTelemetryModule requestTrackingTelemetryModule = new RequestTrackingTelemetryModule();
            requestTrackingTelemetryModule.Initialize(tconfig);

            _ = builder.Services.AddSingleton<ILogger>(_ => new AILogger(loggingConfiguration, tconfig, itm));
            _ = builder.Services.Configure<CosmosDBSetting>(configuration.GetSection("CosmosDB"));
            _ = builder.Services.Configure<CosmosDBSetting>(s => s.Key = configuration["CosmosDBKey"]);
            _ = builder.Services.Configure<CosmosDBSetting>(s => s.Uri = configuration["CosmosDBURI"]);
            _ = builder.Services.Configure<StorageAccountSetting>(configuration.GetSection("StorageAccount"));
            _ = builder.Services.Configure<StorageAccountSetting>(s => s.ConnectionString = configuration["StorageAccountConnectionString"]);
            _ = builder.Services.AddSingleton<IConfiguration>(configuration);
            _ = builder.Services.AddScoped<ICosmosLinqQuery, CustomCosmosLinqQuery>();
            _ = builder.Services.AddSingleton<ICosmosDBQueryClient, CosmosDBQueryClient>();
            _ = builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            _ = builder.Services.AddScoped<EmailNotificationRepository>();
            _ = builder.Services.AddScoped<IEmailNotificationRepository, EmailNotificationRepository>(s => s.GetService<EmailNotificationRepository>());
            _ = builder.Services.AddScoped<TableStorageEmailRepository>();
            _ = builder.Services.AddScoped<IEmailNotificationRepository, TableStorageEmailRepository>(s => s.GetService<TableStorageEmailRepository>());
            _ = builder.Services.AddScoped<ITableStorageClient, TableStorageClient>();
            _ = builder.Services.AddHttpClient<IHttpClientHelper, HttpClientHelper>();
        }
    }
}
