using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol
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
