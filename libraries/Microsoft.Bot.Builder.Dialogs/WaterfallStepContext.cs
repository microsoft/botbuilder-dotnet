using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class WaterfallStepContext
    {
        private readonly WaterfallDialog _parent;
        private readonly DialogContext _dc;
        private bool _nextCalled;

        internal WaterfallStepContext(WaterfallDialog parent, DialogContext dc, DialogOptions options, IDictionary<string, object> values, int index, DialogReason reason, object result = null)
        {
            _parent = parent;
            _dc = dc;
            _nextCalled = false;
            Options = options;
            Values = values;
            Index = index;
            Reason = reason;
            Result = result;
        }

        /// <summary>
        /// The index of the current waterfall step being executed.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Any options the waterfall dialog was called with.
        /// </summary>
        public DialogOptions Options { get; }

        /// <summary>
        /// The reason the waterfall step is being executed.
        /// </summary>
        public DialogReason Reason { get; }

        /// <summary>
        /// Results returned by a dialog called in the previous waterfall step.
        /// </summary>
        public object Result { get; }

        /// <summary>
        /// A dictionary of values which will be persisted across all waterfall steps.
        /// </summary>
        public IDictionary<string, object> Values { get; }

        /// <summary>
        /// Used to skip to the next waterfall step.
        /// </summary>
        /// <param name="result">Optional result to pass to next step.</param>
        /// <returns></returns>
        public async Task<DialogTurnResult> NextAsync(object result = null)
        {
            // Ensure next hasn't been called
            if (_nextCalled)
            {
                throw new Exception($"WaterfallStepContext.NextAsync(): method already called for dialog and step '{_parent.Id}[{Index}]'.");
            }

            // Trigger next step
            _nextCalled = true;
            return await _parent.DialogResumeAsync(_dc, DialogReason.NextCalled, result).ConfigureAwait(false);
        }
    }
}
