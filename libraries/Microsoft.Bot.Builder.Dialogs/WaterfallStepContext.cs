// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class WaterfallStepContext : DialogContext
    {
        private readonly WaterfallDialog _parent;
        private bool _nextCalled;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterfallStepContext"/> class.
        /// Provides context for a turn of a waterfall dialog. Contains ITurnContext as property 'Context'.
        /// </summary>
        /// <param name= "parent">The parent of the waterfall dialog.</param>
        /// <param name= "dc">The dialog's context.</param>
        /// <param name= "options">Any options to call the waterfall dialog with.</param>
        /// <param name= "values">A dictionary of values which will be persisted across all waterfall steps.</param>
        /// <param name= "index">The index of the current waterfall to execute.</param>
        /// <param name= "reason">The reason the waterfall step is being executed.</param>
        /// <param name= "result">Results returned by a dialog called in the previous waterfall step.</param>
        internal WaterfallStepContext(WaterfallDialog parent, DialogContext dc, object options,  IDictionary<string, object> values, int index, DialogReason reason, object result = null)
            : base(dc.Dialogs, dc.Context, new DialogState(dc.Stack))
        {
            _parent = parent;
            _nextCalled = false;
            Parent = dc.Parent;
            Index = index;
            Options = options;
            Reason = reason;
            Result = result;
            Values = values;
        }

        /// <summary>
        /// Gets the index of the current waterfall step being executed.
        /// </summary>
        /// <value>
        /// The index of the current waterfall step being executed.
        /// </value>
        public int Index { get; }

        /// <summary>
        /// Gets any options the waterfall dialog was called with.
        /// </summary>
        /// <value>
        /// Any options the waterfall dialog was called with.
        /// </value>
        public object Options { get; }

        /// <summary>
        /// Gets the reason the waterfall step is being executed.
        /// </summary>
        /// <value>
        /// The reason the waterfall step is being executed.
        /// </value>
        public DialogReason Reason { get; }

        /// <summary>
        /// Gets results returned by a dialog called in the previous waterfall step.
        /// </summary>
        /// <value>
        /// Results returned by a dialog called in the previous waterfall step.
        /// </value>
        public object Result { get; }

        /// <summary>
        /// Gets a dictionary of values which will be persisted across all waterfall steps.
        /// </summary>
        /// <value>
        /// A dictionary of values which will be persisted across all waterfall steps.
        /// </value>
        public IDictionary<string, object> Values { get; }

        /// <summary>
        /// Used to skip to the next waterfall step.
        /// </summary>
        /// <param name="result">Optional result to pass to next step.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> of <see cref="DialogTurnResult"/> representing the asynchronous operation.</returns>
        public async Task<DialogTurnResult> NextAsync(object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ensure next hasn't been called
            if (_nextCalled)
            {
                throw new Exception($"WaterfallStepContext.NextAsync(): method already called for dialog and step '{_parent.Id}[{Index}]'.");
            }

            // Trigger next step
            _nextCalled = true;
            return await _parent.ResumeDialogAsync(this, DialogReason.NextCalled, result).ConfigureAwait(false);
        }
    }
}
