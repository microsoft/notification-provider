// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Providers
{
    using System;
    using NotificationService.BusinessLibrary.Interfaces;

    /// <summary>
    /// Notification Provider Factory.
    /// </summary>
    public class NotificationProviderFactory : INotificationProviderFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProviderFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/> .</param>
        public NotificationProviderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public INotificationProvider GetNotificationProvider(NotificationProviderType type)
        {
            switch (type)
            {
                case NotificationProviderType.Graph:
                    return (INotificationProvider)this.serviceProvider.GetService(typeof(MSGraphNotificationProvider));
                case NotificationProviderType.DirectSend:
                    return (INotificationProvider)this.serviceProvider.GetService(typeof(DirectSendNotificationProvider));
                case NotificationProviderType.SMTP:
                    return (INotificationProvider)this.serviceProvider.GetService(typeof(SMTPNotificationProvider));
                default:
                    return null;
            }
        }
    }
}
