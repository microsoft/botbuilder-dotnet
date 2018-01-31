// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Asynchronous external event
    /// </summary>
    public interface IEventActivity : IActivity
    {
        /// <summary>
        /// Name of the event
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Open-ended value 
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Reference to another conversation or activity
        /// </summary>
        ConversationReference RelatesTo { get; set; }
    }
}
