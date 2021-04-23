// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationHandler
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using NotificationService.BusinessLibrary;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Utilities;
    using NotificationService.Common;
    using NotificationService.Common.Configurations;
    using NotificationService.Common.Logger;
    using NotificationService.Data;
    using NotificationService.Data.Interfaces;
    using NotificationService.SvCommon.Common;

    /// <summary>
    /// Startup configuration for the service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : StartupCommon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup()
            : base("Handler")
        {
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">An instance of <see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">An instance of <see cref="IWebHostEnvironment"/>.</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ConfigureCommon(app, env);

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
            });

            _ = app.UseSwagger();
            _ = app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationHandler");
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.ConfigureServicesCommon(services);

            _ = services.AddMvc();
            _ = services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NotificationHandler", Version = "v1" });
            });

            _ = services.AddScoped<IEmailManager, EmailManager>(s =>
                    new EmailManager(
                        this.Configuration,
                        s.GetService<IRepositoryFactory>(),
                        s.GetService<ILogger>(),
                        s.GetService<IMailTemplateManager>(),
                        s.GetService<ITemplateMerge>()))
                .AddScoped<IEmailHandlerManager, EmailHandlerManager>(s =>
                    new EmailHandlerManager(
                        this.Configuration,
                        Options.Create(this.Configuration.GetSection(ConfigConstants.MSGraphSettingConfigSectionKey).Get<MSGraphSetting>()),
                        s.GetService<ICloudStorageClient>(),
                        s.GetService<ILogger>(),
                        s.GetService<IEmailManager>()));
            _ = services.AddScoped<ITemplateMerge, TemplateMerge>()
                .AddScoped<INotificationReportManager, NotificationReportManager>();
        }
    }
}
