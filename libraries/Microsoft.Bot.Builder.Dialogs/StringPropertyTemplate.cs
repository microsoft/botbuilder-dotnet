using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    /// <summary>
    /// Defines a template for a string using property semantics.
    /// </summary>
    /// <remarks>
    /// This defines an string template which is driven by the types array and property name.
    /// </remarks>
    public class StringPropertyTemplate : ITextTemplate
    {
        public StringPropertyTemplate()
        {
        }

        // Fixed text constructor
        public StringPropertyTemplate(IEnumerable<string> types, string property)
        {
            this.Types = types.ToList();
            this.Property = property ?? throw new ArgumentNullException(nameof(Property));
        }

        public List<string> Types { get; set; }

        public string Property { get; set; }

        public async Task<string> BindToData(ITurnContext turnContext, object data)
        {
            ILanguageGenerator languageGenerator = turnContext.TurnState.Get<ILanguageGenerator>();
            if (languageGenerator != null)
            {
                var result = await languageGenerator.Generate(
                    turnContext,
                    template: $"[{this.Property}]",
                    data: data).ConfigureAwait(false);
                return result;
            }

            throw new Exception("There is no global ILanguageGenerator registered with the application! are you missing a .Use(new RegisterClasMiddleware(new LanguageGenerator())); in your middleware?");
        }

    }
}
