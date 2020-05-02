using Swashbuckle.Application;
using System;
using System.Web.Http;

namespace LegacyAPIApp
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration
                .EnableSwagger(c => c.SingleApiVersion("v1", "LegacyAPIApp"))
                .EnableSwaggerUi();
        }
    }
}