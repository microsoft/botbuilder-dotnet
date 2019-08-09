// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Rendering.Html;
using Microsoft.Bot.Builder.Adapters.WeChat.Extensions;
using Microsoft.Bot.Builder.Adapters.WeChat.Helpers;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Responses;
using Microsoft.Bot.Connector;
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
    public class WeChatMessageMapper
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
        /// <param name="wechatRequest">WeChat request message.</param>
        /// <returns>Activity.</returns>
        public async Task<IActivity> ToConnectorMessage(IRequestMessageBase wechatRequest)
        {
            var activity = CreateActivity(wechatRequest);
            if (wechatRequest is TextRequest textRequest)
            {
                activity.Text = textRequest.Content;
            }
            else if (wechatRequest is ImageRequest imageRequest)
            {
                var attachment = new Attachment
                {
                    ContentType = MimeTypesMap.GetMimeType(imageRequest.PicUrl) ?? MediaTypes.Image,
                    ContentUrl = imageRequest.PicUrl,
                };
                activity.Attachments.Add(attachment);
            }
            else if (wechatRequest is VoiceRequest voiceRequest)
            {
                activity.Text = voiceRequest.Recognition;
                var attachment = new Attachment
                {
                    ContentType = MimeTypesMap.GetMimeType(voiceRequest.Format) ?? MediaTypes.Voice,
                    ContentUrl = await _wechatClient.GetMediaUrlAsync(voiceRequest.MediaId).ConfigureAwait(false),
                };
                activity.Attachments.Add(attachment);
            }
            else if (wechatRequest is VideoRequest videoRequest)
            {
                var attachment = new Attachment
                {
                    // video request don't have format, type will be value.
                    ContentType = MediaTypes.Video,
                    ContentUrl = await _wechatClient.GetMediaUrlAsync(videoRequest.MediaId).ConfigureAwait(false),
                    ThumbnailUrl = await _wechatClient.GetMediaUrlAsync(videoRequest.ThumbMediaId).ConfigureAwait(false),
                };
                activity.Attachments.Add(attachment);
            }
            else if (wechatRequest is ShortVideoRequest shortVideoRequest)
            {
                var attachment = new Attachment
                {
                    ContentType = MediaTypes.Video,
                    ContentUrl = await _wechatClient.GetMediaUrlAsync(shortVideoRequest.MediaId).ConfigureAwait(false),
                    ThumbnailUrl = await _wechatClient.GetMediaUrlAsync(shortVideoRequest.ThumbMediaId).ConfigureAwait(false),
                };
                activity.Attachments.Add(attachment);
            }
            else if (wechatRequest is LocationRequest locationRequest)
            {
                var geo = new GeoCoordinates
                {
                    Name = locationRequest.Label,
                    Latitude = locationRequest.Latitude,
                    Longitude = locationRequest.Longtitude,
                };
                activity.Entities.Add(geo);
            }
            else if (wechatRequest is LinkRequest linkRequest)
            {
                activity.Text = linkRequest.Title + linkRequest.Url;
                activity.Summary = linkRequest.Description;
            }

            return activity;
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
                _logger.LogError(e, "Parse to WeChat message failed.");
                throw;
            }
        }

        /// <summary>
        /// Process all types of general attachment.
        /// </summary>
        /// <param name="activity">The message activity.</param>
        /// <param name="attachment">The attachment object need to be processed.</param>
        /// <returns>List of WeChat response message.</returns>
        public async Task<IList<IResponseMessageBase>> ProcessAttachmentAsync(IMessageActivity activity, Attachment attachment)
        {
            var responseList = new List<IResponseMessageBase>();

            // Create media response directly if mediaId provide by user.
            attachment.Properties.TryGetValue("MediaId", StringComparison.InvariantCultureIgnoreCase, out var mediaId);
            if (mediaId != null)
            {
                responseList.Add(CreateMediaResponse(activity, mediaId.ToString(), attachment.ContentType));
            }

            if (!string.IsNullOrEmpty(attachment.ThumbnailUrl))
            {
                responseList.Add(await MediaContentToWeChatResponse(activity, attachment.Name, attachment.ThumbnailUrl, attachment.ContentType).ConfigureAwait(false));
            }

            if (!string.IsNullOrEmpty(attachment.ContentUrl))
            {
                responseList.Add(await MediaContentToWeChatResponse(activity, attachment.Name, attachment.ContentUrl, attachment.ContentType).ConfigureAwait(false));
            }

            if (AttachmentHelper.IsUrl(attachment.Content))
            {
                responseList.Add(await MediaContentToWeChatResponse(activity, attachment.Name, attachment.ContentUrl, attachment.ContentType).ConfigureAwait(false));
            }
            else if (attachment.Content != null)
            {
                responseList.AddRange(GetChunkedMessages(activity, attachment.Content as string));
            }

            return responseList;
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

                // Hero card don't have original url, but it's required by WeChat.
                // Set a default value instead.
                ContentSourceUrl = DefaultContentUrl,
            };

            foreach (var image in heroCard.Images ?? new List<CardImage>())
            {
                // MP news image is required and can not be a temporary media.
                var mediaMessage = await MediaContentToWeChatResponse(activity, image.Alt, image.Url, MediaTypes.Image).ConfigureAwait(false);
                news.ThumbMediaId = (mediaMessage as ImageResponse).Image.MediaId;
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

                // TODO: should upload all media.
                foreach (var element in card.Body)
                {
                    if (element is AdaptiveMedia adaptiveMedia)
                    {
                        foreach (var media in adaptiveMedia.Sources)
                        {
                            var attachmentData = await CreateAttachmentDataAsync(adaptiveMedia.AltText ?? adaptiveMedia.Id, media.Url, media.MimeType).ConfigureAwait(false);
                            var uploadResult = await _wechatClient.UploadMediaAsync(attachmentData, false).ConfigureAwait(false) as UploadPersistentMediaResult;
                            media.Url = uploadResult.Url;
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

            SetCommenField(response, activity);
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
            SetCommenField(response, activity);
            return response;
        }

        /// <summary>
        /// Set commen field in response message.
        /// </summary>
        /// <param name="responseMessage">Response message need to be set.</param>
        /// <param name="activity">Activity instance from bot.</param>
        private static void SetCommenField(ResponseMessage responseMessage, IActivity activity)
        {
            responseMessage.FromUserName = activity.From.Id;
            responseMessage.ToUserName = activity.Recipient.Id;
            responseMessage.CreateTime = activity.Timestamp.HasValue ? activity.Timestamp.Value.ToUnixTimeSeconds() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
        /// Create Activtiy from WeChat request.
        /// </summary>
        /// <param name="wechatRequest">WeChat request instance.</param>
        /// <returns>A activity instance.</returns>
        private static Activity CreateActivity(IRequestMessageBase wechatRequest)
        {
            var activity = new Activity
            {
                ChannelId = Channels.WeChat,
                Recipient = new ChannelAccount(wechatRequest.ToUserName, "Bot", "bot"),
                From = new ChannelAccount(wechatRequest.FromUserName, "User", "user"),

                // Message is handled by adapter itself, may not need serviceurl.
                ServiceUrl = string.Empty,

                // Set user ID as conversation id. wechat request don't have conversation id.
                // TODO: consider how to handle conversation end request if needed. For now Wechat don't have this type.
                Conversation = new ConversationAccount(false, id: wechatRequest.FromUserName),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(wechatRequest.CreateTime),
                ChannelData = wechatRequest,
                Attachments = new List<Attachment>(),
                Entities = new List<Entity>(),
            };

            if (wechatRequest is RequestMessage requestMessage)
            {
                activity.Id = requestMessage.MsgId.ToString(CultureInfo.InvariantCulture);
                activity.Type = ActivityTypes.Message;
            }
            else
            {
                // Event message don't have Id;
                activity.Id = new Guid().ToString();
                activity.Type = ActivityTypes.Event;
            }

            return activity;
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
        /// Convert suggestion actions to WeChat message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="suggestedActions">the suggestions.</param>
        /// <returns>Response message to WeChat.</returns>
        private static IResponseMessageBase SuggestionActionsToWeChatMessage(IMessageActivity activity, SuggestedActions suggestedActions)
        {
            var response = CreateTextResponseFromMessageActivity(activity);
            var actionString = SuggestedActionsToText(suggestedActions.Actions);
            response.Content = actionString;
            return response;
        }

        /// <summary>
        /// Process thumbnail card and return the WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="thumbnailCard">Thumbnail card instance need to be converted.</param>
        /// <returns>WeChat response message.</returns>
        private static IList<IResponseMessageBase> ProcessThumbnailCard(IMessageActivity activity, ThumbnailCard thumbnailCard)
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
                var buttonString = ButtonsToText(thumbnailCard.Buttons);
                messages.AddRange(GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        /// <summary>
        /// Downgrade ReceiptCard into text replies for low-fi channels.
        /// </summary>
        /// <returns>List of response message to WeChat.</returns>
        private static IList<IResponseMessageBase> ProcessReceiptCard(IMessageActivity activity, ReceiptCard receiptCard)
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
        private static List<IResponseMessageBase> ProcessSigninCard(SigninCard signinCard, IMessageActivity activity)
        {
            var messages = new List<IResponseMessageBase>();

            if (signinCard.Buttons != null)
            {
                var buttonText = ButtonToText(signinCard.Buttons.First());
                var messageContent = AddLine(signinCard.Text, buttonText);

                messages.AddRange(GetChunkedMessages(activity, messageContent));
            }

            return messages;
        }

        /// <summary>
        /// Downgrade OAuthCard into text replies for low-fi channels.
        /// </summary>
        private static List<IResponseMessageBase> ProcessOAuthCard(OAuthCard oauthCard, IMessageActivity activity)
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
        /// Get fixed media type before upload media to WeChat, ensure upload successful.
        /// </summary>
        /// <param name="type">The type of the media, typically it should be a mime type.</param>
        /// <returns>The fixed media type WeChat supported.</returns>
        private static string GetFixedMeidaType(string type)
        {
            var fixedType = string.Empty;
            if (type.Contains(MediaTypes.Image))
            {
                fixedType = MediaTypes.Image;
            }

            if (type.Contains(MediaTypes.Video))
            {
                fixedType = MediaTypes.Video;
            }

            if (type.Contains(MediaTypes.Audio))
            {
                fixedType = MediaTypes.Voice;
            }

            return fixedType;
        }

        private async Task<IList<IResponseMessageBase>> ProcessAnimationCardAsync(IMessageActivity activity, AnimationCard animationCard)
        {
            var messages = new List<IResponseMessageBase>();

            // Generate body
            var body = animationCard.Title;
            body = AddLine(body, animationCard.Subtitle);
            body = AddLine(body, animationCard.Text);

            // Add buttons
            if (animationCard.Buttons != null && animationCard.Buttons.Count > 0)
            {
                body = AddLine(body, ButtonsToText(animationCard.Buttons));
            }

            // Add image
            if (!string.IsNullOrEmpty(animationCard.Image?.Url))
            {
                messages.Add(await MediaContentToWeChatResponse(activity, animationCard.Image.Alt, animationCard.Image.Url, MediaTypes.Image).ConfigureAwait(false));
            }

            // Add mediaUrls
            foreach (var mediaUrl in animationCard.Media ?? new List<MediaUrl>())
            {
                messages.Add(await MediaContentToWeChatResponse(activity, mediaUrl.Profile, mediaUrl.Url, MediaTypes.Gif).ConfigureAwait(false));
            }

            messages.AddRange(GetChunkedMessages(activity, body));

            return messages;
        }

        /// <summary>
        /// render a adaptiveCard into text replies for low-fi channels.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="adaptiveCard">Adaptive card instance need to be converted.</param>
        /// <param name="name">Name of the adaptive card attachment.</param>
        /// <returns>WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAdaptiveCardAsync(IMessageActivity activity, AdaptiveCard adaptiveCard, string name)
        {
            var messages = new List<IResponseMessageBase>();

            try
            {
                var news = await CreateNewsFromAdaptiveCard(activity, adaptiveCard, name).ConfigureAwait(false);

                // TODO: Upload news image must be persistent media.
                var uploadResult = await _wechatClient.UploadNewsAsync(new News[] { news }, _uploadTemporaryMedia).ConfigureAwait(false);
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
            var uploadResult = await _wechatClient.UploadNewsAsync(new News[] { news }, _uploadTemporaryMedia).ConfigureAwait(false);
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
        /// Convert video card to WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="videoCard">Video card instance need to be converted.</param>
        /// <returns>WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessVideoCardAsync(IMessageActivity activity, VideoCard videoCard)
        {
            var messages = new List<IResponseMessageBase>();

            var body = videoCard.Subtitle;
            body = AddLine(body, videoCard.Text);
            Video video = null;

            // upload thumbnail image.
            if (!string.IsNullOrEmpty(videoCard.Image?.Url))
            {
                // TODO: WeChat doc have thumb_media_id for video mesasge, but not implemented in current package.
                var reponseList = await MediaContentToWeChatResponse(activity, videoCard.Title, videoCard.Media[0].Url, MediaTypes.Video).ConfigureAwait(false);
                if (reponseList is VideoResponse videoResponse)
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
                buttonString = ButtonsToText(videoCard.Buttons);
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
                var reponseList = await MediaContentToWeChatResponse(activity, audioCard.Image.Alt, audioCard.Image.Url, MediaTypes.Image).ConfigureAwait(false);
                if (reponseList is ImageResponse imageResponse)
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

        /// <summary>
        /// Upload media to WeChat and map to WeChat Response message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="name">Media's name.</param>
        /// <param name="content">Media content, can be a url or base64 string.</param>
        /// <param name="contentType">Media content type.</param>
        /// <returns>Response message to WeChat.</returns>
        private async Task<IResponseMessageBase> MediaContentToWeChatResponse(IMessageActivity activity, string name, string content, string contentType)
        {
            var attachmentData = await CreateAttachmentDataAsync(name, content, contentType).ConfigureAwait(false);

            // document said mp news should not use temp media_id, but is working actually.
            var uploadResult = await _wechatClient.UploadMediaAsync(attachmentData, _uploadTemporaryMedia).ConfigureAwait(false);
            return CreateMediaResponse(activity, uploadResult.MediaId, attachmentData.Type);
        }

        /// <summary>
        /// Create Attachment data object using the give parameters.
        /// </summary>
        /// <param name="name">Attachment name.</param>
        /// <param name="content">Attachment content url.</param>
        /// <param name="contentType">Attachment content type.</param>
        /// <returns>A valid AttachmentData instance.</returns>
        private async Task<AttachmentData> CreateAttachmentDataAsync(string name, string content, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType), "Content type can not be null.");
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "Content url can not be null.");
            }

            // ContentUrl can contain a url or dataUrl of the form "data:image/jpeg;base64,XXXXXXXXX..."
            byte[] bytesData;
            if (AttachmentHelper.IsUrl(content))
            {
                bytesData = await _wechatClient.SendHttpRequestAsync(HttpMethod.Get, content).ConfigureAwait(false);
            }
            else
            {
                bytesData = AttachmentHelper.DecodeBase64String(content, out contentType);
            }

            name = name ?? new Guid().ToString();
            contentType = GetFixedMeidaType(contentType);

            return new AttachmentData(contentType, name, bytesData, bytesData);
        }
    }
}
