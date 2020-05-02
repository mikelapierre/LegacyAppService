using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace LegacyWebApp
{
    internal static class Common
    {
        internal const string bootstrapTable = @"<h2>{0}</h2>
                                                <table class=""table"">
                                                <thead>
                                                  <tr>
                                                    <th>Key</th>
                                                    <th>Value</th>
                                                  </tr>
                                                </thead>
                                                <tbody>
                                                  {1}
                                                </tbody>
                                              </table>";

        internal static string TenantID
        {
            get { return GetSetting("TenantID"); }
        }

        internal static string ClientID
        {
            get { return GetSetting("ClientID"); }
        }

        internal static string ClientSecret
        {
            get { return GetSetting("ClientSecret"); }
        }

        internal static string ASMXAppURI
        {
            get { return GetSetting("ASMXAppURI"); }
        }

        internal static string ASMXURL
        {
            get { return GetSetting("ASMXURL"); }
        }

        internal static string WCFAppURI
        {
            get { return GetSetting("WCFAppURI"); }
        }

        internal static string WCFURL
        {
            get { return GetSetting("WCFURL"); }
        }

        internal static string WebAPIAppURI
        {
            get { return GetSetting("WebAPIAppURI"); }
        }

        internal static string WebApiURL
        {
            get { return GetSetting("WebApiURL"); }
        }

        private readonly static Lazy<JObject> UserSettings = new Lazy<JObject>(() =>
        {
            var settingsFile = HttpContext.Current.Server.MapPath("settings.user");
            if (File.Exists(settingsFile))
            {
                return JObject.Parse(File.ReadAllText(settingsFile));
            }
            return new JObject();
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
    }
}