// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Calls BotFrameworkAdapter.GetActivityMembers() and sets the result to a memory property.
    /// </summary>
    public class GetActivityMembers : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.GetActivityMembers";

        private Expression activityId;
        private Expression disabled;

        [JsonConstructor]
        public GetActivityMembers([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
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
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to put into property path.
        /// </summary>
        /// <value>
        /// The expression to get the value to put into property path. If this is missing, then the current turn Activity.id will be used.
        /// </value>
        [JsonProperty("activityId")]
        public string ActivityId
        {
            get { return activityId?.ToString(); }
            set { this.activityId = (value != null) ? new ExpressionEngine().Parse(value) : null; }
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

            var bfAdapter = dc.Context.Adapter as BotFrameworkAdapter;
            if (bfAdapter == null)
            {
                throw new Exception("GetActivityMembers() only works with BotFrameworkAdapter");
            }

            string id = dc.Context.Activity.Id;
            if (this.activityId != null)
            {
                var (value, valueError) = this.activityId.TryEvaluate(dc.GetState());
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {this.ActivityId}. Error: {valueError}");
                }

                id = value as string;
            }

            var result = await bfAdapter.GetActivityMembersAsync(dc.Context, id, cancellationToken).ConfigureAwait(false);

            dc.GetState().SetValue(this.Property, result);

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.ActivityId ?? string.Empty},{this.Property ?? string.Empty}]";
        }
    }
}
