// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Rendering.Html;
using Microsoft.Bot.Builder.Adapters.WeChat.Extensions;
using Microsoft.Bot.Builder.Adapters.WeChat.Helpers;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.MarkedNet;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// WeChat massage mapper that can convert the message from a WeChat request to Activity or Activity to WeChat response.
    /// </summary>
    /// <remarks>
    /// WeChat message mapper will help create the bot activity and WeChat response.
    /// When deal with the media attachments or cards, mapper will upload the data first to aquire the acceptable media url.
    /// </remarks>
    public class WeChatMessageMapper : IWeChatMessageMapper
    {
        /// <summary>
        /// Max size for a single WeChat response message.
        /// </summary>
        private const int MaxSingleMessageLength = 2048;

        /// <summary>
        /// Max size for one request to WeChat.
        /// </summary>
        private const int MaxTotalMessageLength = 20480;

        /// <summary>
        /// Default value for WeChat news message.
        /// </summary>
        private const string DefaultContentUrl = "https://dev.botframework.com";

        /// <summary>
        /// Default word break when join two word.
        /// </summary>
        private const string WordBreak = "  ";

        /// <summary>
        /// New line string.
        /// </summary>
        private const string NewLine = "\r\n";

        private readonly WeChatClient _wechatClient;
        private readonly ILogger _logger;
        private readonly bool _uploadTemporaryMedia;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeChatMessageMapper"/> class,
        /// using a injected configuration and wechatClient.
        /// </summary>
        /// <param name="uploadTemporaryMedia">The IConfiguration instance need to used by mapper.</param>
        /// <param name="wechatClient">The WeChat client need to be used when need to call WeChat api, like upload media, etc.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public WeChatMessageMapper(WeChatClient wechatClient, bool uploadTemporaryMedia, ILogger logger = null)
        {
            _wechatClient = wechatClient;
            _uploadTemporaryMedia = uploadTemporaryMedia;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Convert WeChat message to Activity.
        /// </summary>
        /// <param name="request">WeChat request message.</param>
        /// <returns>Activity.</returns>
        public async Task<IActivity> ToConnectorMessage(IRequestMessageBase request)
        {
            // Handle event request
            if (request is IRequestMessageEventBase eventRequest)
            {
                // TODO: currently set event body into channel data.
                var eventActivity = Activity.CreateEventActivity();
                eventActivity.SetValueFromRequest(eventRequest);
                return eventActivity;
            }
            else
            {
                var messageActivity = Activity.CreateMessageActivity();
                messageActivity.SetValueFromRequest(request);
                if (request is TextRequest textRequest)
                {
                    messageActivity.Text = textRequest.Content;
                }
                else if (request is ImageRequest imageRequest)
                {
                    var attachment = new Attachment
                    {
                        ContentType = MimeTypesMap.GetMimeType(imageRequest.PicUrl) ?? MediaTypes.Image,
                        ContentUrl = imageRequest.PicUrl,
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is VoiceRequest voiceRequest)
                {
                    messageActivity.Text = voiceRequest.Recognition;
                    var attachment = new Attachment
                    {
                        ContentType = MimeTypesMap.GetMimeType(voiceRequest.Format) ?? MediaTypes.Voice,
                        ContentUrl = await _wechatClient.GetMediaUrlAsync(voiceRequest.MediaId).ConfigureAwait(false),
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is VideoRequest videoRequest)
                {
                    var attachment = new Attachment
                    {
                        // video request don't have format, type will be value.
                        ContentType = MediaTypes.Video,
                        ContentUrl = await _wechatClient.GetMediaUrlAsync(videoRequest.MediaId).ConfigureAwait(false),
                        ThumbnailUrl = await _wechatClient.GetMediaUrlAsync(videoRequest.ThumbMediaId).ConfigureAwait(false),
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is ShortVideoRequest shortVideoRequest)
                {
                    var attachment = new Attachment
                    {
                        ContentType = MediaTypes.Video,
                        ContentUrl = await _wechatClient.GetMediaUrlAsync(shortVideoRequest.MediaId).ConfigureAwait(false),
                        ThumbnailUrl = await _wechatClient.GetMediaUrlAsync(shortVideoRequest.ThumbMediaId).ConfigureAwait(false),
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is LocationRequest locationRequest)
                {
                    var geo = new GeoCoordinates
                    {
                        Name = locationRequest.Label,
                        Latitude = locationRequest.Latitude,
                        Longitude = locationRequest.Longtitude,
                    };
                    messageActivity.Entities.Add(geo);
                }
                else if (request is LinkRequest linkRequest)
                {
                    messageActivity.Text = linkRequest.Title + linkRequest.Url;
                    messageActivity.Summary = linkRequest.Description;
                }

                return messageActivity;
            }

            throw new NotImplementedException("Message type not supported yet.");
        }

        /// <summary>
        /// Convert response message from Bot format to Wechat format.
        /// </summary>
        /// <param name="activity">message activity received from bot.</param>
        /// <returns>WeChat message list.</returns>
        public async Task<IList<IResponseMessageBase>> ToWeChatMessages(IActivity activity)
        {
            try
            {
                var responseMessageList = new List<IResponseMessageBase>();

                if (activity is IMessageActivity messageActivity)
                {
                    // Chunk message into pieces as necessary
                    responseMessageList.AddRange(GetChunkedMessages(messageActivity, messageActivity.Text));

                    // Process suggested actions if any
                    if (messageActivity.SuggestedActions?.Actions?.Any() == true)
                    {
                        responseMessageList.Add(SuggestionActionsToWeChatMessage(messageActivity, messageActivity.SuggestedActions));
                    }

                    // Message with no attachments
                    if (messageActivity.Attachments == null || messageActivity.Attachments.Count == 0)
                    {
                        return responseMessageList;
                    }

                    foreach (var attachment in messageActivity.Attachments)
                    {
                        if (attachment.ContentType == AdaptiveCard.ContentType ||
                            attachment.ContentType == "application/adaptive-card" ||
                            attachment.ContentType == "application/vnd.microsoft.card.adaptive")
                        {
                            var adaptiveCard = attachment.ContentAs<AdaptiveCard>();
                            responseMessageList.AddRange(await ProcessAdaptiveCardAsync(messageActivity, adaptiveCard, attachment.Name).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == AudioCard.ContentType)
                        {
                            var audioCard = attachment.ContentAs<AudioCard>();
                            responseMessageList.AddRange(await ProcessAudioCardAsync(messageActivity, audioCard).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == AnimationCard.ContentType)
                        {
                            var animationCard = attachment.ContentAs<AnimationCard>();
                            responseMessageList.AddRange(await ProcessAnimationCardAsync(messageActivity, animationCard).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == HeroCard.ContentType)
                        {
                            var heroCard = attachment.ContentAs<HeroCard>();
                            responseMessageList.AddRange(await ProcessHeroCardAsync(messageActivity, heroCard).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == ThumbnailCard.ContentType)
                        {
                            var thumbnailCard = attachment.ContentAs<ThumbnailCard>();
                            responseMessageList.AddRange(ProcessThumbnailCard(messageActivity, thumbnailCard));
                        }
                        else if (attachment.ContentType == ReceiptCard.ContentType)
                        {
                            var receiptCard = attachment.ContentAs<ReceiptCard>();
                            responseMessageList.AddRange(ProcessReceiptCard(messageActivity, receiptCard));
                        }
                        else if (attachment.ContentType == SigninCard.ContentType)
                        {
                            var signinCard = attachment.ContentAs<SigninCard>();
                            responseMessageList.AddRange(ProcessSigninCard(signinCard, messageActivity));
                        }
                        else if (attachment.ContentType == OAuthCard.ContentType)
                        {
                            var oauthCard = attachment.ContentAs<OAuthCard>();
                            responseMessageList.AddRange(ProcessOAuthCard(oauthCard, messageActivity));
                        }
                        else if (attachment.ContentType == VideoCard.ContentType)
                        {
                            var videoCard = attachment.ContentAs<VideoCard>();
                            responseMessageList.AddRange(await ProcessVideoCardAsync(messageActivity, videoCard).ConfigureAwait(false));
                        }
                        else if (attachment != null &&
                                    (!string.IsNullOrEmpty(attachment.ContentUrl) ||
                                     attachment.Content != null ||
                                     !string.IsNullOrEmpty(attachment.ThumbnailUrl)))
                        {
                            responseMessageList.AddRange(await ProcessAttachmentAsync(messageActivity, attachment).ConfigureAwait(false));
                        }
                        else
                        {
                            _logger.LogInformation($"Unsupported content type {attachment.ContentType}");
                        }
                    }
                }
                else if (activity is IEventActivity eventActivity)
                {
                    // WeChat won't accept event type, just bypass.
                }

                return responseMessageList;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Parse to wechat message failed");
                throw;
            }
        }

        /// <summary>
        /// Create a News instance use hero card.
        /// </summary>
        /// <param name="activity">Message activity received from bot.</param>
        /// <param name="heroCard">Hero card instance.</param>
        /// <returns>A new instance of News create by hero card.</returns>
        public async Task<News> CreateNewsFromHeroCard(IMessageActivity activity, HeroCard heroCard)
        {
            // Add text
            var news = new News
            {
                Author = activity.From.Name,
                Description = heroCard.Subtitle,
                Content = heroCard.Text,
                Title = heroCard.Title,
                ShowCoverPicture = heroCard.Images.Count > 0 ? "1" : "0",

                // TODO: replace with url
                ContentSourceUrl = DefaultContentUrl,
            };

            foreach (var image in heroCard.Images ?? new List<CardImage>())
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = image.Url,
                    ContentType = MediaTypes.Image,
                    Name = image.Alt,
                };

                // MP news image is required and can not be a temporary media.
                var mediaMessage = await AttachmentToWeChatMessageAsync(activity, surrogate).ConfigureAwait(false);
                news.ThumbMediaId = (mediaMessage.FirstOrDefault() as ImageResponse).Image.MediaId;
                news.ThumbUrl = image.Url;
            }

            return news;
        }

        public async Task<News> CreateNewsFromAdaptiveCard(IMessageActivity activity, AdaptiveCard card, string title)
        {
            try
            {
                var renderer = new AdaptiveCardRenderer();
                var schemaVersion = renderer.SupportedSchemaVersion;

                foreach (var element in card.Body)
                {
                    if (element is AdaptiveMedia adaptiveMedia)
                    {
                        foreach (var media in adaptiveMedia.Sources)
                        {
                            var mediaUrl = await UploadNewsImage(media, adaptiveMedia.AltText ?? adaptiveMedia.Id).ConfigureAwait(false);
                            media.Url = mediaUrl;
                        }
                    }
                }

                // Render the card
                var renderedCard = renderer.RenderCard(card);

                // Get the output HTML
                var html = renderedCard.Html;

                // (Optional) Check for any renderer warnings
                // This includes things like an unknown element type found in the card
                // Or the card exceeded the maximum number of supported actions, etc
                var warnings = renderedCard.Warnings;

                // Add text
                var news = new News
                {
                    Author = activity.From.Name,
                    Description = card.Speak ?? card.FallbackText,
                    Content = html.ToString(),
                    Title = title ?? new Guid().ToString(),

                    // Set not should cover, because adaptive card don't have a cover.
                    ShowCoverPicture = "0",
                    ContentSourceUrl = DefaultContentUrl,
                };

                // WeChat news don't support background image.
                return news;
            }
            catch (AdaptiveException ex)
            {
                // Failed rendering
                _logger.LogError(ex, "Failed to rending adaptive card.");
                throw;
            }
            catch (Exception ex)
            {
                // upload graphic message failed.
                _logger.LogError(ex, "Error when uploading adaptive card.");
                throw;
            }
        }

        /// <summary>
        /// Create a media type response message using mediaId and acitivity.
        /// </summary>
        /// <param name="activity">Activity from bot.</param>
        /// <param name="mediaId">MediaId from WeChat.</param>
        /// <param name="type">Media type.</param>
        /// <returns>Media resposne such as ImageResponse, etc.</returns>
        private static ResponseMessage CreateMediaResponse(IActivity activity, string mediaId, string type)
        {
            ResponseMessage response = null;
            if (type.Contains(MediaTypes.Image))
            {
                response = new ImageResponse(mediaId);
            }

            if (type.Contains(MediaTypes.Video))
            {
                response = new VideoResponse(mediaId);
            }

            if (type.Contains(MediaTypes.Audio))
            {
                response = new VoiceResponse(mediaId);
            }

            response.SetProperties(activity);
            return response;
        }

        /// <summary>
        /// Convert Text To WeChat Message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <returns>Response message to WeChat.</returns>
        private static TextResponse CreateTextResponseFromMessageActivity(IMessageActivity activity)
        {
            var response = new TextResponse
            {
                Content = activity.Text,
            };
            response.SetProperties(activity);
            return response;
        }

        /// <summary>
        /// Add new line and append new text.
        /// </summary>
        /// <param name="text">The origin text.</param>
        /// <param name="newText">Text need to be attached.</param>
        /// <returns>Combined new text string.</returns>
        private static string AddLine(string text, string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return text;
            }

            if (string.IsNullOrEmpty(text))
            {
                return newText;
            }

            return text + NewLine + newText;
        }

        /// <summary>
        /// Add text break and append the new text.
        /// </summary>
        /// <param name="text">The origin text.</param>
        /// <param name="newText">Text need to be attached.</param>
        /// <returns>Combined new text string.</returns>
        private static string AddText(string text, string newText)
        {
            if (string.IsNullOrEmpty(newText))
            {
                return text;
            }

            if (string.IsNullOrEmpty(text))
            {
                return newText;
            }

            return text + WordBreak + newText;
        }

        /// <summary>
        /// Chunk the text message and return it as WeChat response.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="text">Text content need to be chunked.</param>
        /// <returns>Response message list.</returns>
        private static IList<IResponseMessageBase> GetChunkedMessages(IMessageActivity activity, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<IResponseMessageBase>();
            }

            if (activity.TextFormat == TextFormatTypes.Markdown)
            {
                var marked = new Marked
                {
                    Options =
                    {
                        Sanitize = false,
                        Mangle = false,
                    },
                };
                marked.Options.Renderer = new TextMarkdownRenderer(marked.Options);

                // Marked package will return additional new line in the end.
                text = marked.Parse(text).Trim();
            }

            // If message doesn't need to be chunked just return it
            if (text.Length <= MaxSingleMessageLength)
            {
                var textResponse = CreateTextResponseFromMessageActivity(activity);
                textResponse.Content = text;
                return new List<IResponseMessageBase>
                {
                    textResponse,
                };
            }

            // Truncate to maximum total length as necessary
            if (text.Length > MaxTotalMessageLength)
            {
                text = text.Substring(0, MaxTotalMessageLength);
            }

            // Split text into chunks
            var messages = new List<IResponseMessageBase>();
            var chunkLength = MaxSingleMessageLength - 20;  // leave 20 chars for footer
            var chunkNum = 0;
            var chunkCount = text.Length / chunkLength;

            if (text.Length % chunkLength > 0)
            {
                chunkCount++;
            }

            for (var i = 0; i < text.Length; i += chunkLength)
            {
                if (chunkLength + i > text.Length)
                {
                    chunkLength = text.Length - i;
                }

                var chunk = text.Substring(i, chunkLength);

                if (chunkCount > 1)
                {
                    chunk += $"{NewLine}({++chunkNum} of {chunkCount})";
                }

                // Create chunked message and add to list of messages
                var textResponse = CreateTextResponseFromMessageActivity(activity);
                textResponse.Content = chunk;
                messages.Add(textResponse);
            }

            return messages;
        }

        /// <summary>
        /// Convert buttons to text string for channels that can't display button.
        /// </summary>
        /// <param name="button">The Card Action.</param>
        /// <param name="index">Index of current action in action list.</param>
        /// <returns>Card action as string.</returns>
        private static string ButtonToText(CardAction button, int index = -1)
        {
            switch (button.Type)
            {
                case ActionTypes.OpenUrl:
                case ActionTypes.PlayAudio:
                case ActionTypes.PlayVideo:
                case ActionTypes.ShowImage:
                case ActionTypes.Signin:
                case ActionTypes.DownloadFile:
                    if (index != -1)
                    {
                        return $"{index}. <a href='{button.Value}'>{button.Title}</a>";
                    }

                    return $"<a href='{button.Value}'>{button.Title}</a>";
                case ActionTypes.MessageBack:
                    if (index != -1)
                    {
                        return $"{index}. {button.Title ?? button.Text}";
                    }

                    return $"{button.Title ?? button.Text}";
                default:
                    if (index != -1)
                    {
                        return $"{index}. {button.Title ?? button.Value}";
                    }

                    return $"{button.Title ?? button.Value}";
            }
        }

        /// <summary>
        /// Convert all buttons in a message to text string for channels that can't display button.
        /// </summary>
        /// <param name="actions">CardAction list.</param>
        /// <param name="actionToString">Specific way of converting actions to string was specified.</param>
        /// <returns>CardAction as string.</returns>
        private static string ButtonsToText(IList<CardAction> actions, Func<CardAction, string> actionToString = null)
        {
            // Convert any options to text
            var text = string.Empty;
            actions = actions ?? new List<CardAction>();
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (i > 0)
                {
                    text += NewLine;
                }

                // If a specific way of converting actions to string was specified, use it
                if (actionToString != null)
                {
                    text += actionToString(action);
                }

                // Otherwise, use the default
                else
                {
                    var index = actions.Count == 1 ? -1 : i + 1;
                    text += ButtonToText(action, index);
                }
            }

            return text;
        }

        /// <summary>
        /// Convert all suggestedActions to text string for channels that can't display them natively.
        /// </summary>
        /// <param name="actions">List of card action.</param>
        /// <returns>CardAction as string.</returns>
        private static string SuggestedActionsToText(IList<CardAction> actions)
        {
            var result = ButtonsToText(actions, action =>
            {
                var buttonText = action.Text == ActionTypes.MessageBack ? action.Text : action.Value as string;
                return buttonText ?? action.Title;
            });
            return result;
        }

        /// <summary>
        /// Convert suggestion actions to wechat message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="suggestedActions">the suggestions.</param>
        /// <returns>Response message to WeChat.</returns>
        private IResponseMessageBase SuggestionActionsToWeChatMessage(IMessageActivity activity, SuggestedActions suggestedActions)
        {
            var response = CreateTextResponseFromMessageActivity(activity);
            var actionString = SuggestedActionsToText(suggestedActions.Actions);
            response.Content = actionString;
            return response;
        }

        /// <summary>
        /// Convert attachment to wechat messages.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="attachmentData">AttachmentData object.</param>
        /// <returns>Response message to WeChat.</returns>
        private async Task<IResponseMessageBase> AttachmentDataToWeChatMessage(IMessageActivity activity, AttachmentData attachmentData)
        {
            if (!AttachmentHelper.IsValidAttachmentData(attachmentData))
            {
                _logger.LogInformation($"InValid AttachmentData.");
                return null;
            }

            var type = string.Empty;
            if (attachmentData.Type.Contains(MediaTypes.Image))
            {
                type = UploadMediaType.Image;
            }

            if (attachmentData.Type.Contains(MediaTypes.Video))
            {
                type = UploadMediaType.Video;
            }

            if (attachmentData.Type.Contains(MediaTypes.Audio))
            {
                type = UploadMediaType.Voice;
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new NotSupportedException($"Attachment type: {attachmentData.Type} not supported yet.");
            }

            string mediaId;

            // document said mp news should not use temp media_id, but is working actually.
            if (_uploadTemporaryMedia)
            {
                var uploadResult = await _wechatClient.UploadTemporaryMediaAsync(type, attachmentData).ConfigureAwait(false);
                mediaId = uploadResult.MediaId;
            }
            else
            {
                var uploadResult = await _wechatClient.UploadPersistentMediaAsync(type, attachmentData).ConfigureAwait(false);
                mediaId = uploadResult.MediaId;
            }

            return CreateMediaResponse(activity, mediaId, attachmentData.Type);
        }

        /// <summary>
        /// Upload the image in adaptive card and get the WeChat acceptable media url.
        /// </summary>
        /// <param name="mediaSource">Adaptive card media source.</param>
        /// <returns>Media url.</returns>
        private async Task<string> UploadNewsImage(AdaptiveMediaSource mediaSource, string name)
        {
            // ContentUrl can contain a url or dataUrl of the form "data:image/jpeg;base64,XXXXXXXXX..."
            var attachmentData = new AttachmentData(name: string.IsNullOrEmpty(name) ? new Guid().ToString() : name);
            if (AttachmentHelper.IsUrl(mediaSource.Url))
            {
                var bytesData = await _wechatClient.SendHttpRequestAsync(HttpMethod.Get, mediaSource.Url).ConfigureAwait(false);
                attachmentData.Type = mediaSource.MimeType;
                attachmentData.OriginalBase64 = bytesData;
                attachmentData.ThumbnailBase64 = bytesData;
            }
            else
            {
                var bytesData = AttachmentHelper.DecodeBase64String(mediaSource.Url, out var contentType);
                attachmentData.Type = contentType;
                attachmentData.OriginalBase64 = bytesData;
                attachmentData.ThumbnailBase64 = bytesData;
            }

            var uploadResult = await _wechatClient.UploadPersistentMediaAsync(UploadMediaType.Image, attachmentData).ConfigureAwait(false);
            return uploadResult.Url;
        }

        /// <summary>
        /// Upload media to WeChat and map to WeChat Response message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="attachment">Current attachment.</param>
        /// <returns>List of response message to WeChat.</returns>
        private async Task<IList<IResponseMessageBase>> AttachmentToWeChatMessageAsync(IMessageActivity activity, Attachment attachment)
        {
            var responseList = new List<IResponseMessageBase>();
            attachment.Properties.TryGetValue("MediaId", StringComparison.InvariantCultureIgnoreCase, out var mediaId);
            if (!string.IsNullOrEmpty(mediaId?.ToString()))
            {
                var response = CreateMediaResponse(activity, mediaId.ToString(), attachment.ContentType);
                responseList.Add(response);
            }
            else if (attachment.ContentUrl != null)
            {
                // ContentUrl can contain a url or dataUrl of the form "data:image/jpeg;base64,XXXXXXXXX..."
                var attachmentData = new AttachmentData(name: attachment.Name ?? new Guid().ToString());
                if (AttachmentHelper.IsUrl(attachment.ContentUrl))
                {
                    var bytesData = await _wechatClient.SendHttpRequestAsync(HttpMethod.Get, attachment.ContentUrl).ConfigureAwait(false);
                    attachmentData.Type = attachment.ContentType;
                    attachmentData.OriginalBase64 = bytesData;
                    attachmentData.ThumbnailBase64 = bytesData;
                }
                else
                {
                    var bytesData = AttachmentHelper.DecodeBase64String(attachment.ContentUrl, out var contentType);
                    attachmentData.Type = contentType;
                    attachmentData.OriginalBase64 = bytesData;
                    attachmentData.ThumbnailBase64 = bytesData;
                }

                var response = await AttachmentDataToWeChatMessage(activity, attachmentData).ConfigureAwait(false);
                if (response != null)
                {
                    responseList.Add(response);
                }
            }

            return responseList;
        }

        /// <summary>
        /// render a adaptiveCard into text replies for low-fi channels.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="adaptiveCard">Adaptive card instance need to be converted.</param>
        /// <returns>WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAdaptiveCardAsync(IMessageActivity activity, AdaptiveCard adaptiveCard, string title)
        {
            var messages = new List<IResponseMessageBase>();

            try
            {
                var news = await CreateNewsFromAdaptiveCard(activity, adaptiveCard, title).ConfigureAwait(false);
                var uploadResult = await _wechatClient.UploadPersistentNewsAsync(new News[] { news }).ConfigureAwait(false);
                var mpnews = new MPNewsResponse(uploadResult.MediaId);
                messages.Add(mpnews);
            }
#pragma warning disable CA1031 // Do not catch general exception types, use fallback text instead.
            catch
#pragma warning disable CA1031 // Do not catch general exception types, use fallback text instead.
            {
                _logger.LogInformation("Convert adaptive card failed.");
                messages.AddRange(GetChunkedMessages(activity, adaptiveCard.FallbackText));
            }

            return messages;
        }

        /// <summary>
        /// Convert hero card to WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="heroCard">Hero card instance need to be converted.</param>
        /// <returns>WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessHeroCardAsync(IMessageActivity activity, HeroCard heroCard)
        {
            var messages = new List<IResponseMessageBase>();
            var news = await CreateNewsFromHeroCard(activity, heroCard).ConfigureAwait(false);
            var uploadResult = await _wechatClient.UploadTemporaryNewsAsync(new News[] { news }).ConfigureAwait(false);
            var mpnews = new MPNewsResponse(uploadResult.MediaId);
            messages.Add(mpnews);

            // Add buttons
            if (heroCard.Buttons != null && heroCard.Buttons.Count > 0)
            {
                var buttonString = ButtonsToText(heroCard.Buttons);
                messages.AddRange(GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        /// <summary>
        /// Process thumbnail card and return the WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="thumbnailCard">Thumbnail card instance need to be converted.</param>
        /// <returns>WeChat response message.</returns>
        private IList<IResponseMessageBase> ProcessThumbnailCard(IMessageActivity activity, ThumbnailCard thumbnailCard)
        {
            var messages = new List<IResponseMessageBase>();

            // Add text
            var body = thumbnailCard.Subtitle;
            body = AddLine(body, thumbnailCard.Text);
            var article = new Article
            {
                Title = thumbnailCard.Title,
                Description = body,
                Url = thumbnailCard.Tap?.Value.ToString(),
                PicUrl = thumbnailCard.Images.FirstOrDefault().Url,
            };
            var newsResponse = new NewsResponse()
            {
                Articles = new List<Article>() { article },
            };
            messages.Add(newsResponse);

            // Add buttons
            if (thumbnailCard.Buttons != null && thumbnailCard.Buttons.Count > 0)
            {
                var buttonString = ButtonsToText(thumbnailCard.Buttons.ToArray());
                messages.AddRange(GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        private async Task<IList<IResponseMessageBase>> ProcessVideoCardAsync(IMessageActivity activity, VideoCard videoCard)
        {
            var messages = new List<IResponseMessageBase>();

            var body = videoCard.Subtitle;
            body = AddLine(body, videoCard.Text);
            Video video = null;

            // upload thumbnail image.
            if (!string.IsNullOrEmpty(videoCard.Image?.Url))
            {
                // TODO: wechat doc have thumb_media_id for video mesasge, but not implemented in current package.
                var surrogate = new Attachment()
                {
                    ContentType = MediaTypes.Video,
                    Name = videoCard.Title,
                    ContentUrl = videoCard.Media[0].Url,
                };
                var reponseList = await AttachmentToWeChatMessageAsync(activity, surrogate).ConfigureAwait(false);
                if (reponseList.FirstOrDefault() is VideoResponse videoResponse)
                {
                    video = new Video(videoResponse.Video.MediaId, videoCard.Title, body);
                }
            }

            messages.Add(new VideoResponse
            {
                CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                FromUserName = activity.From.Id,
                ToUserName = activity.Recipient.Id,
                Video = video,
            });

            // Add buttons
            if (videoCard.Buttons != null && videoCard.Buttons.Count > 0)
            {
                var buttonString = string.Empty;
                buttonString = ButtonsToText(videoCard.Buttons.ToArray());
                messages.AddRange(GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        /// <summary>
        /// Convert audio card as music resposne.
        /// Thumbnail image size limitation is not clear.
        /// </summary>
        /// <returns>List of response message to WeChat.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAudioCardAsync(IMessageActivity activity, AudioCard audioCard)
        {
            var messages = new List<IResponseMessageBase>();

            var body = audioCard.Subtitle;
            body = AddLine(body, audioCard.Text);
            var music = new Music
            {
                Title = audioCard.Title,
                MusicUrl = audioCard.Media[0].Url,
                HQMusicUrl = audioCard.Media[0].Url,
                Description = body,
            };

            // upload thumbnail image.
            if (!string.IsNullOrEmpty(audioCard.Image?.Url))
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = audioCard.Image.Url,
                    ContentType = MediaTypes.Image,
                    Name = audioCard.Image.Alt,
                };
                var reponseList = await AttachmentToWeChatMessageAsync(activity, surrogate).ConfigureAwait(false);
                if (reponseList.FirstOrDefault() is ImageResponse imageResponse)
                {
                    music.ThumbMediaId = imageResponse.Image.MediaId;
                }
            }

            var musicResponse = new MusicResponse
            {
                CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                FromUserName = activity.From.Id,
                ToUserName = activity.Recipient.Id,
                Music = music,
            };
            messages.Add(musicResponse);

            // Add buttons
            if (audioCard.Buttons != null && audioCard.Buttons.Count > 0)
            {
                var buttonString = string.Empty;
                buttonString = ButtonsToText(audioCard.Buttons);
                messages.AddRange(GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        private async Task<IList<IResponseMessageBase>> ProcessAnimationCardAsync(IMessageActivity activity, AnimationCard mediacard)
        {
            var messages = new List<IResponseMessageBase>();

            // Generate body
            var body = mediacard.Title;
            body = AddLine(body, mediacard.Subtitle);
            body = AddLine(body, mediacard.Text);

            // Add buttons
            if (mediacard.Buttons != null && mediacard.Buttons.Count > 0)
            {
                body = AddLine(body, ButtonsToText(mediacard.Buttons));
            }

            // Add image
            if (!string.IsNullOrEmpty(mediacard.Image?.Url))
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = mediacard.Image.Url,
                    ContentType = MediaTypes.Image,
                    Name = mediacard.Image.Alt,
                };

                messages.AddRange(await AttachmentToWeChatMessageAsync(activity, surrogate).ConfigureAwait(false));
            }

            // Add mediaUrls
            foreach (var mediaUrl in mediacard.Media ?? new List<MediaUrl>())
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = mediaUrl.Url,
                    Name = mediaUrl.Profile,
                    ContentType = MediaTypes.Gif,
                };

                messages.AddRange(await AttachmentToWeChatMessageAsync(activity, surrogate).ConfigureAwait(false));
            }

            messages.AddRange(GetChunkedMessages(activity, body));

            return messages;
        }

        /// <summary>
        /// Downgrade ReceiptCard into text replies for low-fi channels.
        /// </summary>
        /// <returns>List of response message to WeChat.</returns>
        private IList<IResponseMessageBase> ProcessReceiptCard(
            IMessageActivity activity,
            ReceiptCard receiptCard)
        {
            var messages = new List<IResponseMessageBase>();

            // Build text portion of receipt
            var body = receiptCard.Title;
            foreach (var fact in receiptCard.Facts ?? new List<Fact>())
            {
                body = AddLine(body, $"{fact.Key}:  {fact.Value}");
            }

            messages.AddRange(GetChunkedMessages(activity, body));

            // Add items, grouping text only ones into a single post
            string textbody = null;
            foreach (var item in receiptCard.Items ?? new List<ReceiptItem>())
            {
                if (item.Image != null)
                {
                    body = AddText(item.Title, item.Price);
                    body = AddLine(body, item.Subtitle);
                    body = AddLine(body, item.Text);
                    messages.AddRange(GetChunkedMessages(activity, body));
                }
                else
                {
                    textbody = AddLine(textbody, item.Title);
                    textbody = AddText(textbody, item.Price);
                    textbody = AddLine(textbody, item.Subtitle);
                    textbody = AddLine(textbody, item.Text);
                }
            }

            // Add textonly items
            messages.AddRange(GetChunkedMessages(activity, textbody));

            // Add totals
            body = $"Tax:  {receiptCard.Tax}";
            body = AddLine(body, $"Total:  {receiptCard.Total}");

            // Add buttons
            if (receiptCard.Buttons != null && receiptCard.Buttons.Count > 0)
            {
                body = AddLine(body, ButtonsToText(receiptCard.Buttons));
            }

            messages.AddRange(GetChunkedMessages(activity, body));

            return messages;
        }

        /// <summary>
        /// Downgrade SigninCard into text replies for low-fi channels.
        /// </summary>
        private List<IResponseMessageBase> ProcessSigninCard(SigninCard signinCard, IMessageActivity activity)
        {
            var messages = new List<IResponseMessageBase>();

            // Add text
            messages.AddRange(GetChunkedMessages(activity, signinCard.Text));

            // Add button
            if (signinCard.Buttons != null)
            {
                messages.AddRange(GetChunkedMessages(activity, ButtonToText(signinCard.Buttons.First())));
            }

            return messages;
        }

        /// <summary>
        /// Downgrade OAuthCard into text replies for low-fi channels.
        /// </summary>
        private List<IResponseMessageBase> ProcessOAuthCard(OAuthCard oauthCard, IMessageActivity activity)
        {
            var messages = new List<IResponseMessageBase>();

            // Add text
            messages.AddRange(GetChunkedMessages(activity, oauthCard.Text));

            // Add button
            if (oauthCard.Buttons != null)
            {
                messages.AddRange(GetChunkedMessages(activity, ButtonToText(oauthCard.Buttons.FirstOrDefault())));
            }

            return messages;
        }

        /// <summary>
        /// Convert attachments to WeChat response message.
        /// </summary>
        private async Task<List<IResponseMessageBase>> ProcessAttachmentAsync(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();

            messages.AddRange(await AttachmentToWeChatMessageAsync(activity, attachment).ConfigureAwait(false));

            if (messages.Any())
            {
                return messages;
            }

            return messages;
        }
    }
}
