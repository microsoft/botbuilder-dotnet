//using System;   
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs.Debugging;
//using Microsoft.Bot.Builder.Dialogs.Declarative;
//using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
//using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
//using Microsoft.Bot.Builder.Integration.AspNet.Core;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;

//namespace Contoso.CustomAdapter
//{
//    public class CustomComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
//    {
//        private readonly IConfiguration _config;

//        public CustomComponentRegistration(IConfiguration config, IServiceProvider sp)
//        {
//            this._config = config ?? throw new ArgumentNullException(nameof(config));
//        }

//        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
//        {
//            yield return new DeclarativeType<AdventureWorksAdapter>(AdventureWorksAdapter.Kind);
//            o
//            {
//                CustomDeserializer = new ConfigTypeLoadr(config =>)
//            };
//        }
//    }
//}
