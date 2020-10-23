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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TextTemplate";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTemplate"/> class.
        /// </summary>
        /// <param name="template">The template to evaluate to create the string.</param>
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

        /// <summary>
        /// Given the turn context bind to the data to create the object of type string.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="data">Optional, data to bind to. If Null, then dc.State will be used.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> for this task.</param>
        /// <returns>Instance of string.</returns>
        public virtual async Task<string> BindAsync(DialogContext dialogContext, object data = null, CancellationToken cancellationToken = default)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (data is CancellationToken)
            {
                throw new ArgumentException($"{nameof(data)} cannot be a cancellation token");
            }

            if (string.IsNullOrEmpty(this.Template))
            {
                throw new InvalidOperationException($"The {nameof(this.Template)} property can't be empty.");
            }

            var languageGenerator = dialogContext.Services.Get<LanguageGenerator>();
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

        /// <summary>
        /// Returns a string that represents <see cref="TextTemplate"/>.
        /// </summary>
        /// <returns>A string that represents <see cref="TextTemplate"/>.</returns>
        public override string ToString()
        {
            return $"{nameof(TextTemplate)}({this.Template})";
        }
    }
}
