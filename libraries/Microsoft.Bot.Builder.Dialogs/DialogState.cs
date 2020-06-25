// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Contains state information for the dialog stack.
    /// </summary>
    public class DialogState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogState"/> class.
        /// </summary>
        /// <remarks>The new instance is created with an empty dialog stack.</remarks>
        /// <seealso cref="DialogContext.Stack"/>
        /// <seealso cref="DialogSet(IStatePropertyAccessor{DialogState})"/>
        public DialogState()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogState"/> class.
        /// </summary>
        /// <param name="stack">The state information to initialize the stack with.</param>
        /// <remarks>The new instance has a dialog stack that is populated using the information
        /// in <paramref name="stack"/>.</remarks>
        public DialogState(List<DialogInstance> stack)
        {
            DialogStack = stack ?? new List<DialogInstance>();
        }

        /// <summary>
        /// Gets or sets the state information for a dialog stack.
        /// </summary>
        /// <value>State information for a dialog stack.</value>
        [JsonProperty("dialogStack")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<DialogInstance> DialogStack { get; set; } = new List<DialogInstance>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
