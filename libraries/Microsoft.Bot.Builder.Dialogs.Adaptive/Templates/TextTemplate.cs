// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
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

        public virtual async Task<string> BindAsync(DialogContext dialogContext, object data = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                throw new InvalidOperationException($"The {nameof(this.Template)} property can't be empty.");
            }

<<<<<<< HEAD
            LanguageGenerator languageGenerator = dialogContext.Services.Get<LanguageGenerator>();
=======
            var languageGenerator = dialogContext.Services.Get<LanguageGenerator>();
>>>>>>> f127fca9b2eef1fe51f52bbfb2fbbab8a10fc0e8
            if (languageGenerator != null)
            {
                var result = await languageGenerator.GenerateAsync(
                    dialogContext,
                    template: Template,
                    data: data ?? dialogContext.State,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return result.ToString();
            }

            return null;
        }

        public override string ToString()
        {
            return $"{nameof(TextTemplate)}({this.Template})";
        }
    }
}
