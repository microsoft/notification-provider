// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationService.BusinessLibrary.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Xsl;
    using Newtonsoft.Json;
    using NotificationService.BusinessLibrary.Interfaces;
    using NotificationService.Common.Logger;

    /// <summary>
    /// Merge template.
    /// </summary>
    public class TemplateMerge : ITemplateMerge
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateMerge"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public TemplateMerge(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Creates mailbody using template and templateData provided in notification input.
        /// </summary>
        /// <param name="templateType">template type.</param>
        /// <param name="notificationTemplate">mail template .</param>
        /// <param name="notificationInput">notificationInput .</param>
        /// <returns>Email Body created using templates.</returns>
        public string CreateMailBodyUsingTemplate(string templateType, string notificationTemplate, string notificationInput)
        {
            string mailBody = null;
            if (string.Equals(templateType, "XSLT", StringComparison.InvariantCultureIgnoreCase))
            {
                mailBody = this.ConvertXSLT(notificationTemplate, notificationInput);
            }
            else
            {
                mailBody = this.ConvertText(notificationTemplate, notificationInput);
            }

            return mailBody;
        }

        /// <summary>
        /// Converts Xml template.
        /// </summary>
        /// <param name="notificationTemplate">notificationTemplate .</param>
        /// /// <param name="notificationXml">notificationXml .</param>
        /// <returns>XSLT type template.</returns>
        private string ConvertXSLT(string notificationTemplate, string notificationXml)
        {
            this.logger.TraceInformation($"Started {nameof(this.ConvertXSLT)} method of {nameof(TemplateMerge)}.");

            // Initialize object
            var xmlData = new XmlDocument();

            if (!string.IsNullOrEmpty(notificationXml))
            {
                MemoryStream stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(notificationXml));
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
                XmlReader reader = XmlReader.Create(stream, xmlReaderSettings);

                // Load message data into xmlDocument
                xmlData.Load(reader);
            }

            var xslTemplate = new XslCompiledTransform();

            // Create the XsltSettings object with script disabled.
            XsltSettings xsltSettings = new XsltSettings(false, false);

            // Load xsl template into xsl transformation object
            using (var sReader = new StringReader(notificationTemplate))
            {
                using (var xmlTextReader = new XmlTextReader(sReader))
                {
                    xmlTextReader.DtdProcessing = DtdProcessing.Prohibit;
                    xslTemplate.Load(xmlTextReader, xsltSettings, null);
                }
            }

            //// Create an XsltArgumentList and add an instance of NotificationXSLTemplateHelper
            var xslArg = new XsltArgumentList();

            // Perform the transformation, results go into a string builder object
            var stringBuilder = new StringBuilder();
            using (var sReader = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
            {
                using (var xmlWriter = XmlWriter.Create(sReader, xslTemplate.OutputSettings))
                {
                    xslTemplate.Transform(xmlData, xslArg, xmlWriter, new XmlUrlResolver());
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ConvertXSLT)} method of {nameof(TemplateMerge)}.");

            // return message
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts text template.
        /// </summary>
        /// <param name="notificationTemplate">notificationTemplate .</param>
        /// <param name="notificationText">notificationText .</param>
        /// <returns>text type template.</returns>
        private string ConvertText(string notificationTemplate, string notificationText)
        {
            this.logger.TraceInformation($"Started {nameof(this.ConvertText)} method of {nameof(TemplateMerge)}.");
            if (string.IsNullOrEmpty(notificationText))
            {
                return notificationTemplate;
            }

            Dictionary<string, string> tokens = null;
            tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(notificationText);

            foreach (KeyValuePair<string, string> tokenConfiguration in tokens)
            {
                if (Regex.IsMatch(notificationTemplate, tokenConfiguration.Key))
                {
                    notificationTemplate = Regex.Replace(notificationTemplate, tokenConfiguration.Key, tokenConfiguration.Value);
                }
            }

            this.logger.TraceInformation($"Finished {nameof(this.ConvertText)} method of {nameof(TemplateMerge)}.");

            return notificationTemplate;
        }
    }
}
