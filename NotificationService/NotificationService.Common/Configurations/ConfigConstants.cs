namespace NotificationService.Common.Configurations
{
    /// <summary>
    /// Configuration Constants class.
    /// </summary>
    public static class ConfigConstants
    {
        /// <summary>
        /// A constant for application accounts config section key.
        /// </summary>
        public const string ApplicationAccountsConfigSectionKey = "ApplicationAccounts";

        /// <summary>
        /// Seconds to wait between attempts at polling the Azure KeyVault for changes in configuration.
        /// </summary>
        public const string KeyVaultConfigRefreshDurationSeconds = "KeyVaultConfigRefreshDurationSeconds";

        /// <summary>
        /// The notification encryption key.
        /// </summary>
        public const string NotificationEncryptionKey = "NotificationEncryptionKey";

        /// <summary>
        /// The notification encryption intial vector.
        /// </summary>
        public const string NotificationEncryptionIntialVector = "NotificationEncryptionIntialVector";

        /// <summary>
        /// A constant for StorageAccount config-section key from appsetting.json.
        /// </summary>
        public const string StorageAccountConfigSectionKey = "StorageAccount";

        /// <summary>
        /// A constant for StorageAccountConnectionString config key from appsetting.json.
        /// </summary>
        public const string StorageAccountConnectionStringConfigKey = "StorageAccountConnectionString";

        /// <summary>
        /// A constant for MailSetting config key from appsetting.json.
        /// </summary>
        public const string MailSettingsConfigKey = "MailSettings";

        /// <summary>
        /// A constant for RetrySetting config-section key from appsetting.json.
        /// </summary>
        public const string RetrySettingConfigSectionKey = "RetrySetting";

        /// <summary>
        /// A constant for MaxRetryCout config key from appsetting.json.
        /// </summary>
        public const string RetrySettingMaxRetryCountConfigKey = "MaxRetries";

        /// <summary>
        /// A constant for MSGraphSetting config section key from appsetting.json.
        /// </summary>
        public const string MSGraphSettingConfigSectionKey = "MSGraphSetting";

        /// <summary>
        /// A constant for MSGraphSettingClientCredentials config key from appsetting.json.
        /// </summary>
        public const string MSGraphSettingClientCredentialConfigKey = "MSGraphSettingClientCredential";

        /// <summary>
        /// A constant for MSGraphSettingClientId config key from appsetting.json.
        /// </summary>
        public const string MSGraphSettingClientIdConfigKey = "MSGraphSettingClientId";

        /// <summary>
        /// A constant for CosmosDB config section key from appsetting.json.
        /// </summary>
        public const string CosmosDBConfigSectionKey = "CosmosDB";

        /// <summary>
        /// A constant for ConsmosDBKey config key from appsetting.json.
        /// </summary>
        public const string CosmosDBKeyConfigKey = "CosmosDBKey";

        /// <summary>
        /// A constant for CosmosDBURI config key from appsetting.json.
        /// </summary>
        public const string CosmosDBURIConfigKey = "CosmosDBURI";

        /// <summary>
        /// A constant for UserTokenSetting config section key from appsetting.json.
        /// </summary>
        public const string UserTokenSettingConfigSectionKey = "UserTokenSetting";

        /// <summary>
        /// A constant for Keyvault RSAKeyURI config key from appsetting.json.
        /// </summary>
        public const string KeyVaultRSAUriConfigKey = "KeyVault:RSAKeyUri";

        /// <summary>
        /// A constant for Authority config key from appsetting.json.
        /// </summary>
        public const string AuthorityConfigKey = "Authority";

        /// <summary>
        /// A constant for BearerTokenAuthentication config section key from appsetting.json.
        /// </summary>
        public const string BearerTokenConfigSectionKey = "BearerTokenAuthentication";

        /// <summary>
        /// A constant for A config section key from appsetting.json.
        /// </summary>
        public const string AIConfigSectionKey = "ApplicationInsights";

        /// <summary>
        /// A constant for ForceRefresh config key from appsetting.json.
        /// </summary>
        public const string ForceRefreshConfigKey = "AppConfig:ForceRefresh";

        /// <summary>
        /// A constant for KeyVaultUrl config key from appsetting.json.
        /// </summary>
        public const string KeyVaultUrlConfigKey = "KeyVaultUrl";

        /// <summary>
        /// A constant for MaxDequeueCount config key from appsetting.json.
        /// </summary>
        public const string MaxDequeueCountConfigKey = "AzureFunctionsJobHost:extensions:queues:maxDequeueCount";

        /// <summary>
        /// A constant for DirectSendSetting config section key from appsetting.json.
        /// </summary>
        public const string DirectSendSettingConfigSectionKey = "DirectSendSetting";

        /// <summary>
        /// A constant for resend allowed days.
        /// </summary>
        public const string AllowedMaxResendDurationInDays = "AllowedMaxResendDurationInDays";

        /// <summary>
        /// A constact for StorageAccount notification queue. 
        /// </summary>
        public const string StorageAccNotificationQueueName = "NotificationQueueName";

        /// <summary>
        /// StorageType.
        /// </summary>
        public const string StorageType = "StorageType";

        /// <summary>
        /// NotificationProviderType.
        /// </summary>
        public const string NotificationProviderType = "NotificationProviderType";

        /// <summary>
        /// A constant for BearerTokenAuthenticationIssuer config key from appsetting.json.
        /// </summary>
        public static string BearerTokenIssuerConfigKey = $"{BearerTokenConfigSectionKey}:Issuer";

        /// <summary>
        /// A constant for BearerTokenAuthenticationValidAudiences config key from appsetting.json.
        /// </summary>
        public static string BearerTokenValidAudiencesConfigKey = $"{BearerTokenConfigSectionKey}:ValidAudiences";

        /// <summary>
        /// A constant for AI Tracelevel config key from appsetting.json.
        /// </summary>
        public static string AITraceLelelConfigKey = $"{AIConfigSectionKey}:TraceLevel";

        /// <summary>
        /// A constant for AI InstrumentationKey config key from appsetting.json.
        /// </summary>
        public static string AIInsrumentationConfigKey = $"{AIConfigSectionKey}:InstrumentationKey";

        /// <summary>
        /// A constant for AzureAppConfigConnectionstring config key from appsetting.json.
        /// </summary>
        public const string AzureAppConfigConnectionstringConfigKey = "AzureAppConfigConnectionstring";

        /// <summary>
        /// A constant for DirectSend DisplayName config key from appsetting.json.
        /// </summary>
        public static string DirectSendFromAddressDisplayNameConfigKey = $"{DirectSendSettingConfigSectionKey}:FromAddressDisplayName";

        /// <summary>
        /// A constant for DirectSend SMTPPort config key from appsetting.json.
        /// </summary>
        public static string DirectSendSMTPPortConfigKey = $"{DirectSendSettingConfigSectionKey}:SmtpPort";

        /// <summary>
        /// A constant for DIrectSend SMTPServer config key from appsetting.json.
        /// </summary>
        public static string DirectSendSMTPServerConfigKey = $"{DirectSendSettingConfigSectionKey}:SmtpServer";
    }
}
