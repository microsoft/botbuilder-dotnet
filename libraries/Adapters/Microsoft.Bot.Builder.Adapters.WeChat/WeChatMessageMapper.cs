// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Rendering;
using AdaptiveCards.Rendering.Html;
using Microsoft.Bot.Builder.Adapters.WeChat.Extensions;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request.Event;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.MarkedNet;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatMessageMapper : IWeChatMessageMapper
    {
        private readonly WeChatClient wechatClient;
        private readonly WeChatLogger logger;
        private readonly bool uploadTemporaryMedia;

        public WeChatMessageMapper(IConfiguration configuration, WeChatClient wechatClient, WeChatLogger logger = null)
        {
            this.wechatClient = wechatClient;
            this.uploadTemporaryMedia = configuration.GetSection("WeChatSetting")?.GetValue<bool>("UploadTemporaryMedia") ?? true;
            this.logger = logger ?? WeChatLogger.Instance;
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
                        ContentType = MimeTypesMap.GetMimeType(imageRequest.PicUrl) ?? MediaType.Image,
                        ContentUrl = imageRequest.PicUrl,
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is VoiceRequest voiceRequest)
                {
                    messageActivity.Text = voiceRequest.Recognition;
                    var attachment = new Attachment
                    {
                        ContentType = MimeTypesMap.GetMimeType(voiceRequest.Format) ?? MediaType.Voice,
                        ContentUrl = await this.wechatClient.GetMediaUrlAsync(voiceRequest.MediaId).ConfigureAwait(false),
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is VideoRequest videoRequest)
                {
                    var attachment = new Attachment
                    {
                        // video request don't have format, type will be value.
                        ContentType = MediaType.Video,
                        ContentUrl = await this.wechatClient.GetMediaUrlAsync(videoRequest.MediaId).ConfigureAwait(false),
                        ThumbnailUrl = await this.wechatClient.GetMediaUrlAsync(videoRequest.ThumbMediaId).ConfigureAwait(false),
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is ShortVideoRequest shortVideoRequest)
                {
                    var attachment = new Attachment
                    {
                        ContentType = MediaType.Video,
                        ContentUrl = await this.wechatClient.GetMediaUrlAsync(shortVideoRequest.MediaId).ConfigureAwait(false),
                        ThumbnailUrl = await this.wechatClient.GetMediaUrlAsync(shortVideoRequest.ThumbMediaId).ConfigureAwait(false),
                    };
                    messageActivity.Attachments.Add(attachment);
                }
                else if (request is LocationRequest locationRequest)
                {
                    var geo = new GeoCoordinates
                    {
                        Name = locationRequest.Label,
                        Latitude = locationRequest.Location_X,
                        Longitude = locationRequest.Location_Y,
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
        /// <param name="secretInfo">SecretInfo contains token, AES key, etc.</param>
        /// <returns>WeChat message list.</returns>
        public async Task<IList<IResponseMessageBase>> ToWeChatMessages(IActivity activity, SecretInfo secretInfo)
        {
            try
            {
                var responseMessageList = new List<IResponseMessageBase>();

                if (activity is IMessageActivity messageActivity)
                {
                    var text = this.ParseActivityText(messageActivity);

                    // Chunk message into pieces as necessary
                    responseMessageList.AddRange(this.GetChunkedMessages(messageActivity, text));

                    // Process suggested actions if any
                    if (messageActivity.SuggestedActions?.Actions?.Any() == true)
                    {
                        responseMessageList.Add(this.SuggestionActionsToWeChatMessage(messageActivity, messageActivity.SuggestedActions));
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
                            var hostConfig = new AdaptiveHostConfig();
                            hostConfig.ContainerStyles.Default.BackgroundColor = "#00FFFFFF"; // transparent
                            hostConfig.FontFamily = "Segoe UI";
                            hostConfig.FontSizes.Small = 13;
                            hostConfig.FontSizes.Default = 15;
                            hostConfig.FontSizes.Medium = 17;
                            hostConfig.FontSizes.Large = 20;
                            hostConfig.FontSizes.ExtraLarge = 23;
                            hostConfig.SupportsInteractivity = false;
                            responseMessageList.AddRange(await this.ProcessAdaptiveCardAsync(messageActivity, adaptiveCard, hostConfig, secretInfo).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == AudioCard.ContentType)
                        {
                            var audioCard = attachment.ContentAs<AudioCard>();
                            responseMessageList.AddRange(await this.ProcessAudioCardAsync(messageActivity, audioCard, secretInfo).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == AnimationCard.ContentType)
                        {
                            var animationCard = attachment.ContentAs<AnimationCard>();
                            responseMessageList.AddRange(await this.ProcessAnimationCardAsync(messageActivity, animationCard, attachment, secretInfo).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == HeroCard.ContentType)
                        {
                            var heroCard = attachment.ContentAs<HeroCard>();
                            responseMessageList.AddRange(await this.ProcessHeroCardAsync(messageActivity, heroCard, attachment, secretInfo).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == ThumbnailCard.ContentType)
                        {
                            var thumbnailCard = attachment.ContentAs<ThumbnailCard>();
                            responseMessageList.AddRange(this.ProcessThumbnailCard(messageActivity, thumbnailCard, attachment, secretInfo));
                        }
                        else if (attachment.ContentType == ReceiptCard.ContentType)
                        {
                            var receiptCard = attachment.ContentAs<ReceiptCard>();
                            responseMessageList.AddRange(await this.ProcessReceiptCardAsync(messageActivity, receiptCard, attachment, secretInfo).ConfigureAwait(false));
                        }
                        else if (attachment.ContentType == SigninCard.ContentType)
                        {
                            var signinCard = attachment.ContentAs<SigninCard>();
                            responseMessageList.AddRange(this.ProcessSigninCard(signinCard, messageActivity));
                        }
                        else if (attachment.ContentType == OAuthCard.ContentType)
                        {
                            var oauthCard = attachment.ContentAs<OAuthCard>();
                            responseMessageList.AddRange(this.ProcessOAuthCard(oauthCard, messageActivity));
                        }
                        else if (attachment.ContentType == VideoCard.ContentType)
                        {
                            var videoCard = attachment.ContentAs<VideoCard>();
                            responseMessageList.AddRange(await this.ProcessVideoCardAsync(messageActivity, videoCard, attachment, secretInfo).ConfigureAwait(false));
                        }
                        else if (attachment != null &&
                                    (!string.IsNullOrEmpty(attachment.ContentUrl) ||
                                     attachment.Content != null ||
                                     !string.IsNullOrEmpty(attachment.ThumbnailUrl)))
                        {
                            responseMessageList.AddRange(await this.ProcessAttachmentAsync(messageActivity, attachment, secretInfo).ConfigureAwait(false));
                        }
                        else
                        {
                            this.logger.TrackTrace($"Unsupported content type {attachment.ContentType}");
                        }
                    }
                }
                else if (activity is IEventActivity eventActivity)
                {
                    // TODO: handle event message from bot
                    // WeChat won't accept event type.
                }

                return responseMessageList;
            }
            catch (Exception e)
            {
                this.logger.TrackException("Parse to wechat message failed", e);
                throw e;
            }
        }

        /// <summary>
        /// Convert all buttons in a message to text string for channels that can't display button.
        /// </summary>
        /// <param name="actions">CardAction list.</param>
        /// <param name="actionToString">Specific way of converting actions to string was specified.</param>
        /// <returns>CardAction as string.</returns>
        public string ButtonsToText(IList<CardAction> actions, Func<CardAction, string> actionToString = null)
        {
            // Convert any options to text
            var text = string.Empty;
            var newline = "\r\n";
            actions = actions ?? new List<CardAction>();
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (i > 0)
                {
                    text += newline;
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
                    text += this.ButtonToText(action, index);
                }
            }

            return text;
        }

        public string ParseActivityText(IMessageActivity activity, Marked marked = null)
        {
            try
            {
                var outText = activity.Text;

                if (string.IsNullOrWhiteSpace(outText))
                {
                    return outText;
                }

                if (marked != null)
                {
                    return marked.Parse(outText);
                }

                return outText;
            }
            catch
            {
                // Don't fail if malformed data is passed in, just return as text
                return activity.Text;
            }
        }

        /// <summary>
        /// Convert buttons to text string for channels that can't display button.
        /// </summary>
        /// <param name="button">The Card Action.</param>
        /// <param name="index">Index of current action in action list.</param>
        /// <returns>Card action as string.</returns>
        public string ButtonToText(CardAction button, int index = -1)
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

        public async Task<News> CreateNewsFromHeroCard(IMessageActivity activity, HeroCard heroCard, SecretInfo secretInfo)
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
                ContentSourceUrl = Constants.DefaultContentUrl,
            };

            foreach (var image in heroCard.Images ?? new CardImage[] { })
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = image.Url,
                    ContentType = MediaType.Image,
                    Name = image.Alt,
                };

                // Mp news image is required and can not be a temporary media.
                var mediaMessage = await this.AttachmentToWeChatMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false);
                news.ThumbMediaId = (mediaMessage.FirstOrDefault() as ImageResponse).Image.MediaId;
                news.ThumbUrl = image.Url;
            }

            return news;
        }

        public async Task<News> CreateNewsFromAdaptiveCard(IMessageActivity activity, AdaptiveCard card, SecretInfo secretInfo)
        {
            try
            {
                var renderer = new AdaptiveCardRenderer();
                var schemaVersion = renderer.SupportedSchemaVersion;

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
                    Title = card.Title,

                    // Set not should cover, because adaptive card don't have a cover.
                    ShowCoverPicture = "0",

                    // TODO: replace with url
                    ContentSourceUrl = Constants.DefaultContentUrl,
                };
                var imageUrl = card.BackgroundImage?.AbsolutePath;
                var imageName = new Guid().ToString();
                var surrogate = new Attachment()
                {
                    ContentUrl = imageUrl,
                    ContentType = MediaType.Image,
                    Name = imageName,
                };

                // Mp news image is required and can not be a temporary media.
                var mediaMessage = await this.AttachmentToWeChatMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false);
                news.ThumbMediaId = (mediaMessage.FirstOrDefault() as ImageResponse).Image.MediaId;
                news.ThumbUrl = imageUrl;
                return news;
            }
            catch (AdaptiveException ex)
            {
                // Failed rendering
                this.logger.TrackException("Failed to rending Adaptive card", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                // upload graphic message failed.
                this.logger.TrackException("Error when uploading adaptive card", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Convert suggestion actions to wechat message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="suggestedActions">the suggestions.</param>
        /// <returns>Response message to WeChat.</returns>
        private IResponseMessageBase SuggestionActionsToWeChatMessage(IMessageActivity activity, SuggestedActions suggestedActions)
        {
            var response = this.CreateTextResponseFromMessageActivity(activity);
            var actionString = this.SuggestedActionsToText(suggestedActions.Actions);
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
                this.logger.TrackTrace($"InValid AttachmentData.");
                return null;
            }

            var mediaId = await this.UploadMediaAsync(attachmentData);
            return CreateMediaResponse(mediaId, attachmentData.Type, activity);
        }

        private Task<string> UploadToGetMediaUrl(SecretInfo secretInfo, AttachmentData attachmentData, UploadMediaType type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Upload the media to WeChat.
        /// </summary>
        /// <param name="attachmentData">AttachmentData need to be uploaded.</param>
        /// <returns>Media id.</returns>
        private async Task<string> UploadMediaAsync(AttachmentData attachmentData)
        {
            string type = string.Empty;
            if (attachmentData.Type.Contains(MediaType.Image))
            {
                type = UploadMediaType.Image;
            }

            if (attachmentData.Type.Contains(MediaType.Video))
            {
                type = UploadMediaType.Video;
            }

            if (attachmentData.Type.Contains(MediaType.Audio))
            {
                type = UploadMediaType.Voice;
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new NotSupportedException($"Attachment type: {attachmentData.Type} not supported yet.");
            }

            string mediaId;

            // document said mp news should not use temp media_id, but is working actually.
            if (this.uploadTemporaryMedia)
            {
                var uploadResult = await this.wechatClient.UploadTemporaryMediaAsync(type, attachmentData);
                mediaId = uploadResult.MediaId;
            }
            else
            {
                var uploadResult = await this.wechatClient.UploadPersistentMediaAsync(type, attachmentData);
                mediaId = uploadResult.MediaId;
            }

            return mediaId;
        }

        /// <summary>
        /// Upload media to WeChat and map to WeChat Response message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="attachment">Current attachment.</param>
        /// <param name="secretInfo">secretInfo from wechat.</param>
        /// <returns>List of response message to WeChat.</returns>
        private async Task<IList<IResponseMessageBase>> AttachmentToWeChatMessageAsync(IMessageActivity activity, Attachment attachment, SecretInfo secretInfo)
        {
            var responseList = new List<IResponseMessageBase>();
            attachment.Properties.TryGetValue("MediaId", StringComparison.InvariantCultureIgnoreCase, out var mediaId);
            if (!string.IsNullOrEmpty(mediaId?.ToString()))
            {
                var response = this.CreateMediaResponse(mediaId.ToString(), attachment.ContentType, activity);
                responseList.Add(response);
            }
            else if (attachment.ContentUrl != null)
            {
                // ContentUrl can contain a url or dataUrl of the form "data:image/jpeg;base64,XXXXXXXXX..."
                var attachmentData = new AttachmentData(name: attachment.Name ?? new Guid().ToString());
                if (AttachmentHelper.IsUrl(attachment.ContentUrl))
                {
                    var bytesData = await this.wechatClient.SendHttpRequestAsync(HttpMethod.Get, attachment.ContentUrl).ConfigureAwait(false);
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

                var response = await this.AttachmentDataToWeChatMessage(activity, attachmentData).ConfigureAwait(false);
                if (response != null)
                {
                    responseList.Add(response);
                }
            }

            return responseList;
        }

        private ResponseMessage CreateMediaResponse(string mediaId, string type, IActivity activity)
        {
            ResponseMessage response = null;
            if (type.Contains(MediaType.Image))
            {
                response = new ImageResponse(mediaId);
            }

            if (type.Contains(MediaType.Video))
            {
                response = new VideoResponse(mediaId);
            }

            if (type.Contains(MediaType.Audio))
            {
                response = new VoiceResponse(mediaId);
            }

            response.SetProperties(activity);
            return response;
        }

        /// <summary>
        /// Convert Text To WeChat Message.
        /// </summary>
        /// <param name="activity">message activity from bot.</param>
        /// <param name="text">text need to be sent.</param>
        /// <returns>Response message to WeChat.</returns>
        private IResponseMessageBase TextToWeChatMessage(IMessageActivity activity, string text)
        {
            var response = this.CreateTextResponseFromMessageActivity(activity);
            response.Content = text;
            return response;
        }

        private TextResponse CreateTextResponseFromMessageActivity(IMessageActivity activity)
        {
            var response = new TextResponse
            {
                CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                FromUserName = activity.From.Id,
                ToUserName = activity.Recipient.Id,
                Content = activity.Text,
            };
            return response;
        }

        /// <summary>
        /// Convert all suggestedActions to text string for channels that can't display them natively.
        /// </summary>
        /// <param name="actions">List of card action.</param>
        /// <returns>CardAction as string.</returns>
        private string SuggestedActionsToText(IList<CardAction> actions)
        {
            return this.ButtonsToText(actions, action =>
            {
                var buttonText = action.Text == ActionTypes.MessageBack ? action.Text : action.Value as string;
                return $"{buttonText ?? action.Title}";
            });
        }

        private IList<IResponseMessageBase> GetChunkedMessages(IMessageActivity activity, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<IResponseMessageBase>();
            }

            if (activity.TextFormat == TextFormatTypes.Markdown)
            {
                text = this.GetMarked().Parse(text).Trim();
            }

            // If message doesn't need to be chunked just return it
            if (text.Length <= Constants.MaxSingleMessageLength)
            {
                return new List<IResponseMessageBase>
                {
                    this.TextToWeChatMessage(activity, text),
                };
            }

            // Truncate to maximum total length as necessary
            if (text.Length > Constants.MaxTotalMessageLength)
            {
                text = text.Substring(0, Constants.MaxTotalMessageLength);
            }

            // Split text into chunks
            var messages = new List<IResponseMessageBase>();
            var chunkLength = Constants.MaxSingleMessageLength - 20;  // leave 20 chars for footer
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
                    chunk += $"\n({++chunkNum} of {chunkCount})";
                }

                // Create chunked message and add to list of messages
                messages.Add(this.TextToWeChatMessage(activity, chunk));
            }

            return messages;
        }

        /// <summary>
        /// render a adaptiveCard into text replies for low-fi channels.
        /// </summary>
        private async Task<IList<IResponseMessageBase>> ProcessAdaptiveCardAsync(IMessageActivity activity, AdaptiveCard adaptiveCard, AdaptiveHostConfig adaptiveCardOptions, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            try
            {
                // TODO: need to delete if we won't use convert to png solution.

                // var styleFilePath = AppDomain.CurrentDomain.BaseDirectory + "Content\\AdaptiveCardStyles.xaml";
                // var attachmentData = await MapperUtils.RenderAdaptiveCardToAttachmentDataAsync(
                //    adaptiveCard,
                //    _adaptiveCardOptions,
                //    styleFilePath).ConfigureAwait(false);

                // messages.AddRange(await CallAttachmentToChannelMessageAsync(activity, attachmentData, secretInfo).ConfigureAwait(false));

                // string text = null;
                // int i = 1;
                //// Convert any options to text
                // foreach (AdaptiveAction action in adaptiveCard.Actions)
                // {
                //    if (text == null)
                //        text = "Options:\n";

                // if (action is AdaptiveOpenUrlAction)
                //    {
                //        text += $"{i++}. {action.Title} {((AdaptiveOpenUrlAction)action).Url}\n";
                //    }
                //    else
                //    {
                //        text += $"{i++}. {action.Title}\n";
                //    }
                // }
                var news = await this.CreateNewsFromAdaptiveCard(activity, adaptiveCard, secretInfo);
                var uploadResult = await this.wechatClient.UploadTemporaryNewsAsync(10000, news);
                var mpnews = new MpNewsResponse(uploadResult.MediaId);
                messages.Add(mpnews);
            }
            catch
            {
                this.logger.TrackTrace("Convert adaptive card failed.");
                messages.AddRange(this.GetChunkedMessages(activity, adaptiveCard.FallbackText));
            }

            return messages;
        }

        private async Task<IList<IResponseMessageBase>> ProcessHeroCardAsync(IMessageActivity activity, HeroCard heroCard, Attachment attachment, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();
            string mediaId;
            var news = await this.CreateNewsFromHeroCard(activity, heroCard, secretInfo);
            var uploadResult = await this.wechatClient.UploadTemporaryNewsAsync(10000, news);

            mediaId = uploadResult.MediaId;
            var mpnews = new MpNewsResponse(mediaId);

            messages.Add(mpnews);

            // Add buttons
            if (heroCard.Buttons != null && heroCard.Buttons.Count > 0)
            {
                var buttonString = this.ButtonsToText(heroCard.Buttons.ToArray());
                messages.AddRange(this.GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        private IList<IResponseMessageBase> ProcessThumbnailCard(IMessageActivity activity, ThumbnailCard basicCard, Attachment attachment, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            // Add text
            var body = basicCard.Subtitle;
            body = body.AddLine(basicCard.Text);
            var article = new Article
            {
                Title = basicCard.Title,
                Description = body,
                Url = basicCard.Tap?.Value.ToString(),
                PicUrl = basicCard.Images.FirstOrDefault().Url,
            };
            var newsResponse = new NewsResponse()
            {
                Articles = new List<Article>() { article },
            };
            messages.Add(newsResponse);

            // Add buttons
            if (basicCard.Buttons != null && basicCard.Buttons.Count > 0)
            {
                var buttonString = this.ButtonsToText(basicCard.Buttons.ToArray());
                messages.AddRange(this.GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        private async Task<IList<IResponseMessageBase>> ProcessVideoCardAsync(IMessageActivity activity, VideoCard videoCard, Attachment attachment, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            var body = videoCard.Subtitle;
            body = body.AddLine(videoCard.Text);
            Video video = null;

            // upload thumbnail image.
            if (!string.IsNullOrEmpty(videoCard.Image?.Url))
            {
                // TODO: wechat doc have thumb_media_id for video mesasge, but not implemented in current package.

                // Attachment surrogate = new Attachment()
                // {
                //    ContentUrl = videoCard.Image.Url,
                //    ContentType = MediaType.Image,
                //    Name = videoCard.Image.Alt,
                // };
                // var reponseList = await CallMediaToChannelMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false);
                // var imageResponse = reponseList.FirstOrDefault() as ImageResponse;
                // if (imageResponse != null)
                // {
                //    video.MediaId = imageResponse.Image.MediaId;
                // }
                var surrogate = new Attachment()
                {
                    ContentType = MediaType.Video,
                    Name = videoCard.Title,
                    ContentUrl = videoCard.Media[0].Url,
                };
                var reponseList = await this.AttachmentToWeChatMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false);
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
                buttonString = this.ButtonsToText(videoCard.Buttons.ToArray());
                messages.AddRange(this.GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        /// <summary>
        /// Convert audio card as music resposne.
        /// Thumbnail image size limitation is not clear.
        /// </summary>
        /// <returns>List of response message to WeChat.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessAudioCardAsync(IMessageActivity activity, AudioCard audioCard, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            var body = audioCard.Subtitle;
            body = body.AddLine(audioCard.Text);
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
                    ContentType = MediaType.Image,
                    Name = audioCard.Image.Alt,
                };
                var reponseList = await this.AttachmentToWeChatMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false);
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
                buttonString = this.ButtonsToText(audioCard.Buttons.ToArray());
                messages.AddRange(this.GetChunkedMessages(activity, buttonString));
            }

            return messages;
        }

        private async Task<IList<IResponseMessageBase>> ProcessAnimationCardAsync(IMessageActivity activity, AnimationCard mediacard, Attachment attachment, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            // Generate body
            var body = mediacard.Title;
            body = body.AddLine(mediacard.Subtitle);
            body = body.AddLine(mediacard.Text);

            // Add buttons
            if (mediacard.Buttons != null && mediacard.Buttons.Count > 0)
            {
                body = body.AddLine(this.ButtonsToText(mediacard.Buttons.ToArray()));
            }

            // Add image
            if (!string.IsNullOrEmpty(mediacard.Image?.Url))
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = mediacard.Image.Url,
                    ContentType = MediaType.Image,
                    Name = mediacard.Image.Alt,
                };

                messages.AddRange(await this.AttachmentToWeChatMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false));
            }

            // Add mediaUrls
            foreach (var mediaUrl in mediacard.Media ?? new MediaUrl[] { })
            {
                var surrogate = new Attachment()
                {
                    ContentUrl = mediaUrl.Url,
                    Name = mediaUrl.Profile,
                    ContentType = MediaType.Gif,
                };

                messages.AddRange(await this.AttachmentToWeChatMessageAsync(activity, surrogate, secretInfo).ConfigureAwait(false));
            }

            messages.AddRange(this.GetChunkedMessages(activity, body));

            return messages;
        }

        /// <summary>
        /// Downgrade ReceiptCard into text replies for low-fi channels.
        /// </summary>
        /// <returns>List of response message to WeChat.</returns>
        private async Task<IList<IResponseMessageBase>> ProcessReceiptCardAsync(
            IMessageActivity activity,
            ReceiptCard receiptCard,
            Attachment attachment,
            SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            // Build text portion of receipt
            var body = receiptCard.Title;
            foreach (var fact in receiptCard.Facts ?? new Fact[] { })
            {
                body = body.AddLine($"{fact.Key}:  {fact.Value}");
            }

            messages.AddRange(this.GetChunkedMessages(activity, body));

            // Add items, grouping text only ones into a single post
            string textbody = null;
            foreach (var item in receiptCard.Items ?? new ReceiptItem[] { })
            {
                if (item.Image != null)
                {
                    body = item.Title.AddText(item.Price).AddLine(item.Subtitle).AddLine(item.Text);
                    messages.AddRange(await this.AttachmentToWeChatMessageAsync(activity, attachment, secretInfo).ConfigureAwait(false));
                    messages.AddRange(this.GetChunkedMessages(activity, body));
                }
                else
                {
                    textbody = textbody.AddLine(item.Title).AddText(item.Price).AddLine(item.Subtitle).AddLine(item.Text);
                }
            }

            // Add textonly items
            messages.AddRange(this.GetChunkedMessages(activity, textbody));

            // Add totals
            body = $"Tax:  {receiptCard.Tax}";
            body = body.AddLine($"Total:  {receiptCard.Total}");

            // Add buttons
            if (receiptCard.Buttons != null && receiptCard.Buttons.Count > 0)
            {
                body = body.AddLine(this.ButtonsToText(receiptCard.Buttons));
            }

            messages.AddRange(this.GetChunkedMessages(activity, body));

            return messages;
        }

        /// <summary>
        /// Downgrade SigninCard into text replies for low-fi channels.
        /// </summary>
        private List<IResponseMessageBase> ProcessSigninCard(SigninCard signinCard, IMessageActivity activity)
        {
            var messages = new List<IResponseMessageBase>();

            // Add text
            messages.AddRange(this.GetChunkedMessages(activity, signinCard.Text));

            // Add button
            if (signinCard.Buttons != null)
            {
                messages.AddRange(this.GetChunkedMessages(activity, this.ButtonToText(signinCard.Buttons.First())));
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
            messages.AddRange(this.GetChunkedMessages(activity, oauthCard.Text));

            // Add button
            if (oauthCard.Buttons != null)
            {
                messages.AddRange(this.GetChunkedMessages(activity, this.ButtonToText(oauthCard.Buttons.FirstOrDefault())));
            }

            return messages;
        }

        /// <summary>
        /// Convert attachments to WeChat response message.
        /// </summary>
        private async Task<List<IResponseMessageBase>> ProcessAttachmentAsync(IMessageActivity activity, Attachment attachment, SecretInfo secretInfo)
        {
            var messages = new List<IResponseMessageBase>();

            messages.AddRange(await this.AttachmentToWeChatMessageAsync(activity, attachment, secretInfo).ConfigureAwait(false));

            if (messages.Any())
            {
                return messages;
            }

            return messages;
        }

        private Marked GetMarked()
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
            return marked;
        }
    }
}
