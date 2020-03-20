// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class CodePoint : ICodePoint
    {
        public CodePoint(ICodeModel codeModel, DialogContext dialogContext, object item, string more)
        {
            CodeModel = codeModel ?? throw new ArgumentNullException(nameof(codeModel));
            DialogContext = dialogContext ?? throw new ArgumentNullException(nameof(dialogContext));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            More = more;
        }

        public object Item { get; }

        public string More { get; }

        public string Name => CodeModel.NameFor(Item) + (More != null ? ":" + More : string.Empty);

        public object Data
        {
            get
            {
                // try to avoid regenerating Identifier values within a breakpoint
                if (CachedData == null)
                {
                    CachedData = DialogContext.State.GetMemorySnapshot();
                }

                return CachedData;
            }
        }

        private object CachedData { get; set; }

        private ICodeModel CodeModel { get; }

        private DialogContext DialogContext { get; }

        public override string ToString() => Name;

        object ICodePoint.Evaluate(string expression) => DialogContext.State.GetValue<object>(expression);
    }
}
