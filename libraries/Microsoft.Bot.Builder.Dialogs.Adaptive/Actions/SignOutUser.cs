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
    public class SignOutUser : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.SignOutUser";

        private Expression userId;
        private Expression disabled;

        [JsonConstructor]
        public SignOutUser([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// <value>Expression to userId.  If there is no expression, then the current user.id will be used.</value>
        [JsonProperty("userId")]
        public string UserId
        {
            get { return userId?.ToString(); }
            set { this.userId = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>The name of the OAuth connection.</value>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

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

            string userId = null;
            var (result, error) = new ExpressionEngine().Parse(this.UserId).TryEvaluate(dc.GetState());
            if (result != null)
            {
                userId = result as string;
            }

            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("SignoutUser(): not supported by the current adapter");
            }

            await adapter.SignOutUserAsync(dc.Context, this.ConnectionName, (string)userId, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({ConnectionName}, {UserId})";
        }
    }
}
