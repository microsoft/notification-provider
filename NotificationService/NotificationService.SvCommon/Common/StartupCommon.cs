// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Azure.Core.Cryptography;
    using Azure.Identity;
    using Azure.Security.KeyVault.Keys.Cryptography;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Exceptions;
    using NotificationService.Common.Logger;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;
    using NotificationService.Data.Repositories;
    using NotificationService.SvCommon.Attributes;

    /// <summary>
    /// Common Startup configuration for the service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StartupCommon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupCommon"/> class.
        /// </summary>
        /// <param name="application">service/application name (Handler, Service).</param>
        public StartupCommon(string application)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var config = builder.Build();
            AzureKeyVaultConfigurationOptions azureKeyVaultConfigurationOptions = new AzureKeyVaultConfigurationOptions(
                config[ConfigConstants.KeyVaultUrlConfigKey])
            {
                ReloadInterval = TimeSpan.FromSeconds(double.Parse(config[ConfigConstants.KeyVaultConfigRefreshDurationSeconds], CultureInfo.InvariantCulture)),
            };
            _ = builder.AddAzureKeyVault(azureKeyVaultConfigurationOptions);

            this.Configuration = builder.Build();

            _ = builder.AddAzureAppConfiguration(options =>
            {
                var settings = options.Connect(this.Configuration[ConfigConstants.AzureAppConfigConnectionstringConfigKey]).Select(KeyFilter.Any, "Common")
                .Select(KeyFilter.Any, application);
                _ = settings.ConfigureRefresh(refreshOptions =>
                {
                    _ = refreshOptions.Register(key: this.Configuration[ConfigConstants.ForceRefreshConfigKey], refreshAll: true, label: LabelFilter.Null);
                });
            });

            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">An instance of <see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">An instance of <see cref="IWebHostEnvironment"/>.</param>
        public static void ConfigureCommon(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
            }

            _ = app.UseMiddleware<ExceptionMiddleware>();

            _ = app.UseHttpsRedirection();

            _ = app.UseFileServer();

            _ = app.UseRouting();

            _ = app.UseAuthentication();

            _ = app.UseAuthorization();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
        public void ConfigureServicesCommon(IServiceCollection services)
        {
            _ = services.AddAuthorization(configure =>
            {
                configure.AddPolicy(ApplicationConstants.AppNameAuthorizePolicy, policy =>
                {
                    policy.Requirements.Add(new AppNameAuthorizeRequirement());
                });
                configure.AddPolicy(ApplicationConstants.AppIdAuthorizePolicy, policy =>
                {
                    policy.Requirements.Add(new AppIdAuthorizeRequirement());
                });
            });
            _ = services.AddSingleton<IAuthorizationHandler, AppNameAuthorizePolicyHandler>(s => new AppNameAuthorizePolicyHandler(s.GetService<IHttpContextAccessor>(), this.Configuration));
            _ = services.AddSingleton<IAuthorizationHandler, AppIdAuthorizePolicyHandler>(s => new AppIdAuthorizePolicyHandler(s.GetService<IHttpContextAccessor>(), this.Configuration));

            _ = services.AddControllers();

            _ = services.AddApplicationInsightsTelemetry();
            _ = services.AddScoped(typeof(ValidateModelAttribute));
            _ = services.AddOptions();

            _ = services.Configure<StorageAccountSetting>(this.Configuration.GetSection(ConfigConstants.StorageAccountConfigSectionKey));
            _ = services.Configure<StorageAccountSetting>(s => s.ConnectionString = this.Configuration[ConfigConstants.StorageAccountConnectionStringConfigKey]);
            _ = services.Configure<UserTokenSetting>(this.Configuration.GetSection(ConfigConstants.UserTokenSettingConfigSectionKey));
            _ = services.Configure<RetrySetting>(this.Configuration.GetSection(ConfigConstants.RetrySettingConfigSectionKey));

            _ = services.AddSingleton<IConfiguration>(this.Configuration);
            _ = services.AddSingleton<IEncryptionService, EncryptionService>();
            _ = services.AddSingleton<IKeyEncryptionKey, CryptographyClient>(cc => new CryptographyClient(new Uri(this.Configuration[ConfigConstants.KeyVaultRSAUriConfigKey]), new DefaultAzureCredential()));

            _ = services.AddTransient<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<ICloudStorageClient, CloudStorageClient>()
                .AddScoped<ITokenHelper, TokenHelper>()
                .AddScoped<IRepositoryFactory, RepositoryFactory>()
                .AddSingleton<IEmailAccountManager, EmailAccountManager>();

            StorageType storageType = (StorageType)Enum.Parse(typeof(StorageType), this.Configuration?[ConfigConstants.StorageType]);

            if (storageType == StorageType.DocumentDB)
            {
                this.ConfigureCosmosDB(services);
            }

            ConfigureStorageAccountServices(services);

            _ = services.AddHttpContextAccessor();

            _ = services.AddAuthentication(ApplicationConstants.BearerAuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = this.Configuration[ConfigConstants.BearerTokenIssuerConfigKey];
                options.ClaimsIssuer = this.Configuration[ConfigConstants.BearerTokenIssuerConfigKey];
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidAudiences = this.Configuration[ConfigConstants.BearerTokenValidAudiencesConfigKey].Split(ApplicationConstants.SplitCharacter),
                };
            });

            ITelemetryInitializer[] itm = new ITelemetryInitializer[1];
            var envInitializer = new EnvironmentInitializer
            {
                Service = this.Configuration[AIConstants.ServiceConfigName],
                ServiceLine = this.Configuration[AIConstants.ServiceLineConfigName],
                ServiceOffering = this.Configuration[AIConstants.ServiceOfferingConfigName],
                ComponentId = this.Configuration[AIConstants.ComponentIdConfigName],
                ComponentName = this.Configuration[AIConstants.ComponentNameConfigName],
                EnvironmentName = this.Configuration[AIConstants.EnvironmentName],
                IctoId = "IctoId",
            };
            itm[0] = envInitializer;

            LoggingConfiguration loggingConfiguration = new LoggingConfiguration
            {
                IsTraceEnabled = true,
                TraceLevel = (SeverityLevel)Enum.Parse(typeof(SeverityLevel), this.Configuration[ConfigConstants.AITraceLelelConfigKey]),
                EnvironmentName = this.Configuration[AIConstants.EnvironmentName],
            };

#pragma warning disable CA2000 // Dispose objects before losing scope
            var tconfig = TelemetryConfiguration.CreateDefault();
#pragma warning restore CA2000 // Dispose objects before losing scope
            tconfig.InstrumentationKey = this.Configuration[ConfigConstants.AIInsrumentationConfigKey];

#pragma warning disable CA2000 // Dispose objects before losing scope
            DependencyTrackingTelemetryModule depModule = new DependencyTrackingTelemetryModule();
#pragma warning restore CA2000 // Dispose objects before losing scope
            depModule.Initialize(tconfig);

#pragma warning disable CA2000 // Dispose objects before losing scope
            RequestTrackingTelemetryModule requestTrackingTelemetryModule = new RequestTrackingTelemetryModule();
#pragma warning restore CA2000 // Dispose objects before losing scope
            requestTrackingTelemetryModule.Initialize(tconfig);

            _ = services.AddSingleton<ILogger>(_ => new AILogger(loggingConfiguration, tconfig, itm));
        }

        /// <summary>
        /// Configure storage account services.
        /// </summary>
        /// <param name="services"> IServiceCollection instance.</param>
        private static void ConfigureStorageAccountServices(IServiceCollection services)
        {
            _ = services.AddScoped<TableStorageEmailRepository>()
                .AddScoped<IEmailNotificationRepository, TableStorageEmailRepository>(s => s.GetService<TableStorageEmailRepository>())
                .AddScoped<ITableStorageClient, TableStorageClient>()
                .AddScoped<IMailTemplateManager, MailTemplateManager>()
                .AddScoped<IMailTemplateRepository, MailTemplateRepository>()
                .AddScoped<IMailAttachmentRepository, MailAttachmentRepository>();
        }

        /// <summary>
        /// Configure Cosmos DB services.
        /// </summary>
        /// <param name="services"> IServiceCollection instance.</param>
        private void ConfigureCosmosDB(IServiceCollection services)
        {
            _ = services.Configure<CosmosDBSetting>(this.Configuration.GetSection(ConfigConstants.CosmosDBConfigSectionKey));
            _ = services.Configure<CosmosDBSetting>(s => s.Key = this.Configuration[ConfigConstants.CosmosDBKeyConfigKey]);
            _ = services.Configure<CosmosDBSetting>(s => s.Uri = this.Configuration[ConfigConstants.CosmosDBURIConfigKey]);
            _ = services.AddScoped<ICosmosLinqQuery, CustomCosmosLinqQuery>()
                .AddSingleton<ICosmosDBQueryClient, CosmosDBQueryClient>()
                .AddScoped<EmailNotificationRepository>()
                .AddScoped<IEmailNotificationRepository, EmailNotificationRepository>(s => s.GetService<EmailNotificationRepository>());
        }
    }
}
