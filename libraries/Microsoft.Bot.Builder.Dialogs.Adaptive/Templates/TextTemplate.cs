// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Templates
{
    /// <summary>
    /// Defines an text Template where the template expression is local aka "inline"
    /// and processed through registered ILanguageGenerator.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    public class TextTemplate : ITemplate<string>
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TextTemplate";

        // Fixed text constructor for inline template
        public TextTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the text.
        /// </summary>
        /// <value>
        /// The template to evaluate to create the text.
        /// </value>
        [JsonProperty("template")]
        public string Template { get; set; }

        public virtual async Task<string> BindAsync(DialogContext dialogContext, object data = null)
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                throw new ArgumentNullException(nameof(this.Template));
            }

            LanguageGenerator languageGenerator = dialogContext.Services.Get<LanguageGenerator>();
            if (languageGenerator != null)
            {
                var result = await languageGenerator.Generate(
                    dialogContext,
                    template: Template,
                    data: data ?? dialogContext.State).ConfigureAwait(false);
                return result;
            }

            return null;
        }

        public override string ToString()
        {
            return $"{nameof(TextTemplate)}({this.Template})";
        }
    }
}
