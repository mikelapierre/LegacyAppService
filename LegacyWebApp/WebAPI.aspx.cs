using IdentityWebAPI;
using Microsoft.Rest;
using System;
using System.Linq;
using System.Net.Http;

namespace LegacyWebApp
{
    public class IdentityWebAPIClientWithNoCreds : IdentityWebAPI.IdentityWebAPIClient
    {
        public IdentityWebAPIClientWithNoCreds(params DelegatingHandler[] handlers) : base(handlers) { }
    }

    public partial class WebAPI : System.Web.UI.Page
    {
        protected void btnAnonymous_Click(object sender, EventArgs e)
        {
            var client = new IdentityWebAPIClientWithNoCreds();
            client.BaseUri = new Uri(Common.WebApiURL);
            GenerateTable(client.Identity.GetHeadersAndClaims());
        }

        protected void btnManagedIdentity_Click(object sender, EventArgs e)
        {
            string accessToken = AuthHelper.GetAccessTokenMI(Common.WebAPIAppURI);

            var client = new IdentityWebAPIClient(new TokenCredentials(accessToken));
            client.BaseUri = new Uri(Common.WebApiURL);
            GenerateTable(client.Identity.GetHeadersAndClaims());
        }

        protected void btnClientSecret_Click(object sender, EventArgs e)
        {
            var accessToken = AuthHelper.GetAccessToken($"{Common.WebAPIAppURI}/.default");

            var client = new IdentityWebAPIClient(new TokenCredentials(accessToken));
            client.BaseUri = new Uri(Common.WebApiURL);
            GenerateTable(client.Identity.GetHeadersAndClaims());
        }

        protected void btnOnBehalfOf_Click(object sender, EventArgs e)
        {
            var accessToken = AuthHelper.GetAccessTokenOBO(new string[] { $"{Common.WebAPIAppURI}/user_impersonation" }, Context.Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"]);

            var client = new IdentityWebAPIClient(new TokenCredentials(accessToken));
            client.BaseUri = new Uri(Common.WebApiURL);
            GenerateTable(client.Identity.GetHeadersAndClaims());
        }

        private void GenerateTable(IdentityWebAPI.Models.ValueTupleValueTuple2SequenceValueTuple2Sequence headersAndClaims)
        {
            var headersTable = String.Format(Common.bootstrapTable, "Headers", String.Join(String.Empty, headersAndClaims.Item1.Select(x => $"<tr><td>{x.Item1}</td><td>{x.Item2}</td></tr>")));
            var claimsTable = String.Format(Common.bootstrapTable, "Claims", String.Join(String.Empty, headersAndClaims.Item2.Select(x => $"<tr><td>{x.Item1}</td><td>{x.Item2}</td></tr>")));
            lblResults.Text = headersTable + claimsTable;
        }
    }
}