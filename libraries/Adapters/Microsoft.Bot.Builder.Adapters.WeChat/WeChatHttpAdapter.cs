using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Request;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Utilities.XmlUtility;
using Microsoft.Extensions.Configuration;
using Microsoft.Microsoft.Bot.Builder.Adapters.WeChat.TaskExtensions;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class WeChatHttpAdapter : WeChatAdapter, IWeChatHttpAdapter
    {
        private readonly string appId;
        private readonly string encodingAESKey;
        private readonly string token;
        private readonly WeChatLogger logger;
        private readonly IBackgroundTaskQueue taskQueue;

        public WeChatHttpAdapter(
            IConfiguration configuration,
            Func<ITurnContext, Exception, Task> onTurnError = null,
            BotStateSet botStateSet = null,
            WeChatLogger logger = null,
            IBackgroundTaskQueue backgroundTaskQueue = null)
            : base(configuration, logger)
        {
            this.appId = configuration.GetSection("WeChatSetting").GetSection("AppId").Value;
            this.encodingAESKey = configuration.GetSection("WeChatSetting").GetSection("EncodingAESKey").Value;
            this.token = configuration.GetSection("WeChatSetting").GetSection("Token").Value;
            this.logger = logger ?? WeChatLogger.Instance;
            this.taskQueue = backgroundTaskQueue;

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

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, SecretInfo secretInfo, bool replyAsync, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.logger.TrackTrace("Receive a new Request");
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
                var ex = new UnauthorizedAccessException("Message check failed");
                this.logger.TrackException("Message check failed", exception: ex);
                throw ex;
            }

            secretInfo.Token = this.token;
            secretInfo.EncodingAESKey = this.encodingAESKey;
            secretInfo.AppId = this.appId;
            var postDataDocument = XmlUtility.Convert(httpRequest.Body);
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
    }
}
