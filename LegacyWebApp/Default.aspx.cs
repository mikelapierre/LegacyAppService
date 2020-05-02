using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LegacyWebApp
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var headersTable = String.Format(Common.bootstrapTable, "Headers", String.Join(String.Empty, Context.Request.Headers.AllKeys.Select(x => $"<tr><td>{x}</td><td>{Context.Request.Headers[x]}</td></tr>")));
            var claimsTable = String.Format(Common.bootstrapTable, "Claims", String.Join(String.Empty, ClaimsPrincipal.Current?.Claims.Select(x => $"<tr><td>{x.Type}</td><td>{x.Value}</td></tr>")));
            lblHeadersAndClaims.Text = headersTable + claimsTable;           
        }
    }
}