using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LanguageGeneratorMiddleware : IMiddleware
    {
        public LanguageGeneratorMiddleware(ILanguageGenerator languageGenerator)
        {
            this.LanguageGenerator = languageGenerator;
        }


        public ILanguageGenerator LanguageGenerator { get; set; }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            turnContext.TurnState.Add<ILanguageGenerator>(this.LanguageGenerator);
            await next(cancellationToken).ConfigureAwait(false);
            return;
        }
    }
}
