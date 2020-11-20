// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.SvCommon.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Azure.Identity;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NotificationService.BusinessLibrary;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Encryption;
    using NotificationService.Common.Logger;
    using NotificationService.Data;
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
        public StartupCommon()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var config = builder.Build();
            AzureKeyVaultConfigurationOptions azureKeyVaultConfigurationOptions = new AzureKeyVaultConfigurationOptions(
                config["KeyVault:SecretUri"])
            {
                ReloadInterval = TimeSpan.FromSeconds(double.Parse(config[Constants.KeyVaultConfigRefreshDurationSeconds], CultureInfo.InvariantCulture)),
            };
            _ = builder.AddAzureKeyVault(azureKeyVaultConfigurationOptions);

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
                configure.AddPolicy(Constants.AppNameAuthorizePolicy, policy =>
                {
                    policy.Requirements.Add(new AppNameAuthorizeRequirement());
                });
                configure.AddPolicy(Constants.AppAudienceAuthorizePolicy, policy =>
                {
                    policy.Requirements.Add(new AppAudienceAuthorizeRequirement());
                });
            });
            _ = services.AddSingleton<IAuthorizationHandler, AppNameAuthorizePolicyHandler>(s => new AppNameAuthorizePolicyHandler(s.GetService<IHttpContextAccessor>(), this.Configuration));
            _ = services.AddSingleton<IAuthorizationHandler, AppAudienceAuthorizePolicyHandler>(s => new AppAudienceAuthorizePolicyHandler(s.GetService<IHttpContextAccessor>(), this.Configuration));

            _ = services.AddControllers();

            _ = services.AddApplicationInsightsTelemetry();
            _ = services.AddScoped(typeof(ValidateModelAttribute));

            _ = services.AddOptions();
            _ = services.Configure<MSGraphSetting>(this.Configuration.GetSection("MSGraphSetting"));
            _ = services.Configure<MSGraphSetting>(s => s.ClientCredential = this.Configuration["MSGraphSettingClientCredential"]);
            _ = services.Configure<MSGraphSetting>(s => s.ClientId = this.Configuration["MSGraphSettingClientId"]);
            _ = services.Configure<CosmosDBSetting>(this.Configuration.GetSection("CosmosDB"));
            _ = services.Configure<CosmosDBSetting>(s => s.Key = this.Configuration["CosmosDBKey"]);
            _ = services.Configure<CosmosDBSetting>(s => s.Uri = this.Configuration["CosmosDBURI"]);
            _ = services.Configure<StorageAccountSetting>(this.Configuration.GetSection("StorageAccount"));
            _ = services.Configure<StorageAccountSetting>(s => s.ConnectionString = this.Configuration["StorageAccountConnectionString"]);
            _ = services.Configure<UserTokenSetting>(this.Configuration.GetSection("UserTokenSetting"));
            _ = services.Configure<RetrySetting>(this.Configuration.GetSection("RetrySetting"));
            _ = services.AddDataProtection()
                .ProtectKeysWithAzureKeyVault(new Uri("https://tempcsp-vault.vault.azure.net/keys/cspRSA/5eed852d7ce046239eb1dd996edfab53"), new DefaultAzureCredential())
                .PersistKeysToAzureBlobStorage(new Uri("https://fvinstorage.blob.core.windows.net/csp/key?sp=racwdl&st=2020-11-03T09:37:34Z&se=2020-11-04T09:37:34Z&sv=2019-12-12&sr=c&sig=4ChA2xCcZPGFxJgnzNbh9Rak2j2Y%2FdW6UrxgASAF3Zo%3D"));

            // _ = services.AddSingleton<IConfiguration>(this.Configuration);
            _ = services.AddSingleton<IEncryptionService, EncryptionService>();

            _ = services.AddTransient<IHttpContextAccessor, HttpContextAccessor>()
                .AddScoped<ICosmosLinqQuery, CustomCosmosLinqQuery>()
                .AddSingleton<ICosmosDBQueryClient, CosmosDBQueryClient>()
                .AddSingleton<ICloudStorageClient, CloudStorageClient>()
                .AddScoped<ITokenHelper, TokenHelper>()
                .AddHttpClient<IMSGraphProvider, MSGraphProvider>();

            _ = services.AddHttpContextAccessor();

            _ = services.AddAuthentication("Bearer").AddJwtBearer(options =>
            {
                options.Authority = this.Configuration["BearerTokenAuthentication:Authority"];
                options.ClaimsIssuer = this.Configuration["BearerTokenAuthentication:Issuer"];
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidAudiences = this.Configuration["BearerTokenAuthentication:ValidAudiences"].Split(Constants.SplitCharacter),
                };
            });

            ITelemetryInitializer[] itm = new ITelemetryInitializer[1];
            var envInitializer = new EnvironmentInitializer
            {
                Service = this.Configuration[Constants.ServiceConfigName],
                ServiceLine = this.Configuration[Constants.ServiceLineConfigName],
                ServiceOffering = this.Configuration[Constants.ServiceOfferingConfigName],
                ComponentId = this.Configuration[Constants.ComponentIdConfigName],
                ComponentName = this.Configuration[Constants.ComponentNameConfigName],
                EnvironmentName = this.Configuration[Constants.EnvironmentName],
                IctoId = "IctoId",
            };
            itm[0] = envInitializer;

            LoggingConfiguration loggingConfiguration = new LoggingConfiguration
            {
                IsTraceEnabled = true,
                TraceLevel = (SeverityLevel)Enum.Parse(typeof(SeverityLevel), this.Configuration["ApplicationInsights:TraceLevel"]),
                EnvironmentName = this.Configuration[Constants.EnvironmentName],
            };

            var tconfig = TelemetryConfiguration.CreateDefault();
            tconfig.InstrumentationKey = this.Configuration["ApplicationInsights:InstrumentationKey"];

            DependencyTrackingTelemetryModule depModule = new DependencyTrackingTelemetryModule();
            depModule.Initialize(tconfig);

            RequestTrackingTelemetryModule requestTrackingTelemetryModule = new RequestTrackingTelemetryModule();
            requestTrackingTelemetryModule.Initialize(tconfig);

            _ = services.AddSingleton<ILogger>(_ => new AILogger(loggingConfiguration, tconfig, itm));
        }
    }
}
