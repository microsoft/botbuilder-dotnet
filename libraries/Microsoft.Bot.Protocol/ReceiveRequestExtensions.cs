using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol
{
    public static class ReceiveRequestExtensions
    {        
        public static async Task<T> ReadBodyAsJson<T>(this ReceiveRequest request)
        {
            IContentStream contentStream = request.Streams?.FirstOrDefault();
            if (contentStream != null)
            {
                var stream = contentStream.GetStream();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        var serializer = JsonSerializer.Create(SerializationSettings.DefaultDeserializationSettings);
                        return serializer.Deserialize<T>(jsonReader);
                    }
                }
            }
            return default(T);
        }

        public static async Task<string> ReadBodyAsString(this ReceiveRequest request)
        {
            IContentStream contentStream = request.Streams?.FirstOrDefault();
            if (contentStream != null)
            {
                var stream = contentStream.GetStream();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            return null;
        }
    }
}
