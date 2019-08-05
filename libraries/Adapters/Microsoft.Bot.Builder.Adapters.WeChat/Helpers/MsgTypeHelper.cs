// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;

namespace Microsoft.Bot.Builder.Adapters.WeChat
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
    }
}
