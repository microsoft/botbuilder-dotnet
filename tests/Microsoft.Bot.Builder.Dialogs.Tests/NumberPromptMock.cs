// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class NumberPromptMock : NumberPrompt<int>
    {
        public NumberPromptMock(string dialogId, PromptValidator<int> validator = null, string defaultLocale = null)
            : base(dialogId, validator, defaultLocale)
        {
        }

        public async Task OnPromptNullContext(object options, CancellationToken cancellationToken = default)
        {
            var opt = (PromptOptions)options;

            // should throw ArgumentNullException
            await OnPromptAsync(turnContext: null, state: null, options: opt, isRetry: false);
        }

        public async Task OnPromptNullOptions(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // should throw ArgumentNullException
            await OnPromptAsync(dc.Context, state: null, options: null, isRetry: false);
        }

        public async Task OnRecognizeNullContext(CancellationToken cancellationToken = default)
        {
            // should throw ArgumentNullException
            await OnRecognizeAsync(turnContext: null, state: null, options: null);
        }
    }
}
