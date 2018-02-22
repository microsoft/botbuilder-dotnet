using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Configuration;
using System.Web.Http;
using Unity;

namespace Microsoft.Bot.Samples.EchoBot_AspNet461
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Json settings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // services
            UnityConfig.Container.RegisterSingleton<BotFrameworkAdapter>(
                new Unity.Injection.InjectionConstructor(
                    new BotFrameworkAdapter(ConfigurationManager.AppSettings[@"MicrosoftAppId"], ConfigurationManager.AppSettings[@"MicrosoftAppPassword"])
                        .Use(new ConversationStateManagerMiddleware(new MemoryStorage()))
                )
            );
        }
    }
}
