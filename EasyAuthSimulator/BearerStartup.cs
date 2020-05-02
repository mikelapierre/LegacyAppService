using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace EasyAuthSimulator
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

    public class BearerStartup
    {
        public void Configuration(IAppBuilder app)
        {
            Common.HandleBindingRedirects();
            Initialize(app);
        }

        private void Initialize(IAppBuilder app)
        {
            var owinHandler = typeof(Microsoft.Owin.Host.SystemWeb.OwinHttpHandler); // Hack to force strong reference

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                Provider = new OAuthBearerAuthenticationProvider(),
                AccessTokenFormat = new JwtFormat(
                    new TokenValidationParameters()
                    {
                        ValidAudience = Common.Audience,
                        ValidateIssuer = true
                    }, 
                    new OpenIdConnectCachingSecurityTokenProvider($"https://login.microsoftonline.com/{Common.TenantID}/.well-known/openid-configuration"))
            }); ;    
            
            app.Use(async (context, next) => {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    throw new UnauthorizedAccessException("You must use bearer authentication to call this service");
                }
                else
                {                    
                    var claims = (await context.Authentication.AuthenticateAsync("Bearer")).Identity.Claims;
                    var principal = new MsClientPrincipal
                    {
                        Claims = claims.Select(c => new UserClaim() { Type = c.Type, Value = c.Value })
                    };
                    var jsonPrincipal = JsonConvert.SerializeObject(principal);
                    var encodedPrincipal = Encoding.Default.GetBytes(jsonPrincipal);
                    var base64Principal = Convert.ToBase64String(encodedPrincipal);
                    HttpContext.Current.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL", base64Principal);
                    if (HttpContext.Current.Items.Contains("MS_HttpRequestMessage"))
                    {
                        HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
                        httpRequestMessage.Headers.Add("X-MS-CLIENT-PRINCIPAL", base64Principal);
                    }
                }
                await next();
            });
        }
    }
}
