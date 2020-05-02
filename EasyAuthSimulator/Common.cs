using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace EasyAuthSimulator
{
    internal class Common
    {
        internal static string TenantID
        {
            get { return GetSetting("TenantID"); }
        }

        internal static string ClientID
        {
            get { return GetSetting("ClientID"); }
        }

        internal static string Audience
        {
            get { return GetSetting("Audience"); }
        }

        private static Lazy<JObject> UserSettings = new Lazy<JObject>(() =>
        {
            var settingsFile = HttpContext.Current.Server.MapPath("settings.user");
            if (File.Exists(settingsFile))
            {
                return JObject.Parse(File.ReadAllText(settingsFile));
            }
            throw new ConfigurationErrorsException("Please run Setup.ps1 to setup your environment and update configuration files");
        }, false);

        private static string GetSetting(string key)
        {
            var setting = ConfigurationManager.AppSettings[key];
            if (setting == null)
            {
                ConfigurationManager.AppSettings[key] = setting = UserSettings.Value[key]?.Value<string>() ??
                    throw new ConfigurationErrorsException($"The application setting {key} is not defined");
            }
            return setting;
        }

        private static readonly string[] bindingRedirects = new string[] { "Microsoft.IdentityModel.Protocols.OpenIdConnect", 
                                                                           "Microsoft.IdentityModel.Tokens", 
                                                                           "System.IdentityModel.Tokens.Jwt", 
                                                                           "Microsoft.IdentityModel.Protocols", 
                                                                           "Newtonsoft.Json" };

        internal static void HandleBindingRedirects()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
       
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = bindingRedirects.FirstOrDefault(b => args.Name.StartsWith(b));
            if (assemblyName != null)
            {
                return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            }
            return null;
        }
    }
}