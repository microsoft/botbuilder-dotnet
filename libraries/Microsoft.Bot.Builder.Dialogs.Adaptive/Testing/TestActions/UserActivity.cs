// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Send an activity to the bot.
    /// </summary>
    public class UserActivity : TestAction
    {
        public UserActivity()
        {
        }

        public Activity Activity { get; set; }

        /// <summary>
        /// Gets or sets the User name.
        /// </summary>
        /// <value>
        /// If user is set then the channalAccount.Id and channelAccount.Name will be from user.
        /// </value>
        public string User { get; set; }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            if (this.Activity == null)
            {
                throw new Exception("You must define one of Text or Activity properties");
            }

            var activity = ObjectPath.Clone(this.Activity);
            activity.ApplyConversationReference(adapter.Conversation, isIncoming: true);

            if (!string.IsNullOrEmpty(this.User))
            {
                activity.From = ObjectPath.Clone(activity.From);
                activity.From.Id = this.User;
                activity.From.Name = this.User;
            }

            await adapter.ProcessActivityAsync(this.Activity, callback, default(CancellationToken)).ConfigureAwait(false);
        }
    }
}
