using System.Web;
using System.Web.Http;

namespace Microsoft.Bot.Sample.RealEstateBot
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
