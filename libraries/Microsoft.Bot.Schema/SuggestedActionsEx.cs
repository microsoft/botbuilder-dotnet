// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Schema
{
    public partial class SuggestedActions
    {
        /// <summary>
        /// Initializes a new instance of the SuggestedActions class.
        /// </summary>
        /// <param name="to">Ids of the recipients that the actions should be
        /// shown to. These Ids are relative to the channelId and a subset of
        /// all recipients of the activity.</param>
        /// <param name="actions">Actions that can be shown to the user.</param>
        /// <exception cref="ArgumentNullException"/>
        public SuggestedActions(IEnumerable<string> to, IEnumerable<CardAction> actions)
            : this(to.ToArray(), actions.ToArray()) { }
    }
}
