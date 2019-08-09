// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.Bot.Builder.Adapters.WeChat.Helpers;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public static class WeChatMessageFactory
    {
        /// <summary>
        /// Parse XDcoument to WeChat request message.
        /// </summary>
        /// <param name="doc">The XDocument object need to be parsed.</param>
        /// <param name="logger">The logger for WeChat adapter.</param>
        /// <returns>WeChat request message.</returns>
        public static IRequestMessageBase GetRequestEntity(XDocument doc, ILogger logger)
        {
            IRequestMessageBase requestMessage = null;

            try
            {
                var msgType = GetRequestMsgTypeString(doc);
                switch (msgType)
                {
                    case RequestMessageTypes.Location:
                        requestMessage = EntityHelper.FillEntityWithXml<LocationRequest>(doc);
                        break;
                    case RequestMessageTypes.Image:
                        requestMessage = EntityHelper.FillEntityWithXml<ImageRequest>(doc);
                        break;
                    case RequestMessageTypes.Link:
                        requestMessage = EntityHelper.FillEntityWithXml<LinkRequest>(doc);
                        break;
                    case RequestMessageTypes.Text:
                        requestMessage = EntityHelper.FillEntityWithXml<TextRequest>(doc);
                        break;
                    case RequestMessageTypes.Video:
                        requestMessage = EntityHelper.FillEntityWithXml<VideoRequest>(doc);
                        break;
                    case RequestMessageTypes.Voice:
                        requestMessage = EntityHelper.FillEntityWithXml<VoiceRequest>(doc);
                        break;
                    case RequestMessageTypes.ShortVideo:
                        requestMessage = EntityHelper.FillEntityWithXml<ShortVideoRequest>(doc);
                        break;
                    case RequestMessageTypes.Event:
                        switch (doc.Root.Element("Event").Value)
                        {
                            case EventTypes.Enter:
                                requestMessage = EntityHelper.FillEntityWithXml<EnterEvent>(doc);
                                break;
                            case EventTypes.Location:
                                requestMessage = EntityHelper.FillEntityWithXml<LocationEvent>(doc);
                                break;
                            case EventTypes.Subscribe:
                                requestMessage = EntityHelper.FillEntityWithXml<SubscribeEvent>(doc);
                                break;
                            case EventTypes.Unsubscribe:
                                requestMessage = EntityHelper.FillEntityWithXml<UnsunscribeEvent>(doc);
                                break;
                            case EventTypes.Click:
                                requestMessage = EntityHelper.FillEntityWithXml<ClickEvent>(doc);
                                break;
                            case EventTypes.Scan:
                                requestMessage = EntityHelper.FillEntityWithXml<ScanEvent>(doc);
                                break;
                            case EventTypes.View:
                                requestMessage = EntityHelper.FillEntityWithXml<ViewEvent>(doc);
                                break;
                            case EventTypes.ScanPush:
                                requestMessage = EntityHelper.FillEntityWithXml<ScanPushEvent>(doc);
                                break;
                            case EventTypes.WaitScanPush:
                                requestMessage = EntityHelper.FillEntityWithXml<WaitScanPushEvent>(doc);
                                break;
                            case EventTypes.Camera:
                                requestMessage = EntityHelper.FillEntityWithXml<CameraEvent>(doc);
                                break;
                            case EventTypes.CameraOrAlbum:
                                requestMessage = EntityHelper.FillEntityWithXml<CameraOrAlbumEvent>(doc);
                                break;
                            case EventTypes.WeChatAlbum:
                                requestMessage = EntityHelper.FillEntityWithXml<WeChatAlbumEvent>(doc);
                                break;
                            case EventTypes.SelectLocation:
                                requestMessage = EntityHelper.FillEntityWithXml<SelectLocationEvent>(doc);
                                break;
                            case EventTypes.MassSendJobFinished:
                                requestMessage = EntityHelper.FillEntityWithXml<MassSendJobFinishedEvent>(doc);
                                break;
                            case EventTypes.TemplateSendFinished:
                                requestMessage = EntityHelper.FillEntityWithXml<TemplateSendFinishedEvent>(doc);
                                break;
                            case EventTypes.ViewMiniProgram:
                                requestMessage = EntityHelper.FillEntityWithXml<ViewMiniProgramEvent>(doc);
                                break;
                        }

                        break;

                    default:
                        {
                            requestMessage = new UnknowRequest()
                            {
                                Content = doc,
                            };
                            break;
                        }
                }
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, string.Format(CultureInfo.InvariantCulture, "RequestMessage Error, MsgType may not exist, XML：{0}", doc));
                throw;
            }

            return requestMessage;
        }

        public static string ConvertResponseToXml(object entity)
        {
            var responseXmlString = string.Empty;

            // Just let it throw when convert entity failed.
            if (entity is IResponseMessageBase responseMessage)
            {
                switch (responseMessage.MsgType)
                {
                    case ResponseMessageTypes.Text:
                        responseXmlString = EntityHelper.ConvertEntityToXmlString<TextResponse>(responseMessage);
                        break;
                    case ResponseMessageTypes.Image:
                        responseXmlString = EntityHelper.ConvertEntityToXmlString<ImageResponse>(responseMessage);
                        break;
                    case ResponseMessageTypes.Voice:
                        responseXmlString = EntityHelper.ConvertEntityToXmlString<VoiceResponse>(responseMessage);
                        break;
                    case ResponseMessageTypes.Video:
                        responseXmlString = EntityHelper.ConvertEntityToXmlString<VideoResponse>(responseMessage);
                        break;
                    case ResponseMessageTypes.Music:
                        responseXmlString = EntityHelper.ConvertEntityToXmlString<MusicResponse>(responseMessage);
                        break;
                    case ResponseMessageTypes.News:
                        responseXmlString = EntityHelper.ConvertEntityToXmlString<NewsResponse>(responseMessage);
                        break;
                }
            }
            else
            {
                responseXmlString = entity.ToString();
            }

            return responseXmlString;
        }

        /// <summary>
        /// Return RequestMessageType based on xml info.
        /// </summary>
        /// <param name="requestMessageDocument">Xml document.</param>
        /// <returns>Request message type string.</returns>
        private static string GetRequestMsgTypeString(XDocument requestMessageDocument)
        {
            if (requestMessageDocument == null || requestMessageDocument.Root == null || requestMessageDocument.Root.Element("MsgType") == null)
            {
                // Enum ToString(IFormatProvider provider) is obsolete.
                return RequestMessageTypes.Unknown;
            }

            return requestMessageDocument.Root.Element("MsgType").Value;
        }
    }
}
