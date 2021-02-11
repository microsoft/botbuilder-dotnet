// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA.Utils
{
    internal class BindToActivity : ITemplate<Activity>
    {
        private readonly Activity _activity;

        public BindToActivity(Activity activity)
        {
            _activity = activity;
        }

        public Task<Activity> BindAsync(DialogContext context, object data = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_activity);
        }

        public override string ToString()
        {
            return $"{_activity.Text}";
        }
    }
}
