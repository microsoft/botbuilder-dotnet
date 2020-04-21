// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA.Utils
{
    internal class BindToActivity : ITemplate<Activity>
    {
        private Activity activity;

        public BindToActivity(Activity activity)
        {
            this.activity = activity;
        }

        public Task<Activity> BindToDataAsync(ITurnContext context, object data)
        {
            return Task.FromResult(activity);
        }

        public override string ToString()
        {
            return $"{this.activity.Text}";
        }
    }
}
