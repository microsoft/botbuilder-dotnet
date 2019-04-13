using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class CodeModel
    {
        public CodeModel(DialogContext dialogContext, string name, object item, object scopes)
        {
            DialogContext = dialogContext;
            Name = name;
            Item = item;
            Scopes = scopes;
        }
        internal DialogContext DialogContext { get; }
        public string Name { get; }
        public object Item { get; }
        public object Scopes { get; }
        public override string ToString() => $"{Name}:{Item}";

        public static string NameFor(object item) => item.GetType().Name;

        public static object ScopesFor(DialogContext dialogContext)
        {
            var state = dialogContext.State;
            return new
            {
                user = state.User,
                conversation = state.Conversation,
                dialog = dialogContext.ActiveDialog != null ? state.Dialog : null,
                turn = state.Turn,
                entities = state.Entities,
                tags = dialogContext.ActiveTags,
            };
        }

        public static IReadOnlyList<CodeModel> FramesFor(DialogContext dialogContext, object item, string more)
        {
            var frames = new List<CodeModel>();

            if (item != null)
            {
                var name = $"{CodeModel.NameFor(item)}:{more}";
                var scopes = ScopesFor(dialogContext);
                var frame = new CodeModel(dialogContext, name, item, scopes);
                frames.Add(frame);
            }

            while (dialogContext != null)
            {
                foreach (var instance in dialogContext.Stack)
                {
                    var scopes = ScopesFor(dialogContext);
                    var dialog = dialogContext.FindDialog(instance.Id);
                    var frame = new CodeModel(dialogContext, instance.Id, dialog, scopes);
                    frames.Add(frame);
                }

                dialogContext = dialogContext.Parent;
            }

            return frames;
        }
    }
}
