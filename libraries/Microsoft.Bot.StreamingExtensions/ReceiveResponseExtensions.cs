using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions
{
    public static class ReceiveResponseExtensions
    {
        public static T ReadBodyAsJson<T>(this ReceiveResponse response)
        {
            var contentStream = response.Streams?.FirstOrDefault();
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

        public static string ReadBodyAsString(this ReceiveResponse response)
        {
            var contentStream = response.Streams?.FirstOrDefault();

            if (contentStream != null)
            {
                return contentStream.GetStream().ReadAsUtf8String();
            }

            return null;
        }

        public static async Task<string> ReadBodyAsStringAsync(this ReceiveResponse response)
        {
            var contentStream = response.Streams?.FirstOrDefault();

            if (contentStream != null)
            {
                return await contentStream.GetStream().ReadAsUtf8StringAsync().ConfigureAwait(false);
            }

            return null;
        }
    }
}
