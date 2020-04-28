// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Calls BotFrameworkAdapter.GetConversationMembers () and sets the result to a memory property.
    /// </summary>
    public class GetConversationMembers : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.GetConversationMembers";

        [JsonConstructor]
        public GetConversationMembers([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

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

            var bfAdapter = dc.Context.Adapter as BotFrameworkAdapter;
            if (bfAdapter == null)
            {
                throw new Exception("GetActivityMembers() only works with BotFrameworkAdapter");
            }

            var result = await bfAdapter.GetConversationMembersAsync(dc.Context, cancellationToken).ConfigureAwait(false);

            if (this.Property != null)
            {
                dc.State.SetValue(this.Property.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.Property?.ToString() ?? string.Empty}]";
        }
    }
}
