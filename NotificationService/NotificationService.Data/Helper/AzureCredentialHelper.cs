namespace NotificationService.Data.Helper
{
    using Azure.Identity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class AzureCredentialHelper
    {
        public static readonly bool IsDeployed = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_RUN_FROM_PACKAGE"));
        public static readonly DefaultAzureCredential AzureCredentials = GetAzureCredentials();

        private static DefaultAzureCredential GetAzureCredentials()
        {
            return new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    // Prevent deployed instances from trying things that don't work and generally take too long
                    ExcludeInteractiveBrowserCredential = IsDeployed,
                    ExcludeVisualStudioCodeCredential = IsDeployed,
                    ExcludeVisualStudioCredential = IsDeployed,
                    ExcludeAzureCliCredential = IsDeployed,
                    ExcludeManagedIdentityCredential = false,
                    ExcludeEnvironmentCredential = true,
                    ExcludeWorkloadIdentityCredential = true,
                    Retry =
                    {
				        // Reduce retries and timeouts to get faster failures
				        MaxRetries = 2,
                        NetworkTimeout = TimeSpan.FromSeconds(5),
                        MaxDelay = TimeSpan.FromSeconds(5)
                    }
                }
            );
        }
    }
}
