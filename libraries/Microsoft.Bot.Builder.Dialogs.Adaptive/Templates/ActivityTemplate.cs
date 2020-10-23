// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates
{
    /// <summary>
    /// Defines an activity Template where the template expression is local aka "inline" 
    /// and processed through registered IActivityGenerator/ILanguageGenerator.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    [JsonConverter(typeof(ActivityTemplateConverter))]
    public class ActivityTemplate : ITemplate<Activity>
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ActivityTemplate";

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTemplate"/> class.
        /// </summary>
        public ActivityTemplate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTemplate"/> class.
        /// </summary>
        /// <param name="template">The template to evaluate to create the activity.</param>
        public ActivityTemplate(string template)
        {
            this.Template = template;
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the activity.
        /// </summary>
        /// <value>
        /// The template to evaluate to create the activity.
        /// </value>
        [JsonProperty("template")]
        public string Template { get; set; }

        /// <summary>
        /// Given the turn context bind to the data to create the object of type <see cref="Activity"/>.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="data">Optional, data to bind to. If Null, then dc.State will be used.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> for this task.</param>
        /// <returns>Instance of <see cref="Activity"/>.</returns>
        public virtual async Task<Activity> BindAsync(DialogContext dialogContext, object data = null, CancellationToken cancellationToken = default)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (data is CancellationToken)
            {
                throw new ArgumentException($"{nameof(data)} cannot be a cancellation token");
            }

            if (!string.IsNullOrEmpty(this.Template))
            {
                var languageGenerator = dialogContext.Services.Get<LanguageGenerator>();
                if (languageGenerator != null)
                {
                    var lgStringResult = await languageGenerator.GenerateAsync(dialogContext, this.Template, data ?? dialogContext.State, cancellationToken).ConfigureAwait(false);
                    var result = ActivityFactory.FromObject(lgStringResult);
                    return result;
                }
                else
                {
                    var message = Activity.CreateMessageActivity();
                    message.Text = this.Template;
                    message.Speak = this.Template;
                    return message as Activity;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a string that represents <see cref="ActivityTemplate"/>.
        /// </summary>
        /// <returns>A string that represents <see cref="ActivityTemplate"/>.</returns>
        public override string ToString()
        {
            return $"{nameof(ActivityTemplate)}({this.Template})";
        }
    }
}
