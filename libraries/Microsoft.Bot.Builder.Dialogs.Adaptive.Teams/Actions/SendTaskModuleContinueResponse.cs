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
        /// <param name="activity"><see cref="Activity"/> to send.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SendTaskModuleContinueResponse(Activity activity = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendTaskModuleContinueResponse"/> class.
        /// </summary>
        /// <param name="text">Optional, template to evaluate to create the activity.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendTaskModuleContinueResponse(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
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
        /// Gets or sets template for the activity.
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

            var activity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);

            //TODO: Add support for parameterized task module info settings
            /*
                    Title
                    Height
                    Width 
                    Url 
                    Card 
                    FallbackUrl 
                    CompletionBotId 
            */

            if (activity.Attachments == null || !activity.Attachments.Any())
            {
                throw new ArgumentException($"A valid attachment is required for Task Module Continue Response.");
            }

            var attachment = activity.Attachments[0];

            // TODO: LG?
            //if (attachment != null)
            //{
            //    var languageGenerator = dc.Services.Get<LanguageGenerator>();
            //    if (languageGenerator != null)
            //    {
            //        var lgStringResult = await languageGenerator.GenerateAsync(dc, attachment.Content.ToString(), dc.State, cancellationToken).ConfigureAwait(false);
            //        attachment.Content = lgStringResult.ToString();
            //    }
            //}

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
                                Card = attachment
                            }
                        }
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
