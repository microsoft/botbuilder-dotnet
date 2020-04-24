// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
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
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ActivityTemplate";

        // Fixed text constructor for inline template
        public ActivityTemplate()
        {
        }

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

        public virtual async Task<Activity> BindAsync(DialogContext dialogContext, object data = null)
        {
            if (!string.IsNullOrEmpty(this.Template))
            {
                var languageGenerator = dialogContext.Services.Get<LanguageGenerator>();
                if (languageGenerator != null)
                {
                    var lgStringResult = await languageGenerator.Generate(dialogContext, this.Template, data ?? dialogContext.State).ConfigureAwait(false);
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

        public override string ToString()
        {
            return $"{nameof(ActivityTemplate)}({this.Template})";
        }
    }
}
