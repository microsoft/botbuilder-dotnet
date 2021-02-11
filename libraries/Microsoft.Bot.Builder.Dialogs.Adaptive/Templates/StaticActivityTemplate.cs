// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates
{
    /// <summary>
    /// Defins a static activity as a template.
    /// </summary>
    [JsonConverter(typeof(StaticActivityTemplateConverter))]
    public class StaticActivityTemplate : ITemplate<Activity>
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.StaticActivityTemplate";

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticActivityTemplate"/> class.
        /// </summary>
        public StaticActivityTemplate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticActivityTemplate"/> class.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/>.</param>
        public StaticActivityTemplate(Activity activity)
        {
            this.Activity = activity;
        }

        /// <summary>
        /// Gets or sets activity.
        /// </summary>
        /// <value>
        /// <see cref="Activity"/>.
        /// </value>
        [JsonProperty("activity")]
        public Activity Activity { get; set; }

        /// <summary>
        /// Gets the activity.
        /// </summary>
        /// <param name="context">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="data">Optional, data to bind to. If Null, then dc.State will be used.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> for this task.</param>
        /// <returns>Task representing an activity.</returns>
        public Task<Activity> BindAsync(DialogContext context, object data = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Activity);
        }

        /// <summary>
        /// Returns a string that represents <see cref="StaticActivityTemplate"/>.
        /// </summary>
        /// <returns>A string that represents <see cref="StaticActivityTemplate"/>.</returns>
        public override string ToString()
        {
            return $"{this.Activity.Text}";
        }
    }
}
