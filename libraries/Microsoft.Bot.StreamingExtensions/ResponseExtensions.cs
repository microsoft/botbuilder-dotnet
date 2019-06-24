// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions
{
    public static class ResponseExtensions
    {
        public static void SetBody(this Response response, string body)
        {
            response.AddStream(new StringContent(body, Encoding.UTF8));
        }

        public static void SetBody(this Response response, object body)
        {
            var json = JsonConvert.SerializeObject(body, SerializationSettings.BotSchemaSerializationSettings);
            response.AddStream(new StringContent(json, Encoding.UTF8, SerializationSettings.ApplicationJson));
        }
    }
}
