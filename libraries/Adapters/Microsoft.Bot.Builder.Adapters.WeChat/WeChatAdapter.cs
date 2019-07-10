using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatAdapter : BotAdapter
    {
        private readonly IWeChatMessageMapper wechatMessageMapper;
        private readonly WeChatClient wechatClient;
        private readonly WeChatLogger logger;

        public WeChatAdapter(IConfiguration configuration, WeChatLogger logger = null)
        {
            this.wechatClient = new WeChatClient(configuration, logger);
            this.wechatMessageMapper = new WeChatMessageMapper(configuration, this.wechatClient, logger);
            this.logger = logger ?? WeChatLogger.Instance;
        }

        public WeChatAdapter(IWeChatMessageMapper wechatMessageMapper, WeChatClient wechatClient, WeChatLogger logger = null)
        {
            this.wechatMessageMapper = wechatMessageMapper;
            this.wechatClient = wechatClient;
            this.logger = logger ?? WeChatLogger.Instance;
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

            return Task.FromResult<ResourceResponse[]>(resourceResponses.ToArray());
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
