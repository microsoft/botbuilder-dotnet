using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{

    /// <summary>
    /// Defines an text Template where the template expression is local aka "inline".
    /// </summary>
    [DebuggerDisplay("{Template}")]
    public class TextTemplate : ITextTemplate
    {
        // Fixed text constructor for inline template
        public TextTemplate(string template)
        {
            this.Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Gets or sets the template to evaluate to create the IMessageActivity.
        /// </summary>
        public string Template { get; set; }

        public async Task<string> BindToData(ITurnContext context, object data)
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                throw new ArgumentNullException(nameof(this.Template));
            }

            ILanguageGenerator languageGenerator = context.TurnState.Get<ILanguageGenerator>();
            if (languageGenerator != null)
            {
                var result = await languageGenerator.Generate(
                    context.Activity.Locale,
                    inlineTemplate: Template,
                    id: null,
                    data: data,
                    tags: null,
                    types: null).ConfigureAwait(false);
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
