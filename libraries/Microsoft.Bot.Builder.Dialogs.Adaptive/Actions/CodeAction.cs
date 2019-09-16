// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    using CodeActionHandler = System.Func<Microsoft.Bot.Builder.Dialogs.DialogContext, object, System.Threading.Tasks.Task<Microsoft.Bot.Builder.Dialogs.DialogTurnResult>>;

    public class CodeAction : Dialog
    {
        private readonly CodeActionHandler codeHandler;

        public CodeAction(CodeActionHandler codeHandler, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.codeHandler = codeHandler ?? throw new ArgumentNullException(nameof(codeHandler));
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            return await this.codeHandler(dc, options).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({codeHandler.ToString()})";
        }
    }
}
