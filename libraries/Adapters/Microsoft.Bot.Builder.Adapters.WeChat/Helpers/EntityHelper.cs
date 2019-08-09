// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Helpers
{
    /// <summary>
    /// Entity related helper class.
    /// </summary>
    internal static class EntityHelper
    {
        /// <summary>
        /// Get the request message entity from XML.
        /// </summary>
        /// <typeparam name="T">Type of IRequestMessageBase.</typeparam>
        /// <param name="doc">The XDocument.</param>
        /// <returns>Request Message Entity.</returns>
        public static IRequestMessageBase FillEntityWithXml<T>(XDocument doc)
            where T : IRequestMessageBase, new()
        {
            try
            {
                var requestMessage = new T();
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = doc.CreateReader())
                {
                    requestMessage = (T)serializer.Deserialize(reader);
                }

                return requestMessage;
            }
            catch (Exception e)
            {
                throw new Exception("Deserialize XDocument failed.", e);
            }
        }

        /// <summary>
        /// Convert Entity to XML string.
        /// </summary>
        /// <typeparam name="T">Type of Entity class.</typeparam>
        /// <param name="responseMessage">Request Message Interface.</param>
        /// <returns>XML String.</returns>
        public static string ConvertEntityToXmlString<T>(IResponseMessageBase responseMessage)
            where T : class
        {
            try
            {
                var entity = responseMessage as T;

                var serializer = new XmlSerializer(typeof(T));

                var settings = new XmlWriterSettings
                {
                    Encoding = new UnicodeEncoding(false, false),
                    Indent = true,
                    OmitXmlDeclaration = true,
                };
                var nameSpace = new XmlSerializerNamespaces();
                nameSpace.Add(string.Empty, string.Empty);

                using (var textWriter = new StringWriter())
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
                throw new Exception("Serialize WeChat response message failed.", e);
            }
        }
    }
}
