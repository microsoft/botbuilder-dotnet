// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

#if SIGNASSEMBLY
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.WeChat.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#else
[assembly: InternalsVisibleTo("Microsoft.Bot.Builder.Adapters.WeChat.Tests")]
#endif

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    /// <summary>
    /// WeChat massage mapper that can convert the message from a WeChat request to Activity or Activity to WeChat response.
    /// </summary>
    /// <remarks>
    /// WeChat message mapper will help create the bot activity and WeChat response.
    /// When deal with the media attachments or cards, mapper will upload the data first to aquire the acceptable media url.
    /// </remarks>
    internal class WeChatMessageMapper
    {
        /// <summary>
        /// Key of content source url.
        /// </summary>
        private const string ContentSourceUrlKey = "contentSourceUrl";

        /// <summary>
        /// Key of cover image.
        /// </summary>
        private const string CoverImageUrlKey = "coverImageUrl";

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
        public async Task<Activity> ToConnectorMessage(IRequestMessageBase wechatRequest)
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
                    responseMessageList.AddRange(GetFixedMessages(messageActivity, messageActivity.Text));

                    // Chunk message into pieces as necessary
                    if (messageActivity.SuggestedActions?.Actions != null)
                    {
                        responseMessageList.AddRange(ProcessCardActions(messageActivity, messageActivity.SuggestedActions.Actions));
                    }

                    foreach (var attachment in messageActivity.Attachments ?? new List<Attachment>())
                    {
                        IList<IResponseMessageBase> attachmentResponses = new List<IResponseMessageBase>();
                        if (attachment.ContentType == AdaptiveCard.ContentType || attachment.ContentType == "application/adaptive-card")
                        {
                            attachmentResponses = await ProcessAdaptiveCardAsync(messageActivity, attachment).ConfigureAwait(false);
                        }
                        else if (attachment.ContentType == AudioCard.ContentType)
                        {
                            attachmentResponses = await ProcessAudioCardAsync(messageActivity, attachment).ConfigureAwait(false);
                        }
                        else if (attachment.ContentType == AnimationCard.ContentType)
                        {
                            attachmentResponses = await ProcessAnimationCardAsync(messageActivity, attachment).ConfigureAwait(false);
                        }
                        else if (attachment.ContentType == HeroCard.ContentType)
                        {
                            attachmentResponses = await ProcessHeroCardAsync(messageActivity, attachment).ConfigureAwait(false);
                        }
                        else if (attachment.ContentType == ThumbnailCard.ContentType)
                        {
                            attachmentResponses = ProcessThumbnailCard(messageActivity, attachment);
                        }
                        else if (attachment.ContentType == ReceiptCard.ContentType)
                        {
                            attachmentResponses = ProcessReceiptCard(messageActivity, attachment);
                        }
                        else if (attachment.ContentType == SigninCard.ContentType)
                        {
                            attachmentResponses = ProcessSigninCard(messageActivity, attachment);
                        }
                        else if (attachment.ContentType == OAuthCard.ContentType)
                        {
                            attachmentResponses = ProcessOAuthCard(messageActivity, attachment);
                        }
                        else if (attachment.ContentType == VideoCard.ContentType)
                        {
                            attachmentResponses = await ProcessVideoCardAsync(messageActivity, attachment).ConfigureAwait(false);
                        }
                        else if (attachment != null &&
                                    (!string.IsNullOrEmpty(attachment.ContentUrl) ||
                                     attachment.Content != null ||
                                     !string.IsNullOrEmpty(attachment.ThumbnailUrl)))
                        {
                            attachmentResponses = await ProcessAttachmentAsync(messageActivity, attachment).ConfigureAwait(false);
                        }
                        else
                        {
                            // Log unsupported attachment.
                            _logger.LogInformation($"Unsupported attachment type: {attachment.ContentType}");
                        }

                        responseMessageList.AddRange(attachmentResponses);
                    }
                }
                else if (activity is IEventActivity eventActivity)
                {
                    // WeChat won't accept event type, just log and bypass.
                    _logger.LogInformation("Receive an event activity which WeChat is not supported.", eventActivity);
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
        /// Create a media type response message using mediaId and acitivity.
        /// </summary>
        /// <param name="activity">Activity from bot.</param>
        /// <param name="mediaId">MediaId from WeChat.</param>
        /// <param name="type">Media type.</param>
        /// <returns>Media resposne such as ImageResponse, etc.</returns>
        private static ResponseMessage CreateMediaResponse(IActivity activity, string mediaId, string type)
        {
            if (type.Contains(MediaTypes.Image))
            {
                return new ImageResponse(activity.From.Id, activity.Recipient.Id, mediaId);
            }

            if (type.Contains(MediaTypes.Video))
            {
                return new VideoResponse(activity.From.Id, activity.Recipient.Id, new Video(mediaId));
            }

            if (type.Contains(MediaTypes.Audio))
            {
                return new VoiceResponse(activity.From.Id, activity.Recipient.Id, mediaId);
            }

            throw new Exception($"Unsupported media type: {type}");
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
        /// Chunk the text message and return it as WeChat response.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="text">Text content need to be chunked.</param>
        /// <returns>Response message list.</returns>
        private static IList<IResponseMessageBase> GetFixedMessages(IMessageActivity activity, string text)
        {
            var responses = new List<IResponseMessageBase>();
            if (string.IsNullOrEmpty(text))
            {
                return responses;
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

            responses.Add(new TextResponse(activity.From.Id, activity.Recipient.Id, text));
            return responses;
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
                activity.Id = Guid.NewGuid().ToString();
                activity.Type = ActivityTypes.Event;
            }

            return activity;
        }

        /// <summary>
        /// Convert all buttons in a message to text string for channels that can't display button.
        /// </summary>
        /// <param name="actions">CardAction list.</param>
        /// <returns>WeChatResponses converted from card actions.</returns>
        private static IList<IResponseMessageBase> ProcessCardActions(IMessageActivity activity, IList<CardAction> actions)
        {
            var messages = new List<IResponseMessageBase>();
            var menuItems = new List<MenuItem>();
            var text = string.Empty;
            foreach (var action in actions ?? new List<CardAction>())
            {
                // Convert action to a tag if its a url other wise convert it to message menu.
                var actionContent = action.DisplayText ?? action.Title ?? action.Text;
                if (AttachmentHelper.IsUrl(action.Value))
                {
                    text = AddLine(text, $"<a href=\"{action.Value}\">{actionContent}</a>");
                }
                else
                {
                    var menuItem = new MenuItem
                    {
                        Id = action.Value == null ? actionContent : action.Value.ToString(),
                        Content = actionContent,
                    };
                    menuItems.Add(menuItem);
                }
            }

            if (menuItems.Count != 0)
            {
                var menuResponse = new MessageMenuResponse()
                {
                    FromUserName = activity.From.Id,
                    ToUserName = activity.Recipient.Id,
                    MessageMenu = new MessageMenu()
                    {
                        HeaderContent = string.Empty,
                        MenuItems = menuItems,
                        TailContent = string.Empty,
                    },
                };
                messages.Add(menuResponse);
            }

            if (!string.IsNullOrEmpty(text))
            {
                var textResponse = new TextResponse(activity.From.Id, activity.Recipient.Id, text);
                messages.Add(textResponse);
            }

            return messages;
        }

        /// <summary>
        /// Process thumbnail card and return the WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains animation card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private static IList<IResponseMessageBase> ProcessThumbnailCard(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var thumbnailCard = attachment.ContentAs<ThumbnailCard>();

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
            var newsResponse = new NewsResponse(activity.From.Id, activity.Recipient.Id, new List<Article>() { article });
            messages.Add(newsResponse);
            messages.AddRange(ProcessCardActions(activity, thumbnailCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Downgrade ReceiptCard into text replies for low-fi channels.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains animation card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private static IList<IResponseMessageBase> ProcessReceiptCard(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var receiptCard = attachment.ContentAs<ReceiptCard>();

            // Build text portion of receipt
            var body = receiptCard.Title;
            foreach (var fact in receiptCard.Facts ?? new List<Fact>())
            {
                body = AddLine(body, $"{fact.Key}:  {fact.Value}");
            }

            // Add items, grouping text only ones into a single post
            foreach (var item in receiptCard.Items ?? new List<ReceiptItem>())
            {
                body = AddLine(body, $"{item.Title}: {item.Price}");
                body = AddLine(body, item.Subtitle);
                body = AddLine(body, item.Text);
            }

            // Add totals
            body = AddLine(body, $"Tax:  {receiptCard.Tax}");
            body = AddLine(body, $"Total:  {receiptCard.Total}");
            messages.AddRange(GetFixedMessages(activity, body));
            messages.AddRange(ProcessCardActions(activity, receiptCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Downgrade SigninCard into text replies for low-fi channels.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains animation card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private static IList<IResponseMessageBase> ProcessSigninCard(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var signinCard = attachment.ContentAs<SigninCard>();
            messages.AddRange(GetFixedMessages(activity, signinCard.Text));
            messages.AddRange(ProcessCardActions(activity, signinCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Downgrade OAuthCard into text replies for low-fi channels.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains animation card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private static List<IResponseMessageBase> ProcessOAuthCard(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var oauthCard = attachment.ContentAs<OAuthCard>();

            // Add text
            messages.AddRange(GetFixedMessages(activity, oauthCard.Text));
            messages.AddRange(ProcessCardActions(activity, oauthCard.Buttons));
            return messages;
        }

        /// <summary>
        /// Process all types of general attachment.
        /// </summary>
        /// <param name="activity">The message activity.</param>
        /// <param name="attachment">The attachment object need to be processed.</param>
        /// <returns>List of WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAttachmentAsync(IMessageActivity activity, Attachment attachment)
        {
            var responseList = new List<IResponseMessageBase>();

            // Create media response directly if mediaId provide by user.
            attachment.Properties.TryGetValue("MediaId", StringComparison.InvariantCultureIgnoreCase, out var mediaId);
            if (mediaId != null)
            {
                responseList.Add(CreateMediaResponse(activity, mediaId.ToString(), attachment.ContentType));
                return responseList;
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
                responseList.Add(await MediaContentToWeChatResponse(activity, attachment.Name, attachment.Content.ToString(), attachment.ContentType).ConfigureAwait(false));
            }
            else if (attachment.Content != null)
            {
                responseList.AddRange(GetFixedMessages(activity, attachment.Content.ToString()));
            }

            return responseList;
        }

        /// <summary>
        /// Create a News instance use hero card.
        /// </summary>
        /// <param name="activity">Message activity received from bot.</param>
        /// <param name="heroCard">Hero card instance.</param>
        /// <returns>A new instance of News create by hero card.</returns>
        private async Task<News> CreateNewsFromHeroCard(IMessageActivity activity, HeroCard heroCard)
        {
            if (heroCard.Images == null || heroCard.Images.Count == 0)
            {
                throw new ArgumentException("Image is required for news.", nameof(heroCard));
            }

            var news = new News
            {
                Author = activity.From.Name,
                Description = heroCard.Subtitle,
                Content = heroCard.Text,
                Title = heroCard.Title,
                ShowCoverPicture = "0",

                // Hero card don't have original url, but it's required by WeChat.
                // Let user use openurl action as tap action instead.
                ContentSourceUrl = heroCard.Tap?.Value.ToString() ?? heroCard.Images.FirstOrDefault().Url,
            };

            foreach (var image in heroCard.Images)
            {
                // MP news image is required and can not be a temporary media.
                var mediaMessage = await MediaContentToWeChatResponse(activity, image.Alt, image.Url, MediaTypes.Image).ConfigureAwait(false);
                news.ThumbMediaId = (mediaMessage as ImageResponse).Image.MediaId;
                news.ThumbUrl = image.Url;
            }

            return news;
        }

        /// <summary>
        /// Create WeChat news instance from the given adaptive card.
        /// </summary>
        /// <param name="activity">Message activity received from bot.</param>
        /// <param name="adaptiveCard">Adaptive card instance.</param>
        /// <param name="title">Title or name of the card attachment.</param>
        /// <returns>A <seealso cref="News"/> converted from adaptive card.</returns>
        private async Task<News> CreateNewsFromAdaptiveCard(IMessageActivity activity, AdaptiveCard adaptiveCard, string title)
        {
            try
            {
                if (!adaptiveCard.AdditionalProperties.ContainsKey(CoverImageUrlKey))
                {
                    throw new ArgumentException("Cover image is required.", nameof(adaptiveCard));
                }

                if (!adaptiveCard.AdditionalProperties.ContainsKey(ContentSourceUrlKey))
                {
                    throw new ArgumentException("Content source URL is required.", nameof(adaptiveCard));
                }

                var renderer = new AdaptiveCardRenderer();
                var schemaVersion = renderer.SupportedSchemaVersion;
                var converImageUrl = adaptiveCard.AdditionalProperties[CoverImageUrlKey].ToString();
                var attachmentData = await CreateAttachmentDataAsync(title ?? activity.Text, converImageUrl, MediaTypes.Image).ConfigureAwait(false);
                var thumbMediaId = (await _wechatClient.UploadMediaAsync(attachmentData, false).ConfigureAwait(false)).MediaId;

                // Replace all image URL to WeChat acceptable URL
                foreach (var element in adaptiveCard.Body)
                {
                    await ReplaceAdaptiveImageUri(element).ConfigureAwait(false);
                }

                // Render the card
                var renderedCard = renderer.RenderCard(adaptiveCard);
                var html = renderedCard.Html;

                // (Optional) Check for any renderer warnings
                // This includes things like an unknown element type found in the card
                // Or the card exceeded the maximum number of supported actions, etc
                var warnings = renderedCard.Warnings;
                var news = new News
                {
                    Author = activity.From.Name,
                    Description = adaptiveCard.Speak ?? adaptiveCard.FallbackText,
                    Content = html.ToString(),
                    Title = title,

                    // Set not should cover, because adaptive card don't have a cover.
                    ShowCoverPicture = "0",
                    ContentSourceUrl = adaptiveCard.AdditionalProperties[ContentSourceUrlKey].ToString(),
                    ThumbMediaId = thumbMediaId,
                };

                return news;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Process adaptive card failed.");
                throw;
            }
        }

        /// <summary>
        /// WeChat won't accept the image link outside its domain, recursive upload the image to get the url first.
        /// </summary>
        /// <param name="element">Adaptive card element.</param>
        /// <returns>Task of replace the adaptive card image uri.</returns>
        private async Task ReplaceAdaptiveImageUri(AdaptiveElement element)
        {
            if (element is AdaptiveImage adaptiveImage)
            {
                var attachmentData = await CreateAttachmentDataAsync(adaptiveImage.AltText ?? adaptiveImage.Id, adaptiveImage.Url.AbsoluteUri, adaptiveImage.Type).ConfigureAwait(false);
                var uploadResult = await _wechatClient.UploadNewsImageAsync(attachmentData).ConfigureAwait(false) as UploadPersistentMediaResult;
                adaptiveImage.Url = new Uri(uploadResult.Url);
                return;
            }

            if (element is AdaptiveImageSet imageSet)
            {
                foreach (var image in imageSet.Images)
                {
                    await ReplaceAdaptiveImageUri(image).ConfigureAwait(false);
                }
            }
            else if (element is AdaptiveContainer container)
            {
                foreach (var item in container.Items)
                {
                    await ReplaceAdaptiveImageUri(item).ConfigureAwait(false);
                }
            }
            else if (element is AdaptiveColumnSet columnSet)
            {
                foreach (var item in columnSet.Columns)
                {
                    await ReplaceAdaptiveImageUri(item).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Process animation card and convert it to WeChat response messages.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains animation card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAnimationCardAsync(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var animationCard = attachment.ContentAs<AnimationCard>();

            // Add text body
            var body = animationCard.Title;
            body = AddLine(body, animationCard.Subtitle);
            body = AddLine(body, animationCard.Text);
            messages.AddRange(GetFixedMessages(activity, body));

            // Add image
            if (!string.IsNullOrEmpty(animationCard.Image?.Url))
            {
                messages.Add(await MediaContentToWeChatResponse(activity, animationCard.Image.Alt, animationCard.Image.Url, MediaTypes.Image).ConfigureAwait(false));
            }

            // Add mediaUrls
            foreach (var mediaUrl in animationCard.Media ?? new List<MediaUrl>())
            {
                messages.Add(await MediaContentToWeChatResponse(activity, mediaUrl.Profile, mediaUrl.Url, MediaTypes.Image).ConfigureAwait(false));
            }

            // Add buttons
            messages.AddRange(ProcessCardActions(activity, animationCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Process adaptive card and convert it into WeChat response messages.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains adaptive card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAdaptiveCardAsync(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var adaptiveCard = attachment.ContentAs<AdaptiveCard>();
            try
            {
                var news = await CreateNewsFromAdaptiveCard(activity, adaptiveCard, attachment.Name).ConfigureAwait(false);

                // TODO: Upload news image must be persistent media.
                var uploadResult = await _wechatClient.UploadNewsAsync(new News[] { news }, false).ConfigureAwait(false);
                var mpnews = new MPNewsResponse(activity.From.Id, activity.Recipient.Id, uploadResult.MediaId);
                messages.Add(mpnews);
            }
#pragma warning disable CA1031 // Do not catch general exception types, use fallback text instead.
            catch
#pragma warning disable CA1031 // Do not catch general exception types, use fallback text instead.
            {
                _logger.LogInformation("Convert adaptive card failed.");
                messages.AddRange(GetFixedMessages(activity, adaptiveCard.FallbackText));
            }

            return messages;
        }

        /// <summary>
        /// Convert hero card to WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains hero card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessHeroCardAsync(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var heroCard = attachment.ContentAs<HeroCard>();
            var news = await CreateNewsFromHeroCard(activity, heroCard).ConfigureAwait(false);
            var uploadResult = await _wechatClient.UploadNewsAsync(new News[] { news }, _uploadTemporaryMedia).ConfigureAwait(false);
            var mpnews = new MPNewsResponse(activity.From.Id, activity.Recipient.Id, uploadResult.MediaId);
            messages.Add(mpnews);
            messages.AddRange(ProcessCardActions(activity, heroCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Convert video card to WeChat response message.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains video card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessVideoCardAsync(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var videoCard = attachment.ContentAs<VideoCard>();

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

            messages.Add(new VideoResponse(activity.From.Id, activity.Recipient.Id, video));
            messages.AddRange(ProcessCardActions(activity, videoCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Convert audio card as music resposne.
        /// Thumbnail image size limitation is not clear.
        /// </summary>
        /// <param name="activity">Message activity from bot.</param>
        /// <param name="attachment">An <see cref="Attachment"/> contains audio card content.</param>
        /// <returns>List of WeChat response message.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAudioCardAsync(IMessageActivity activity, Attachment attachment)
        {
            var messages = new List<IResponseMessageBase>();
            var audioCard = attachment.ContentAs<AudioCard>();

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

            var musicResponse = new MusicResponse(activity.From.Id, activity.Recipient.Id, music);
            messages.Add(musicResponse);
            messages.AddRange(ProcessCardActions(activity, audioCard.Buttons));

            return messages;
        }

        /// <summary>
        /// Upload media to WeChat and map to WeChat Response message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="name">Media's name.</param>
        /// <param name="content">Media content, can be a url or base64 string.</param>
        /// <param name="contentType">Media content type.</param>
        /// <returns>WeChat response message.</returns>
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
                bytesData = await _wechatClient.SendHttpRequestAsync(HttpMethod.Get, content, timeout: 60000).ConfigureAwait(false);
            }
            else
            {
                bytesData = AttachmentHelper.DecodeBase64String(content, out contentType);
            }

            name = name ?? Guid.NewGuid().ToString();

            // should be lower by WeChat.
#pragma warning disable CA1308
            contentType = contentType.ToLowerInvariant();
#pragma warning restore CA1308
            return new AttachmentData(contentType, name, bytesData, bytesData);
        }
    }
}
