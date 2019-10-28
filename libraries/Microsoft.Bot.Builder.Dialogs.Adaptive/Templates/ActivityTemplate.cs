// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates
{
    /// <summary>
    /// Defines an activity Template where the template expression is local aka "inline" 
    /// and processed through registered IActivityGenerator/ILanguageGenerator.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    public class ActivityTemplate : ITemplate<Activity>
    {
        // Fixed text constructor for inline template
        public ActivityTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the activity.
        /// </summary>
        /// <value>
        /// The template to evaluate to create the activity.
        /// </value>
        public string Template { get; set; }

        public virtual async Task<Activity> BindToData(ITurnContext context, object data)
        {
            if (!string.IsNullOrEmpty(this.Template))
            {
                var languageGenerator = context.TurnState.Get<ILanguageGenerator>();
                var lgStringResult = await languageGenerator.Generate(context, this.Template, data).ConfigureAwait(false);
                var result = ActivityFactory.CreateActivity(lgStringResult);
                return result;
            }

            return null;
        }

        public override string ToString()
        {
            return $"{nameof(ActivityTemplate)}({this.Template})";
        }
    }
}
