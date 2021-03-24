// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using NotificationService.Common.Configurations;
    using NUnit.Framework;
    using System;
    using System.Globalization;


    public class BaseTests
    {

        protected TokenUtility tokenUtility;
        private ServiceProvider serviceProvider;
        protected IConfiguration Configuration;

        [SetUp]
        protected void SetupTestBase()
        {
            Configuration = InitConfiguration();

            var services = new ServiceCollection();

            _ = services.AddOptions();
            _ = services.AddSingleton<IConfiguration>(Configuration);
            _ = services.AddScoped<TokenUtility>();

            serviceProvider = services.BuildServiceProvider();

            // create a userBusiness object with DI    
            tokenUtility = serviceProvider.GetRequiredService<TokenUtility>();
        }

        public static IConfiguration InitConfiguration()
        {
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            var config = builder.Build();
            AzureKeyVaultConfigurationOptions azureKeyVaultConfigurationOptions = new AzureKeyVaultConfigurationOptions(
                config[ConfigConstants.KeyVaultUrlConfigKey])
            {
                ReloadInterval = TimeSpan.FromSeconds(double.Parse(config[ConfigConstants.KeyVaultConfigRefreshDurationSeconds], CultureInfo.InvariantCulture)),
            };
            _ = builder.AddAzureKeyVault(azureKeyVaultConfigurationOptions);

            IConfiguration Configuration = builder.Build();

            _ = builder.AddAzureAppConfiguration(options =>
            {
                var settings = options.Connect(Configuration[ConfigConstants.AzureAppConfigConnectionstringConfigKey]).Select(KeyFilter.Any, "Common")
                .Select(KeyFilter.Any, "Handler")
                .Select(KeyFilter.Any, "Service")
                .Select(KeyFilter.Any, "QueueProcessor");
                _ = settings.ConfigureRefresh(refreshOptions =>
                {
                    _ = refreshOptions.Register(key: Configuration[ConfigConstants.ForceRefreshConfigKey], refreshAll: true, label: LabelFilter.Null);
                });
            });

            Configuration = builder.Build();
            return Configuration;
        }

    }
}