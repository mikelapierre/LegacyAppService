using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Web;

namespace LegacyWCFApp
{
    public class IdentityService : IIdentityService
    {
        public ((string key, string value)[] headers, (string key, string value)[] claims) GetHeadersAndClaims()
        {
            return (WebOperationContext.Current?.IncomingRequest?.Headers.AllKeys.Select(x => (x, WebOperationContext.Current?.IncomingRequest?.Headers[x])).ToArray(),
                    ClaimsPrincipal.Current?.Claims.Select(x => (x.Type, x.Value)).ToArray());
        }
    }
}
