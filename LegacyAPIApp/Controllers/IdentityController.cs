using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace LegacyAPIApp.Controllers
{
    public class IdentityController : ApiController
    {
        // GET api/<controller>
        public ((string key, string value)[] headers, (string key, string value)[] claims) GetHeadersAndClaims()
        {
            return (Request.Headers.Select(x => (x.Key, String.Join(", ", x.Value))).ToArray(),
                    ClaimsPrincipal.Current?.Claims.Select(x => (x.Type, x.Value)).ToArray());
        }
    }
}