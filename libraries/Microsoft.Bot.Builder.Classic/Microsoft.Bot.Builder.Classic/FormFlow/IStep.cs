// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    internal enum StepPhase { Ready, Responding, Completed };
    internal enum StepType { Field, Confirm, Navigation, Message };

    internal struct StepResult
    {
        internal StepResult(bool success, NextStep next, FormPrompt feedback, FormPrompt prompt)
        {
            this.Success = success;
            this.Next = next;
            this.Feedback = feedback;
            this.Prompt = prompt;
        }

        internal NextStep Next { get; set; }
        internal FormPrompt Feedback { get; set; }
        internal FormPrompt Prompt { get; set; }
        internal bool Success { get; set; }
    }

    internal interface IStep<T>
        where T : class
    {
        string Name { get; }

        StepType Type { get; }

        TemplateBaseAttribute Annotation { get; }

        IField<T> Field { get; }

        void SaveResources();

        void Localize();

        bool Active(T state);

        Task<bool> DefineAsync(T state);

        FormPrompt Start(IDialogContext context, T state, FormState form);

        bool InClarify(FormState form);

        IEnumerable<TermMatch> Match(IDialogContext context, T state, FormState form, IMessageActivity input);

        Task<StepResult> ProcessAsync(IDialogContext context, T state, FormState form, IMessageActivity input, IEnumerable<TermMatch> matches);

        FormPrompt NotUnderstood(IDialogContext context, T state, FormState form, IMessageActivity input);

        FormPrompt Help(T state, FormState form, string commandHelp);

        bool Back(IDialogContext context, T state, FormState form);

        IEnumerable<string> Dependencies { get; }
    }

}
