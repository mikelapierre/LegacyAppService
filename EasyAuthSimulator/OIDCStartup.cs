using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace EasyAuthSimulator
{
    public class OIDCStartup
    {
        public void Configuration(IAppBuilder app)
        {            
            Common.HandleBindingRedirects();
            Initialize(app);
        }

        private void Initialize(IAppBuilder app)
        {
            var owinHandler = typeof(Microsoft.Owin.Host.SystemWeb.OwinHttpHandler); // Hack to force strong reference

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions()
            {
                ClientId = Common.ClientID,
                Authority = $"https://login.microsoftonline.com/{Common.TenantID}",
                SaveTokens = true,
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    RedirectToIdentityProvider = n => {
                        n.ProtocolMessage.RedirectUri = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Host}:{HttpContext.Current.Request.Url.Port}";
                        return Task.FromResult(0);
                    }
                },
                CookieManager = new SystemWebCookieManager()
            });
            app.Use(async (context, next) => {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    context.Authentication.Challenge();
                }
                else
                {
                    var idToken = (await context.Authentication.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationType)).Properties.Dictionary["id_token"];
                    HttpContext.Current.Request.Headers.Add("X-MS-TOKEN-AAD-ID-TOKEN", idToken);
                }
                await next();
            });
        }
    }
}
