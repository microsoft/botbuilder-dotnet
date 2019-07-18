using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Helpers
{
    public static class EntityHelper
    {
        public static IRequestMessageBase FillEntityWithXml<T>(XDocument doc)
            where T : IRequestMessageBase, new()
        {
            try
            {
                var requestMessage = new T();
                XmlSerializer serializer = new XmlSerializer(typeof(T));
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

        public static string ConvertEntityToXmlString<T>(IResponseMessageBase responseMessage, TextWriter textWriter = null)
            where T : class
        {
            try
            {
                var entity = responseMessage as T;

                XmlSerializer serializer = new XmlSerializer(typeof(T));

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Encoding = new UnicodeEncoding(false, false),
                    Indent = true,
                    OmitXmlDeclaration = true,
                };
                var nameSpace = new XmlSerializerNamespaces();
                nameSpace.Add(string.Empty, string.Empty);

                using (textWriter = textWriter ?? new StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                    {
                        serializer.Serialize(xmlWriter, entity, nameSpace);
                        return textWriter.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Serialize Error", e);
            }
        }
    }
}
