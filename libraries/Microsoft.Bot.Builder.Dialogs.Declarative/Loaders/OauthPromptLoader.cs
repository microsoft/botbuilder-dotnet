// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Loaders
{
    public class OAuthPromptLoader : ICustomDeserializer
    {
        public object Load(JToken obj, JsonSerializer serializer, Type type)
        {
            // If the OAuthSetting is inlined with the OAuthPrompt, load it here for 
            // simpler json format
            if (obj["ConnectionName"]?.Type == JTokenType.String)
            {
                var authSetting = obj.ToObject<OAuthPromptSettings>();
                var oauthPrompt = new OAuthPrompt("[OAuthPrompt]", authSetting);
                // TODO why does OAuthPrompt have a Property?
                // oauthPrompt.Property = obj["Property"]?.ToString();
                return oauthPrompt;
            }

            // Else, just assume it is the verbose structure with LuisService as inner object
            return obj.ToObject<OAuthPrompt>(serializer);
        }
    }
}
