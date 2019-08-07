// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// Middleware for registering ITemplateRender
    /// </summary>
    public class TemplateManagerMiddleware : IMiddleware
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

        private TemplateManager TemplateManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="transcriptLogger">The conversation store to use.</param>
        public TemplateManagerMiddleware(TemplateManager templateManager = null)
        {
            this.TemplateManager = templateManager ?? new TemplateManager();
        }

        /// <summary>
        /// Template Renderers
        /// </summary>
        public List<ITemplateRenderer> Renderers { get { return this.TemplateManager.Renderers; } set { this.TemplateManager.Renderers = value; } }

        /// <summary>
        /// Language fallback policy
        /// </summary>
        public List<string> LanguageFallback { get { return this.TemplateManager.LanguageFallback; } set { this.TemplateManager.LanguageFallback = value; } }

        /// <summary>
        /// Records incoming and outgoing activities to the conversation store.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="nextTurn">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            // hook up onSend pipeline
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                foreach (var activity in activities)
                {
                    await TransformTemplateActivity(turnContext, activity);
                }

                // run pipeline
                return await nextSend().ConfigureAwait(false);
            });

            // hook up update activity pipeline
            turnContext.OnUpdateActivity(async (ctx, activity, nextUpdate) =>
            {
                await TransformTemplateActivity(turnContext, activity);

                // run full pipeline
                return await nextUpdate().ConfigureAwait(false);
            });

            // process bot logic
            await nextTurn(cancellationToken).ConfigureAwait(false);
        }

        private async Task TransformTemplateActivity(ITurnContext turnContext, Activity activity)
        {
            if (activity.Type == "Template")
            {
                var templateOptions = JToken.FromObject(activity.Value)?.ToObject<TemplateOptions>();

                var newActivity = await this.TemplateManager.RenderTemplate(turnContext,
                    turnContext.Activity.Locale,
                    templateOptions.TemplateId,
                    templateOptions.Data).ConfigureAwait(false);

                foreach(var property in typeof(Activity).GetProperties())
                {
                    switch(property.Name)
                    {
                        // keep envelope information
                        case nameof(IActivity.ChannelId):
                        case nameof(IActivity.From):
                        case nameof(IActivity.Recipient):
                        case nameof(IActivity.Id):
                        case nameof(IActivity.LocalTimestamp):
                        case nameof(IActivity.Timestamp):
                        case nameof(IActivity.ReplyToId):
                        case nameof(IActivity.ServiceUrl):
                        case nameof(IActivity.Conversation):
                            break;
                        default:
                            // shallow copy all other values
                            property.SetValue(activity, property.GetValue(newActivity));
                            break;
                    }
                }
            }
        }
    }
}
