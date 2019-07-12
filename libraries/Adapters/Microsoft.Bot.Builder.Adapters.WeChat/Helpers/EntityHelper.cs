using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public static class EntityHelper
    {
        public static IRequestMessageBase FillEntityWithXml<T>(XDocument doc)
            where T : IRequestMessageBase, new()
        {
            try
            {
                var requestMessage = new T();
                var serializer = new XmlSerializer(typeof(T));
                using (TextReader reader = new StringReader(doc.ToString()))
                {
                    requestMessage = (T)serializer.Deserialize(reader);
                }

                return requestMessage;
            }
            catch (Exception e)
            {
                throw new Exception("Deserialize Error", e);
            }
        }
    }
}
