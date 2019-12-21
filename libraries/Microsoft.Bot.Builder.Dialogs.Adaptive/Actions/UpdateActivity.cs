// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Update an activity with replacement.
    /// </summary>
    public class UpdateActivity : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.UpdateActivity";

        private Expression activityId;
        private Expression disabled;

        public UpdateActivity(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

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
        public string Disabled
        {
            get { return disabled?.ToString(); }
            set { disabled = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

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
        public string ActivityId
        {
            get { return activityId.ToString(); }
            set { activityId = new ExpressionEngine().Parse(value); }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.disabled != null && (bool?)this.disabled.TryEvaluate(dc.GetState()).value == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var activity = await Activity.BindToData(dc.Context, dc.GetState()).ConfigureAwait(false);
            var (result, error) = activityId.TryEvaluate(dc.GetState());
            if (error != null)
            {
                throw new ArgumentException(error);
            }

            activity.Id = (string)result;

            var response = await dc.Context.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            if (Activity is ActivityTemplate at)
            {
                return $"{this.GetType().Name}({Ellipsis(at.Template.Trim(), 30)})";
            }

            return $"{this.GetType().Name}('{Ellipsis(Activity?.ToString().Trim(), 30)}')";
        }

        private static string Ellipsis(string text, int length)
        {
            if (text.Length <= length)
            {
                return text;
            }

            int pos = text.IndexOf(" ", length);

            if (pos >= 0)
            {
                return text.Substring(0, pos) + "...";
            }

            return text;
        }
    }
}
