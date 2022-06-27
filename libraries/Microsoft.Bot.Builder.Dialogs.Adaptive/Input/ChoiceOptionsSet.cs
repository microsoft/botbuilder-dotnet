// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Sets the ChoiceFactoryOptions.
    /// </summary>
    public class ChoiceOptionsSet : ChoiceFactoryOptions, ITemplate<ChoiceFactoryOptions>
    {
        private readonly string template;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceOptionsSet"/> class.
        /// </summary>
        public ChoiceOptionsSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceOptionsSet"/> class.
        /// </summary>
        /// <param name="obj">Choice options values.</param>
        public ChoiceOptionsSet(object obj)
        {
            if (obj is string template)
            {
                this.template = template;
            }
        }

        /// <inheritdoc/>
        public async Task<ChoiceFactoryOptions> BindAsync(DialogContext dialogContext, object data = null, CancellationToken cancellationToken = default)
        {
            if (template == null)
            {
                return this;
            }

            var languageGenerator = dialogContext.Services.Get<LanguageGenerator>() ?? throw new MissingMemberException(nameof(LanguageGeneration));
            var lgResult = await languageGenerator.GenerateAsync(dialogContext, template, dialogContext.State).ConfigureAwait(false);
            if (lgResult is ChoiceFactoryOptions cs)
            {
                return cs;
            }
            else if (lgResult is string str)
            {
                try
                {
                    var jObj = (JArray)JsonConvert.DeserializeObject(str);

                    var options = new ChoiceFactoryOptions
                    {
                        InlineSeparator = jObj[0].ToString(),
                        InlineOr = jObj[1].ToString(),
                        InlineOrMore = jObj[2].ToString(),
                    };

                    if (jObj.Count > 3)
                    {
                        options.IncludeNumbers = jObj[3].ToObject<bool>();
                    }

                    return options;
                }
                catch (JsonReaderException)
                {
                    return null;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            return null;
        }
    }
}
