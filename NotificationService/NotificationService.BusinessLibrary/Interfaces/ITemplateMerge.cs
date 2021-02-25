// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Interfaces
{
    /// <summary>
    /// Merge templates with template input tokens.
    /// </summary>
    public interface ITemplateMerge
    {
        /// <summary>
        /// Creates mailbody using template and templateData provided in notification input.
        /// </summary>
        /// <param name="templateType">template type.</param>
        /// <param name="notificationTemplate">notification template .</param>
        /// <param name="notificationInput">notification Input .</param>
        /// <returns>Email Body created using templates.</returns>
        string CreateMailBodyUsingTemplate(string templateType, string notificationTemplate, string notificationInput);
    }
}
