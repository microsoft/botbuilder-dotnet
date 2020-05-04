// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Send an activity back to the user.
    /// </summary>
    public class SendActivity : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.SendActivity";

        public SendActivity(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        [JsonConstructor]
        public SendActivity(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
            var properties = new Dictionary<string, string>()
            {
                { "template", JsonConvert.SerializeObject(Activity) },
                { "result", activity == null ? string.Empty : JsonConvert.SerializeObject(activity, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) },
            };
            TelemetryClient.TrackEvent("GeneratorResult", properties);

            ResourceResponse response = null;
            if (activity.Type != "message" 
                || !string.IsNullOrEmpty(activity.Text)
                || activity.Attachments?.Any() == true
                || !string.IsNullOrEmpty(activity.Speak)
                || activity.SuggestedActions != null)
            {
                response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

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
