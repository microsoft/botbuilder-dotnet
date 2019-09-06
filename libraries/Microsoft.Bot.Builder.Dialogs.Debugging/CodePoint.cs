using System;

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
                var state = DialogContext.State;
                return new
                {
                    user = state.User,
                    conversation = state.Conversation,
                    dialog = DialogContext.ActiveDialog != null ? state.Dialog : null,
                    turn = state.Turn,
                };
            }
        }

        private ICodeModel CodeModel { get; }

        private DialogContext DialogContext { get; }

        public override string ToString() => Name;

        object ICodePoint.Evaluate(string expression) => DialogContext.State.GetValue<object>(expression);
    }
}
