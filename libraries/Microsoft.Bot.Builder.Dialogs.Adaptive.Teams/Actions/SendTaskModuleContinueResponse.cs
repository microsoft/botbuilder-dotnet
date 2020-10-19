// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send an Task Module Continue response to the user.
    /// </summary>
    public class SendTaskModuleContinueResponse : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendTaskModuleContinueResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendTaskModuleContinueResponse"/> class.
        /// </summary>
        /// <param name="title">Optional, Title for the Task Module Response.</param>
        /// <param name="activity">Optional, <see cref="Activity"/>Activity containing an Hero Card, or Adaptive Card Attachment to send.</param>
        /// <param name="height">Optional, Height for the Task Module Response.</param>
        /// <param name="width">Optional, Width for the Task Module Response.</param>
        /// <param name="url">Optional, url to load within the Task Module Response.</param>
        /// <param name="fallbackUrl">Optional, fallback url to load within the Task Module Response.</param>
        /// <param name="completionBotId">Optionally, specifies a bot App ID to send the result of the 
        /// user's interaction with the task module to. If specified, the bot will receive 
        /// a task/submit invoke event with a JSON object in the event payload.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SendTaskModuleContinueResponse(string title = null, Activity activity = null, int? height = null, int? width = null, string url = null, string fallbackUrl = null, string completionBotId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            if (!string.IsNullOrEmpty(title))
            {
                this.Title = new TextTemplate(title);
            }

            this.Height = height;
            this.Width = width;
            this.Url = url;
            this.FallbackUrl = fallbackUrl;
            this.CompletionBotId = completionBotId;
            this.Activity = new StaticActivityTemplate(activity);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the optional template or text to use to generate the title message to send.
        /// </summary>
        /// <value>
        /// Message to send.
        /// </value>
        [JsonProperty("title")]
        public ITemplate<string> Title { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the height of the Task Module response.
        /// </summary>
        /// <value>
        /// An integer expression. 
        /// </value>
        [JsonProperty("height")]
        public IntExpression Height { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the width of the Task Module response.
        /// </summary>
        /// <value>
        /// An integer expression. 
        /// </value>
        [JsonProperty("width")]
        public IntExpression Width { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the Url of the Task Module response.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("url")]
        public StringExpression Url { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the Fallback Url the Task Module Task Info response.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("url")]
        public StringExpression FallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the Completion Bot Id of the Task Module Task Info response.
        /// This is a bot App ID to send the result of the user's interaction with the task module to. If
        /// specified, the bot will receive a task/submit invoke event with a JSON object in the event payload.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("url")]
        public StringExpression CompletionBotId { get; set; }

        /// <summary>
        /// Gets or sets template for the activity expression containing a Hero Card or Adaptive Card with an Attachment to send.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            Attachment attachment = null;
            if (Activity != null)
            {
                var boundActivity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);

                if (boundActivity.Attachments == null || !boundActivity.Attachments.Any())
                {
                    throw new ArgumentException($"Invalid Activity. A valid url, or valid attachment is required for Task Module Continue Response.");
                }

                attachment = boundActivity.Attachments[0];
            }

            var title = Title == null ? string.Empty : await Title.BindAsync(dc, dc.State).ConfigureAwait(false);
            var height = Height.GetValue(dc.State);
            var width = Width.GetValue(dc.State);
            var url = Url.GetValue(dc.State);
            var fallbackUrl = FallbackUrl.GetValue(dc.State);
            var completionBotId = CompletionBotId.GetValue(dc.State);

            var responseActivity = new Activity
            {
                Value = new InvokeResponse
                {
                    Status = (int)HttpStatusCode.OK,
                    Body = new Schema.Teams.TaskModuleResponse
                    {
                        Task = new Schema.Teams.TaskModuleContinueResponse
                        {
                            Value = new Schema.Teams.TaskModuleTaskInfo
                            {
                                Title = title,
                                Card = attachment,
                                Url = url,
                                FallbackUrl = fallbackUrl,
                                Height = height,
                                Width = width,
                                CompletionBotId = completionBotId,
                            },
                        },
                    }
                },
                Type = ActivityTypesEx.InvokeResponse
            };

            var properties = new Dictionary<string, string>()
            {
                { "SendTaskModuleContinueResponse", responseActivity.ToString() },
            };
            TelemetryClient.TrackEvent("GeneratorResult", properties);

            ResourceResponse sendResponse = await dc.Context.SendActivityAsync(responseActivity, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(sendResponse, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            if (Activity is ActivityTemplate at)
            {
                return $"{this.GetType().Name}({StringUtils.Ellipsis(at.Template.Trim(), 30)})";
            }

            return $"{this.GetType().Name}('{StringUtils.Ellipsis(Activity?.ToString().Trim(), 30)}')";
        }
    }
}
