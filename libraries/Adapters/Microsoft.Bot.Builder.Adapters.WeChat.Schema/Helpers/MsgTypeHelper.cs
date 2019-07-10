using System;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Helpers
{
    /// <summary>
    /// Message type helper.
    /// </summary>
    public static class MsgTypeHelper
    {
        /// <summary>
        /// Return RequestMessageType based on xml info.
        /// </summary>
        /// <param name="requestMessageDocument">Xml document.</param>
        /// <returns>Request message type string.</returns>
        public static string GetRequestMsgTypeString(XDocument requestMessageDocument)
        {
            if (requestMessageDocument == null || requestMessageDocument.Root == null || requestMessageDocument.Root.Element("MsgType") == null)
            {
                return "Unknow";
            }

            return requestMessageDocument.Root.Element("MsgType").Value;
        }

        /// <summary>
        /// Return RequestMessageType based on xml info.
        /// </summary>
        /// <param name="requestMessageDocument">Xml document.</param>
        /// <returns>Request message type.</returns>
        public static RequestMessageType GetRequestMsgType(XDocument requestMessageDocument)
        {
            return GetRequestMsgType(GetRequestMsgTypeString(requestMessageDocument));
        }

        /// <summary>
        /// Return RequestMessageType based on xml info.
        /// </summary>
        /// <param name="str">Request message type string.</param>
        /// <returns>RequestMessageType.</returns>
        public static RequestMessageType GetRequestMsgType(string str)
        {
            try
            {
                return (RequestMessageType)Enum.Parse(typeof(RequestMessageType), str, true);
            }
            catch
            {
                return RequestMessageType.Unknown;
            }
        }

        /*
        /// <summary>
        /// Return ResponseMessageType based on xml info.
        /// </summary>
        /// <param name="doc">Xml document.</param>
        /// <returns>Response message type.</returns>
        public static ResponseMessageType GetResponseMsgType(XDocument doc)
        {
            return GetResponseMsgType(doc.Root.Element("MsgType").Value);
        }

        /// <summary>
        /// Return ResponseMessageType based on xml info.
        /// </summary>
        /// <param name="str">Response message type string.</param>
        /// <returns>Response message type.</returns>
        public static ResponseMessageType GetResponseMsgType(string str)
        {
            return (ResponseMessageType)Enum.Parse(typeof(ResponseMessageType), str, true);
        }
        */
    }
}
