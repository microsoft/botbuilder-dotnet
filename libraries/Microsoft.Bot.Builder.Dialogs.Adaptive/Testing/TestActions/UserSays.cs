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
    /// Send an text to the bot.
    /// </summary>
    public class UserSays : TestAction
    {
        public UserSays()
        {
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// the text to send to the bot.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the User name.
        /// </summary>
        /// <value>
        /// If user is set then the channalAccount.Id and channelAccount.Name will be from user.
        /// </value>
        public string User { get; set; }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            if (this.Text == null)
            {
                throw new Exception("You must define the Text property");
            }

            var activity = adapter.MakeActivity(this.Text);
            if (!string.IsNullOrEmpty(this.User))
            {
                activity.From = ObjectPath.Clone(activity.From);
                activity.From.Id = this.User;
                activity.From.Name = this.User;
            }

            await adapter.ProcessActivityAsync(activity, callback, default(CancellationToken)).ConfigureAwait(false);
        }
    }
}
