using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Identity.Client;

namespace LegacyWebApp
{
    internal static class AuthHelper
    {
        internal static string GetAccessToken(string scope)
        {
            var app = ConfidentialClientApplicationBuilder.Create(Common.ClientID)
                           .WithClientSecret(Common.ClientSecret)
                           .WithTenantId(Common.TenantID) 
                           .Build();
            var scopes = new string[] { scope };
            return app.AcquireTokenForClient(scopes).ExecuteAsync().GetAwaiter().GetResult().AccessToken;
        }

        internal static string GetAccessTokenMI(string scope)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            return azureServiceTokenProvider.GetAccessTokenAsync(scope, Common.TenantID).GetAwaiter().GetResult();
        }

        internal static string GetAccessTokenOBO(string[] scopes, string assertion)
        {
            var app = ConfidentialClientApplicationBuilder.Create(Common.ClientID)
                           .WithClientSecret(Common.ClientSecret)
                           .WithTenantId(Common.TenantID)
                           .Build();
            var userAssertion = new UserAssertion(assertion, "urn:ietf:params:oauth:grant-type:jwt-bearer");
            return app.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync().GetAwaiter().GetResult().AccessToken;
        }
    }
}