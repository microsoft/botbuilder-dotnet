using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Contoso.CustomAdapter
{
    internal class AdventureWorksAdapterDeserializer : ICustomDeserializer
    {
        private readonly IConfiguration _config;

        public AdventureWorksAdapterDeserializer(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            return new AdventureWorksAdapter(_config);
        }
    }
}
