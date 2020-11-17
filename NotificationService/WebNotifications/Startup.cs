// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebNotifications
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using NotificationService.BusinessLibrary.Business.V1;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.BusinessLibrary.Trackers;
    using NotificationService.Contracts.Entities.Web;
    using NotificationService.Data.Interfaces;
    using NotificationService.Data.Repositories;
    using NotificationService.SvCommon.Common;
    using WebNotifications.Hubs;
    using WebNotifications.Carriers.Interfaces;
    using WebNotifications.Carriers;
    using WebNotifications.Providers;
    using WebNotifications.Channels.Internals;
    using WebNotifications.Channels;
    using WebNotifications.BackgroundServices;

    /// <summary>
    /// The <see cref="Startup"/> class configures middlwares in the web application.
    /// </summary>
    /// <seealso cref="NotificationService.SvCommon.Common.StartupCommon" />
    [ExcludeFromCodeCoverage]
    public class Startup : StartupCommon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup()
            : base()
        {
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebNotifications", Version = "v1" });
            });
            _ = services.AddScoped<INotificationsManager, NotificationsManager>()
                .AddScoped<INotificationDelivery, NotificationsManager>()
                .AddScoped<IRepository<WebNotificationItemEntity>, WebNotificationItemEntityRepository>();
            _ = services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = this.Configuration["hrgta-notifications-redisconnectionstring"];
            });
            _ = services.AddSignalR().AddAzureSignalR(this.Configuration["hrgta-notifications-signalrconnectionstring"]);
            _ = services.AddSingleton<IWebNotificationsCarrier, WebNotificationsCarrier>();
            _ = services.AddSingleton<IUserIdProvider, UserObjectIdentifierProvider>();
            _ = services.AddSingleton<UserConnectionCacheTracker>();
            _ = services.AddSingleton<IUserConnectionTracker>(svcp => (IUserConnectionTracker)svcp.GetRequiredService<UserConnectionCacheTracker>());
            _ = services.AddSingleton<IUserConnectionsReader>(svcp => (IUserConnectionsReader)svcp.GetRequiredService<UserConnectionCacheTracker>());
            _ = services.AddSingleton<IChannelProvider, ChannelProvider>();
            _ = services.AddSingleton<INotificationsChannel, NotificationsChannel>();
            _ = services.AddHostedService<NotificationsCarrierService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">An instance of <see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">An instance of <see cref="IWebHostEnvironment"/>.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app.UseCors(builder =>
            {
                string[] origins = this.Configuration["AllowedOrigins"].Split(',', StringSplitOptions.RemoveEmptyEntries);
                builder.AllowAnyMethod().WithOrigins(origins)
                    .AllowAnyHeader().AllowCredentials();
            });
            ConfigureCommon(app, env);
            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
                _ = endpoints.MapHub<NotificationsHub>("/notificationshub");
            });

            _ = app.UseSwagger();
            _ = app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebNotifications");
            });
        }
    }
}
