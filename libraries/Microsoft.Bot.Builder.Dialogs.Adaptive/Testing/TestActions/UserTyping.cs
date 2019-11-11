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

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    public class UserTyping : TestAction
    { 
        public UserTyping()
        { 
        }

        /// <summary>
        /// Gets or sets the User name.
        /// </summary>
        /// <value>
        /// If user is set then the channalAccount.Id and channelAccount.Name will be from user.
        /// </value>
        public string User { get; set; }

        public async override Task ExecuteAsync(TestAdapter adapter, BotCallbackHandler callback)
        {
            var typing = adapter.MakeActivity();
            typing.Type = ActivityTypes.Typing;

            if (!string.IsNullOrEmpty(this.User))
            {
                typing.From = ObjectPath.Clone(typing.From);
                typing.From.Id = this.User;
                typing.From.Name = this.User;
            }

            await adapter.ProcessActivityAsync((Activity)typing, callback, default(CancellationToken)).ConfigureAwait(false);
        }
    }
}
