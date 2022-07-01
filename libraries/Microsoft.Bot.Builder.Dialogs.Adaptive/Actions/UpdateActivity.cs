﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
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
    /// Update an activity with replacement.
    /// </summary>
    public class UpdateActivity : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.UpdateActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivity"/> class.
        /// </summary>
        /// <param name="activity">Replacement <see cref="Activity"/>.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public UpdateActivity(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivity"/> class.
        /// </summary>
        /// <param name="text">Optional, the template to evaluate to create the replacement activity.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public UpdateActivity(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets the expression which resolves to the activityId to update.
        /// </summary>
        /// <value>Expression to activityId.</value>
        [JsonProperty("activityId")]
        public StringExpression ActivityId { get; set; }

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

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var activity = await Activity.BindAsync(dc, dc.State, cancellationToken: cancellationToken).ConfigureAwait(false);

            var properties = new Dictionary<string, string>()
            {
                { "template", JsonConvert.SerializeObject(Activity, new JsonSerializerSettings { MaxDepth = null }) },
                { "result", activity == null ? string.Empty : JsonConvert.SerializeObject(activity, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, MaxDepth = null }) },
                { "context", TelemetryLoggerConstants.UpdateActivityResultEvent }
            };
            TelemetryClient.TrackEvent(TelemetryLoggerConstants.GeneratorResultEvent, properties);

            var (result, error) = this.ActivityId.TryGetValue(dc.State);
            if (error != null)
            {
                throw new ArgumentException(error);
            }

            activity.Id = (string)result;

            var response = await dc.Context.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            if (Activity is ActivityTemplate at)
            {
                return $"{GetType().Name}({StringUtils.Ellipsis(at.Template.Trim(), 30)})";
            }

            return $"{GetType().Name}('{StringUtils.Ellipsis(Activity?.ToString().Trim(), 30)}')";
        }
    }
}
