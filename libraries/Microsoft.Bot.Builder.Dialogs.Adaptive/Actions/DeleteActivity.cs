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
    /// Send an activity back to the user.
    /// </summary>
    public class DeleteActivity : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.DeleteActivity";

        private Expression activityId;
        private Expression disabled;

        [JsonConstructor]
        public DeleteActivity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
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

            var (result, error) = new ExpressionEngine().Parse(this.ActivityId).TryEvaluate(dc.GetState());
            if (error != null)
            {
                throw new ArgumentException(error);
            }

            var id = (string)result;
            await dc.Context.DeleteActivityAsync(id, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}('{ActivityId?.ToString()}')";
        }
    }
}
