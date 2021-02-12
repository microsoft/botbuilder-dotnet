// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Calls TeamsInfo.GetPagedMembers to retrieve a paginated list of members of
    /// one-on-one, group, or team conversation and sets the result to a memory property.
    /// </summary>
    public class GetPagedMembers : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.GetPagedMembers";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPagedMembers"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public GetPagedMembers([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            RegisterSourceLocation(callerPath, callerLine);
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

        /// <summary>
        /// Gets or sets the expression to get the value to use for the continuationToken.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for continuationToken. Default value is null.
        /// </value>
        [JsonProperty("continuationToken")]
        public StringExpression ContinuationToken { get; set; }
        
        /// <summary>
        /// Gets or sets the expression to get the value to use for the page size.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for page size. Default value is null.
        /// </value>
        [JsonProperty("pageSize")]
        public IntExpression PageSize { get; set; }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }
            
            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.ChannelId != Channels.Msteams)
            {
                throw new InvalidOperationException($"{Kind} works only on the Teams channel.");
            }

            string continuationToken = ContinuationToken.GetValueOrNull(dc.State);
            int? pageSize = PageSize.GetValueOrNull(dc.State);

            var result = await TeamsInfo.GetPagedMembersAsync(dc.Context, pageSize, continuationToken, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (Property != null)
            {
                dc.State.SetValue(Property.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{ContinuationToken?.ToString() ?? string.Empty},{PageSize?.ToString() ?? string.Empty},{Property?.ToString() ?? string.Empty}]";
        }
    }
}
