using System.Linq;
using System.Security.Claims;
using System.Web.Services;

namespace LegacyASMXApp
{
    /// <summary>
    /// Summary description for IdentityService
    /// </summary>
    [WebService(Namespace = "http://legacyasmxapp/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class IdentityService : System.Web.Services.WebService
    {
        [WebMethod]
        public ((string key, string value)[] headers, (string key, string value)[] claims) GetHeadersAndClaims()
        {
            return (Context.Request.Headers.AllKeys.Select(x => (x, Context.Request.Headers[x])).ToArray(), 
                    ClaimsPrincipal.Current?.Claims.Select(x => (x.Type, x.Value)).ToArray());
        }
    }
}