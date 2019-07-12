using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatHttpAdapter : BotAdapter, IWeChatHttpAdapter
    {
        private readonly IWeChatMessageMapper wechatMessageMapper;
        private readonly WeChatClient wechatClient;
        private readonly WeChatLogger logger;
        private readonly string appId;
        private readonly string encodingAESKey;
        private readonly string token;
        private readonly IBackgroundTaskQueue taskQueue;

        public WeChatHttpAdapter(
                    IConfiguration configuration,
                    IWeChatMessageMapper wechatMessageMapper = null,
                    WeChatClient wechatClient = null,
                    BotStateSet botStateSet = null,
                    WeChatLogger logger = null,
                    IBackgroundTaskQueue backgroundTaskQueue = null,
                    Func<ITurnContext, Exception, Task> onTurnError = null)
        {
            this.appId = configuration.GetSection("WeChatSetting").GetSection("AppId")?.Value;
            this.encodingAESKey = configuration.GetSection("WeChatSetting").GetSection("EncodingAESKey")?.Value;
            this.token = configuration.GetSection("WeChatSetting").GetSection("Token")?.Value;
            this.logger = logger ?? WeChatLogger.Instance;
            this.wechatClient = wechatClient ?? new WeChatClient(configuration, logger);
            this.wechatMessageMapper = wechatMessageMapper ?? new WeChatMessageMapper(configuration, this.wechatClient, logger);
            this.logger = logger ?? WeChatLogger.Instance;
            this.taskQueue = backgroundTaskQueue ?? BackgroundTaskQueue.Instance;

            if (onTurnError == null)
            {
                this.OnTurnError = async (context, exception) =>
                {
                    await context.SendActivityAsync(Constants.DefaultErrorMessage);
                };
            }

            if (botStateSet != null)
            {
                this.Use(new AutoSaveStateMiddleware(botStateSet));
            }
        }

        public async Task<IResponseMessageBase> ProcessWeChatRequest(IRequestMessageBase wechatRequest, BotCallbackHandler callback, SecretInfo secretInfo, CancellationToken cancellationToken)
        {
            var activity = await this.wechatMessageMapper.ToConnectorMessage(wechatRequest);
            BotAssert.ActivityNotNull(activity);
            using (var context = new TurnContext(this, activity as Activity))
            {
                try
                {
                    var responses = new Dictionary<string, List<Activity>>();
                    context.TurnState.Add(Constants.TurnResponseKey, responses);
                    await this.RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);
                    var key = $"{activity.Conversation.Id}:{activity.Id}";
                    try
                    {
                        IResponseMessageBase response = null;
                        var activities = responses.ContainsKey(key) ? responses[key] : new List<Activity>();
                        await this.ProcessResponse(activities, secretInfo, wechatRequest.FromUserName, context);
                        return response;
                    }
                    catch (Exception e)
                    {
                        // TODO: exception handling when send message to wechat api failed.
                        this.logger.TrackException("Failed to process bot response", e);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // exception handing when bot throw an exception.
                    await this.OnTurnError(context, ex);
                    return null;
                }
            }
        }

        // Does not support by wechat.
        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Does not support by wechat.
        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.EndOfConversation:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";
                        var responses = turnContext.TurnState.Get<Dictionary<string, List<Activity>>>(Constants.TurnResponseKey);
                        if (responses.ContainsKey(key))
                        {
                            responses[key].Add(activity);
                        }
                        else
                        {
                            responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        this.logger.TrackTrace(
                            $"WeChatAdapter.SendActivities(): Activities of type '{activity.Type}' aren't supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, SecretInfo secretInfo, bool replyAsync, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.logger.TrackTrace("Receive a new request from WeChat.");
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (secretInfo == null)
            {
                throw new ArgumentNullException(nameof(secretInfo));
            }

            if (!VerificationHelper.Check(secretInfo.Signature, secretInfo.Timestamp, secretInfo.Nonce, this.token))
            {
                var ex = new UnauthorizedAccessException("Message check failed.");
                this.logger.TrackException("Message check failed.", exception: ex);
                throw ex;
            }

            secretInfo.Token = this.token;
            secretInfo.EncodingAESKey = this.encodingAESKey;
            secretInfo.AppId = this.appId;
            var postDataDocument = XmlHelper.Convert(httpRequest.Body);
            var wechatRequest = this.GetRequestMessage(postDataDocument, secretInfo);

            try
            {
                if (replyAsync)
                {
                    // Running a background task, Get bot response and parse it from activity to wechat response message
                    if (this.taskQueue != null)
                    {
                        this.taskQueue.QueueBackgroundWorkItem(async (ct) =>
                        {
                            await this.ProcessWeChatRequest(
                                            wechatRequest,
                                            bot.OnTurnAsync,
                                            secretInfo,
                                            ct).ConfigureAwait(false);
                        });
                    }
                    else
                    {
                        // Need a message queue here, directly use task run may not reliable enough.
                        // No backgroud queue here, Downgrade to thread pool
                        await Task.Run(() => this.ProcessWeChatRequest(
                                                wechatRequest,
                                                bot.OnTurnAsync,
                                                secretInfo,
                                                cancellationToken).ConfigureAwait(false));
                    }
                }
                else
                {
                    var wechatResponse = await this.ProcessWeChatRequest(
                                wechatRequest,
                                bot.OnTurnAsync,
                                secretInfo,
                                cancellationToken).ConfigureAwait(false);
                    httpResponse.ContentType = "application/json";
                    httpResponse.StatusCode = (int)HttpStatusCode.OK;

                    // Write the response message to response body
                    using (var writer = new StreamWriter(httpResponse.Body))
                    {
                        using (var xmlWriter = new XmlTextWriter(writer))
                        {
                            // TODO: this is not working right now. need a xml serializer
                            // WeChatBotMessageSerializer.Serialize(xmlWriter, weChatResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.TrackException("Process wechat request failed.", exception: ex);
            }
        }

        public IRequestMessageBase GetRequestMessage(XDocument postDataDocument, SecretInfo secretInfo)
        {
            // decrypt xml document message and parse to message
            var postDataStr = postDataDocument.ToString();
            var decryptDoc = postDataDocument;

            if (secretInfo != null
                && !string.IsNullOrWhiteSpace(secretInfo.Token)
                && postDataDocument.Root.Element("Encrypt") != null
                && !string.IsNullOrEmpty(postDataDocument.Root.Element("Encrypt").Value))
            {
                var msgCrype = new MessageCryptography(secretInfo);
                var msgXml = msgCrype.DecryptMessage(postDataStr);

                decryptDoc = XDocument.Parse(msgXml);
            }

            var requestMessage = RequestMessageFactory.GetRequestEntity(decryptDoc);

            return requestMessage;
        }

        private async Task ProcessResponse(List<Activity> activities, SecretInfo secretInfo, string openId, TurnContext context)
        {
            foreach (var activity in activities)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    if (activity.ChannelData != null)
                    {
                        await this.SendMessageToWeChat(activity.ChannelData).ConfigureAwait(false);
                    }
                    else
                    {
                        var resposneList = await this.CreateResponseFromActivity(activity, context, secretInfo);
                        await this.SendMessageToWechat(resposneList, secretInfo, openId).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task SendMessageToWeChat(object channelData)
        {
            try
            {
                await this.wechatClient.SendMessageToUser(channelData).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.logger.TrackException("Send channelData to wechat failed", e);
            }
        }

        private async Task SendMessageToWechat(IList<IResponseMessageBase> responseList, SecretInfo secretInfo, string openId)
        {
            foreach (var response in responseList)
            {
                try
                {
                    switch (response.MsgType)
                    {
                        case ResponseMessageType.Text:
                            var textResponse = response as TextResponse;
                            await this.wechatClient.SendTextAsync(openId, textResponse.Content);
                            break;

                        case ResponseMessageType.Image:
                            var imageResposne = response as ImageResponse;
                            await this.wechatClient.SendImageAsync(openId, imageResposne.Image.MediaId);
                            break;

                        case ResponseMessageType.News:
                            var newsResponse = response as NewsResponse;
                            await this.wechatClient.SendNewsAsync(openId, newsResponse.Articles);
                            break;

                        case ResponseMessageType.Music:
                            var musicResponse = response as MusicResponse;
                            var music = musicResponse.Music;
                            await this.wechatClient.SendMusicAsync(openId, music.Title, music.Description, music.MusicUrl, music.HQMusicUrl, music.ThumbMediaId);
                            break;

                        case ResponseMessageType.MpNews:
                            var mpnewsResponse = response as MpNewsResponse;
                            await this.wechatClient.SendMpNewsAsync(openId, mpnewsResponse.MediaId);
                            break;

                        case ResponseMessageType.Video:
                            var videoResposne = response as VideoResponse;
                            var video = videoResposne.Video;
                            await this.wechatClient.SendVideoAsync(openId, video.MediaId, video.Title, video.Description);
                            break;

                        case ResponseMessageType.Voice:
                            var voiceResponse = response as VoiceResponse;
                            var voice = voiceResponse.Voice;
                            await this.wechatClient.SendVoiceAsync(openId, voice.MediaId);
                            break;
                        case ResponseMessageType.LocationMessage:
                            var locationResponse = response as ResponseMessage;

                            // Currently not supported by wechat api.
                            // TODO: find another way to send location message. perhaps using map service.
                            break;
                        case ResponseMessageType.Other:
                        case ResponseMessageType.SuccessResponse:
                        case ResponseMessageType.Unknown:
                        case ResponseMessageType.UseApi:
                        case ResponseMessageType.Transfer_Customer_Service:
                        case ResponseMessageType.NoResponse:
                        default:
                            this.logger.TrackTrace("Get an unsupported messaged.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    this.logger.TrackException($"Send response to wechat failed", e);
                }
            }
        }

        private async Task<IList<IResponseMessageBase>> CreateResponseFromActivity(Activity activity, TurnContext context, SecretInfo secretInfo)
        {
            var response = await this.wechatMessageMapper.ToWeChatMessages(activity, secretInfo);
            return response;
        }
    }
}
