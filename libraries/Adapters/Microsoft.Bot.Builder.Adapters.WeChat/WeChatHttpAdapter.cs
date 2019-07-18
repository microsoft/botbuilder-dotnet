using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        private readonly IHostedService backgroundService;

        public WeChatHttpAdapter(
                    IConfiguration configuration,
                    IWeChatMessageMapper wechatMessageMapper = null,
                    WeChatClient wechatClient = null,
                    BotStateSet botStateSet = null,
                    WeChatLogger logger = null,
                    IBackgroundTaskQueue backgroundTaskQueue = null,
                    IHostedService backgroundService = null,
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
            this.backgroundService = backgroundService;

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

        public async Task<object> ProcessWeChatRequest(IRequestMessageBase wechatRequest, BotCallbackHandler callback, SecretInfo secretInfo, bool passiveResponse, CancellationToken cancellationToken)
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
                        var activities = responses.ContainsKey(key) ? responses[key] : new List<Activity>();
                        var response = await this.ProcessBotResponse(activities, secretInfo, wechatRequest.FromUserName, passiveResponse);
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

        /// <summary>
        /// Process the request from WeChat.
        /// </summary>
        /// <param name="httpRequest">The request sent from WeChat.</param>
        /// <param name="httpResponse">Http response object of current request.</param>
        /// <param name="bot">The bot instance.</param>
        /// <param name="secretInfo">Secret info for verify the request.</param>
        /// <param name="passiveResponse">If using passvice response mode, if set to true, user can only get one reply.</param>
        /// <param name="cancellationToken">Cancellation Token of this Task.</param>
        /// <returns>Task running result.</returns>
        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, SecretInfo secretInfo, bool passiveResponse, CancellationToken cancellationToken = default(CancellationToken))
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
                // Reply WeChat(User) request have two ways, set response in http response or use background task to process the request async.
                if (!passiveResponse)
                {
                    // Running a background task, Get bot response and parse it from activity to wechat response message
                    if (this.backgroundService == null)
                    {
                        throw new ArgumentNullException("AdapterBackgroundService can not be null.");
                    }

                    this.taskQueue.QueueBackgroundWorkItem(async (ct) =>
                    {
                        await this.ProcessWeChatRequest(
                                        wechatRequest,
                                        bot.OnTurnAsync,
                                        secretInfo,
                                        passiveResponse,
                                        ct).ConfigureAwait(false);
                    });
                }
                else
                {
                    var wechatResponse = await this.ProcessWeChatRequest(
                                wechatRequest,
                                bot.OnTurnAsync,
                                secretInfo,
                                passiveResponse,
                                cancellationToken).ConfigureAwait(false);
                    httpResponse.StatusCode = (int)HttpStatusCode.OK;
                    httpResponse.ContentType = "text/xml";

                    var xmlString = Schema.MessageFactory.ConvertResponseToXml(wechatResponse);
                    var requestBytes = Encoding.UTF8.GetBytes(xmlString);
                    httpResponse.Body.Write(requestBytes, 0, requestBytes.Length);
                }
            }
            catch (Exception ex)
            {
                this.logger.TrackException("Process wechat request failed.", ex);
            }
        }

        /// <summary>
        /// Parse the XDocument to RequestMessage, decrypt it if needed.
        /// </summary>
        /// <param name="postDataDocument">XDocument from WeChat Request.</param>
        /// <param name="secretInfo">The secretInfo used to decrypt the message.</param>
        /// <returns>Decrypted WeChat RequestMessage instance.</returns>
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

            var requestMessage = Schema.MessageFactory.GetRequestEntity(decryptDoc);

            return requestMessage;
        }

        private async Task<object> ProcessBotResponse(List<Activity> activities, SecretInfo secretInfo, string openId, bool passiveResponse = false)
        {
            object response = null;
            foreach (var activity in activities)
            {
                if (activity != null && activity.Type == ActivityTypes.Message)
                {
                    if (activity.ChannelData != null)
                    {
                        if (passiveResponse)
                        {
                            response = activity.ChannelData;
                        }
                        else
                        {
                            await this.SendMessageToWeChat(activity.ChannelData).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var resposneList = await this.wechatMessageMapper.ToWeChatMessages(activity, secretInfo).ConfigureAwait(false);

                        // Passive Response can only response one message per turn, retrun the last acitvity as the response.
                        if (passiveResponse)
                        {
                            response = resposneList.LastOrDefault();
                        }
                        else
                        {
                            await this.SendMessageToWechat(resposneList, openId).ConfigureAwait(false);
                        }
                    }
                }
            }

            return response;
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

        private async Task SendMessageToWechat(IList<IResponseMessageBase> responseList, string openId)
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
                        case ResponseMessageType.SuccessResponse:
                        case ResponseMessageType.Unknown:
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
    }
}
