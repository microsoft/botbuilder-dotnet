// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates
{
    /// <summary>
    /// Defins a static activity as a template.
    /// </summary>
    public class StaticActivityTemplate : ITemplate<Activity>
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.StaticActivityTemplate";

        public StaticActivityTemplate()
        {
        }

        public StaticActivityTemplate(Activity activity)
        {
            this.Activity = activity;
        }

        [JsonProperty("activity")]
        public Activity Activity { get; set; }

        public Task<Activity> BindToDataAsync(ITurnContext context, object data)
        {
            return Task.FromResult(Activity);
        }

        public override string ToString()
        {
            return $"{this.Activity.Text}";
        }
    }
}
