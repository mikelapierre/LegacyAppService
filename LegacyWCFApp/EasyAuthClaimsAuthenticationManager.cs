using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Web;

namespace LegacyWCFApp
{
    // Borrowed from https://github.com/MaximRouiller/MaximeRouiller.Azure.AppService.EasyAuth/tree/master/src

    public class UserClaim
    {
        [JsonProperty("typ")]
        public string Type { get; set; }
        [JsonProperty("val")]
        public string Value { get; set; }
    }

    public class MsClientPrincipal
    {
        [JsonProperty("auth_typ")]
        public string AuthenticationType { get; set; }
        [JsonProperty("claims")]
        public IEnumerable<UserClaim> Claims { get; set; }
        [JsonProperty("name_typ")]
        public string NameType { get; set; }
        [JsonProperty("role_typ")]
        public string RoleType { get; set; }
    }

    public class EasyAuthClaimsAuthenticationManager: ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            var msClientPrincipalEncoded = WebOperationContext.Current?.IncomingRequest?.Headers["X-MS-CLIENT-PRINCIPAL"];
            if (msClientPrincipalEncoded == null) return incomingPrincipal;

            byte[] decodedBytes = Convert.FromBase64String(msClientPrincipalEncoded);
            string msClientPrincipalDecoded = System.Text.Encoding.Default.GetString(decodedBytes);
            MsClientPrincipal clientPrincipal = JsonConvert.DeserializeObject<MsClientPrincipal>(msClientPrincipalDecoded);

            ClaimsPrincipal principal = new ClaimsPrincipal();
            IEnumerable<Claim> claims = clientPrincipal.Claims.Select(x => new Claim(x.Type, x.Value));
            principal.AddIdentity(new ClaimsIdentity(claims, clientPrincipal.AuthenticationType, clientPrincipal.NameType, clientPrincipal.RoleType));

            return principal;
        }
    }
}