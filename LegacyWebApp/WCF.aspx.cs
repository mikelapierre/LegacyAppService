using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace LegacyWebApp
{
    public partial class WCF : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnAnonymous_Click(object sender, EventArgs e)
        {
            var client = new IdentityWCF.IdentityServiceClient("BasicHttpsBinding_IIdentityService", Common.WCFURL);
            GenerateTable(client.GetHeadersAndClaims());
        }

        protected void btnManagedIdentity_Click(object sender, EventArgs e)
        {
            string accessToken = AuthHelper.GetAccessTokenMI(Common.WCFAppURI);

            var client = new IdentityWCF.IdentityServiceClient("BasicHttpsBinding_IIdentityService", Common.WCFURL);
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = new HttpRequestMessageProperty()
                {
                    Headers = { { HttpRequestHeader.Authorization, $"Bearer {accessToken}" } }
                };
                GenerateTable(client.GetHeadersAndClaims());
            }
        }

        protected void btnClientSecret_Click(object sender, EventArgs e)
        {
            var accessToken = AuthHelper.GetAccessToken($"{Common.WCFAppURI}/.default");

            var client = new IdentityWCF.IdentityServiceClient("BasicHttpsBinding_IIdentityService", Common.WCFURL);
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = new HttpRequestMessageProperty()
                {
                    Headers = { { HttpRequestHeader.Authorization, $"Bearer {accessToken}" } }
                };
                GenerateTable(client.GetHeadersAndClaims());
            }
        }

        protected void btnOnBehalfOf_Click(object sender, EventArgs e)
        {
            var accessToken = AuthHelper.GetAccessTokenOBO(new string[] { $"{Common.WCFAppURI}/user_impersonation" }, Context.Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"]);

            var client = new IdentityWCF.IdentityServiceClient("BasicHttpsBinding_IIdentityService", Common.WCFURL);
            using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = new HttpRequestMessageProperty()
                {
                    Headers = { { HttpRequestHeader.Authorization, $"Bearer {accessToken}" } }
                };
                GenerateTable(client.GetHeadersAndClaims());
            }
        }

        private void GenerateTable(((string key, string value)[] headers, (string key, string value)[] claims) headersAndClaims)
        {
            var headersTable = String.Format(Common.bootstrapTable, "Headers", String.Join(String.Empty, headersAndClaims.Item1.Select(x => $"<tr><td>{x.Item1}</td><td>{x.Item2}</td></tr>")));
            var claimsTable = String.Format(Common.bootstrapTable, "Claims", String.Join(String.Empty, headersAndClaims.Item2.Select(x => $"<tr><td>{x.Item1}</td><td>{x.Item2}</td></tr>")));
            lblResults.Text = headersTable + claimsTable;
        }
    }
}