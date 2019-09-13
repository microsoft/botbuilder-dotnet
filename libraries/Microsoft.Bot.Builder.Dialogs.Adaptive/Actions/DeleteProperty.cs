// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Deletes a property from memory.
    /// </summary>
    public class DeleteProperty : DialogAction
    {
        [JsonConstructor]
        public DeleteProperty([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public DeleteProperty(string property, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            if (!string.IsNullOrEmpty(property))
            {
                this.Property = property;
            }
        }

        /// <summary>
        /// Gets or sets property to path to remove Example: user.age will remove "age" from "user".
        /// </summary>
        /// <value>
        /// Property to path to remove Example: user.age will remove "age" from "user".
        /// </value>
        public string Property { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Ensure planning context
            if (dc is SequenceContext planning)
            {
                dc.State.RemoveProperty(Property);
                return await dc.EndDialogAsync();
            }
            else
            {
                throw new Exception("`ClearProperty` should only be used in the context of an adaptive dialog.");
            }
        }
    }
}
