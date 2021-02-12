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
    /// Send an activity back to the user.
    /// </summary>
    public class SignOutUser : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.SignOutUser";

        /// <summary>
        /// Initializes a new instance of the <see cref="SignOutUser"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the expression which resolves to the user Id to signout. (default is current user.id).
        /// </summary>
        /// <value>Expression to userId.  If there is no expression, then the current user.id will be used.</value>
        [JsonProperty("userId")]
        public StringExpression UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the OAuth connection.
        /// </summary>
        /// <value>The name of the OAuth connection.</value>
        [JsonProperty("connectionName")]
        public StringExpression ConnectionName { get; set; }

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

            string userId = this.UserId?.GetValue(dc.State);

            if (!(dc.Context.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("SignoutUser(): not supported by the current adapter");
            }

            var connectionName = this.ConnectionName?.GetValue(dc.State);
            await adapter.SignOutUserAsync(dc.Context, connectionName, (string)userId, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({ConnectionName?.ToString()}, {UserId?.ToString()})";
        }
    }
}
