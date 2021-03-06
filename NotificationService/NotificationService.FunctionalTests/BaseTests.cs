// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.FunctionalTests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    /// <summary>
    /// Base class for all functional tests
    /// </summary>

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

            IConfiguration Configuration = builder.Build();


            return Configuration;
        }

    }
}