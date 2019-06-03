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

        public async Task<string> BindToData(ITurnContext turnContext, object data)
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                throw new ArgumentNullException(nameof(this.Template));
            }

            ILanguageGenerator languageGenerator = turnContext.TurnState.Get<ILanguageGenerator>();
            if (languageGenerator != null)
            {
                var result = await languageGenerator.Generate(
                    turnContext,
                    template: Template,
                    data: data).ConfigureAwait(false);
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
