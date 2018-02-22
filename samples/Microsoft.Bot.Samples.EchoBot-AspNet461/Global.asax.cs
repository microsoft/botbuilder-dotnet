using System.Web.Http;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
