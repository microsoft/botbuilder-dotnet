// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    using CodeActionHandler = Func<DialogContext, object, Task<DialogTurnResult>>;

    public class CodeAction : DialogAction
    {
        private readonly CodeActionHandler codeHandler;

        public CodeAction(CodeActionHandler codeHandler, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0) : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.codeHandler = codeHandler ?? throw new ArgumentNullException(nameof(codeHandler));
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            return await this.codeHandler(dc, options).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"CodeAction({codeHandler.ToString()})";
        }
    }
}
