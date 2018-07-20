// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An activity that signals to the <see cref="BotAdapter"/> that it should introduce a <see cref="Delay">delay</see> 
    /// before sending any other activities.
    /// </summary>
    /// <remarks>
    /// Utilize this activity when you want to emulate a more natural conversation experience with your users rather than 
    /// flooding them with too many response activities at once.
    /// </remarks>
    /// <seealso cref="TypingActivity"/>
    public class DelayActivity : Activity
    {
        public DelayActivity()
            : base(ActivityTypesEx.Delay)
        {
        }

        /// <summary>
        /// Gets or sets the amount of time to delay before any other activities will be sent through.
        /// </summary>
        /// <value>The amount of time to delay.</value>
        public TimeSpan Delay { get; set; }
    }
}
