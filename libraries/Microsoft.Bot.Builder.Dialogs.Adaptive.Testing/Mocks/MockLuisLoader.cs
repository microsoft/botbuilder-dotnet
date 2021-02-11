// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.Luis.Testing
{
    /// <summary>
    /// Custom json serializer for mocking luis.
    /// </summary>
    public class MockLuisLoader : ICustomDeserializer
    {
        private const string AppId = ".appId";
        private IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockLuisLoader"/> class.
        /// </summary>
        /// <param name="configuration">Configuration to use or null for no settings and caching.</param>
        public MockLuisLoader(IConfiguration configuration = null)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            var recognizer = obj.ToObject<LuisAdaptiveRecognizer>(serializer);
            var name = recognizer.ApplicationId.ToString();
            if (name.StartsWith("=", StringComparison.Ordinal))
            {
                if (name.EndsWith(AppId, StringComparison.InvariantCultureIgnoreCase))
                {
                    name = name.Substring(0, name.Length - AppId.Length);
                }

                var start = name.LastIndexOf('.') + 1;
                name = name.Substring(start);
            }

            return new MockLuisRecognizer(recognizer, _configuration == null ? Directory.GetCurrentDirectory() : _configuration.GetValue<string>("luis:resources"), name);
        }
    }
}
