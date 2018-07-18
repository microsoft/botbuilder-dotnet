// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An activity that enables the bot to provide a response to an <see cref="InvokeActivity"/>.
    /// </summary>
    /// <seealso cref="InvokeActivity"/>
    /// <seealso cref="InvokeResponse"/>
    public class InvokeResponseActivity : Activity
    {
        public InvokeResponseActivity()
            : base(ActivityTypesEx.InvokeResponse)
        {
        }

        /// <summary>
        /// Gets or sets a response that should be returned to the caller that issued the invoke request.
        /// </summary>
        /// <value>An <see cref="InvokeResponse"/> that will be returned to the caller.</value>
        public InvokeResponse Response { get; set; }
    }
}
