using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    internal class FormSelector : ITriggerSelector
    {
        private readonly ITriggerSelector _selector;

        public FormSelector(ITriggerSelector selector)
        {
            _selector = selector;
        }

        public void Initialize(IEnumerable<TriggerHandler> rules, bool evaluate = true) 
            => _selector.Initialize(rules);

        public async Task<IReadOnlyList<TriggerHandler>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var candidates = await _selector.Select(context, cancel);
            var choices = new List<TriggerHandler>();
            if (candidates.Any())
            {
                var candidate = candidates.First();
                var choice = new OnDialogEvent(actions: new List<Dialog>(candidate.Actions));
                choice.Actions.Add(new EmitEvent(FormEvents.FillForm, bubble: false));
                choices.Add(choice);
            }

            return (IReadOnlyList<TriggerHandler>)choices;
        }
    }
}
