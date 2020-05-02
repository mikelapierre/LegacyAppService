using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LegacyWebApp
{
    public class IdentityServiceWithHeader : IdentityASMX.IdentityService
    {
        private List<(string header, string value)> headers = new List<(string, string)>();

        public void AddHeader(string header, string value)
        {
            headers.Add((header, value));
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var req = base.GetWebRequest(uri);
            foreach (var header in headers)
            {
                req.Headers.Add(header.header, header.value);
            }
            return req;
        }
    }

    public partial class ASMX : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnAnonymous_Click(object sender, EventArgs e)
        {
            var client = new IdentityASMX.IdentityService();
            client.Url = Common.ASMXURL;
            GenerateTable(client.GetHeadersAndClaims());
        }

        protected void btnManagedIdentity_Click(object sender, EventArgs e)
        {
            string accessToken = AuthHelper.GetAccessTokenMI(Common.ASMXAppURI);

            var client = new IdentityServiceWithHeader();
            client.Url = Common.ASMXURL;
            client.AddHeader("Authorization", $"Bearer {accessToken}");
            GenerateTable(client.GetHeadersAndClaims());
        }

        protected void btnClientSecret_Click(object sender, EventArgs e)
        {
            var accessToken = AuthHelper.GetAccessToken($"{Common.ASMXAppURI}/.default");

            var client = new IdentityServiceWithHeader();
            client.Url = Common.ASMXURL;
            client.AddHeader("Authorization", $"Bearer {accessToken}");
            GenerateTable(client.GetHeadersAndClaims());
        }

        protected void btnOnBehalfOf_Click(object sender, EventArgs e)
        {
            var accessToken = AuthHelper.GetAccessTokenOBO(new string[] { $"{Common.ASMXAppURI}/user_impersonation" }, Context.Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"]);

            var client = new IdentityServiceWithHeader();
            client.Url = Common.ASMXURL;
            client.AddHeader("Authorization", $"Bearer {accessToken}");
            GenerateTable(client.GetHeadersAndClaims());
        }

        private void GenerateTable(IdentityASMX.ValueTupleOfArrayOfValueTupleOfStringStringArrayOfValueTupleOfStringString headersAndClaims)
        {
            var headersTable = String.Format(Common.bootstrapTable, "Headers", String.Join(String.Empty, headersAndClaims.Item1.Select(x => $"<tr><td>{x.Item1}</td><td>{x.Item2}</td></tr>")));
            var claimsTable = String.Format(Common.bootstrapTable, "Claims", String.Join(String.Empty, headersAndClaims.Item2.Select(x => $"<tr><td>{x.Item1}</td><td>{x.Item2}</td></tr>")));
            lblResults.Text = headersTable + claimsTable;
        }
    }
}