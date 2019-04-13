using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class CodeModel
    {
        public CodeModel(string name, object item, object scopes)
        {
            Name = name;
            Item = item;
            Scopes = scopes;
        }
        public string Name { get; }
        public object Item { get; }
        public object Scopes { get; }
        public override string ToString() => $"{Name}:{Item}";

        public static string NameFor(object item) => item.GetType().Name;

        public static IReadOnlyList<CodeModel> FramesFor(DialogContext dialogContext, object item, string more)
        {
            object scope = null;

            var frames = new List<CodeModel>();
            while (dialogContext != null)
            {
                foreach (var instance in dialogContext.Stack)
                {
                    var state = dialogContext.State;
                    scope = new
                    {
                        user = state.User,
                        conversation = state.Conversation,
                        dialog = dialogContext.ActiveDialog != null ? state.Dialog : null,
                        turn = state.Turn,
                        entities = state.Entities,
                        tags = dialogContext.ActiveTags,
                    };

                    var dialog = dialogContext.FindDialog(instance.Id);
                    var frame = new CodeModel(instance.Id, dialog, scope);
                    frames.Add(frame);
                }

                dialogContext = dialogContext.Parent;
            }

            if (item != null)
            {
                var name = $"{CodeModel.NameFor(item)}:{more}";
                frames.Insert(0, new CodeModel(name, item, scope));
            }

            return frames;
        }
    }
}
