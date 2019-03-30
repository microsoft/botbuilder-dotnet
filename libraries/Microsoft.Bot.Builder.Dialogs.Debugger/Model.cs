using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugger
{
    public sealed class VariableModel
    {
        public VariableModel(string name, object value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; }
        public object Value { get; }

        public override string ToString() => $"{Name}={Value}";
    }

    public sealed class FrameModel
    {
        public FrameModel(string name, object item, VariableModel scopes)
        {
            Name = name;
            Item = item;
            Scopes = scopes;
        }
        public string Name { get; }
        public object Item { get; }
        public VariableModel Scopes { get; }
        public override string ToString() => $"{Name}:{Item}";
    }

    public static class Model
    {
        public static IReadOnlyList<FrameModel> FramesFor(DialogContext dialogContext, object item, string more)
        {
            VariableModel scope = null;

            var frames = new List<FrameModel>();
            while (dialogContext != null)
            {
                foreach (var instance in dialogContext.Stack)
                {
                    var state = dialogContext.State;
                    var data = new
                    {
                        user = state.User,
                        conversation = state.Conversation,
                        dialog = dialogContext.ActiveDialog != null ? state.Dialog : null,
                        turn = state.Turn,
                        entities = state.Entities,
                        tags = dialogContext.ActiveTags,
                    };

                    scope = new VariableModel(nameof(data) + frames.Count, data);
                    var dialog = dialogContext.FindDialog(instance.Id);
                    var frame = new FrameModel(instance.Id, dialog, scope);
                    frames.Add(frame);
                }

                dialogContext = dialogContext.Parent;
            }

            if (item != null)
            {
                var name = $"{Policy.NameFor(item)}:{more}";
                frames.Insert(0, new FrameModel(name, item, scope));
            }

            return frames;
        }

        public static IReadOnlyList<VariableModel> VariablesFor(VariableModel variable)
        {
            var variables = new List<VariableModel>();
            var value = variable.Value;
            if (value is IReadOnlyDictionary<string, object> dictionary)
            {
                foreach (var kv in dictionary)
                {
                    variables.Add(new VariableModel(kv.Key, kv.Value));
                }
            }
            else if (value is IEnumerable<object> items)
            {
                int index = 0;
                foreach (var item in items)
                {
                    variables.Add(new VariableModel(index.ToString(), item));
                    ++index;
                }
            }
            else if (value != null)
            {
                var type = value.GetType();
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var index = property.GetIndexParameters();
                    if (index.Length == 0)
                    {
                        variables.Add(new VariableModel(property.Name, property.GetValue(value)));
                    }
                }
            }

            variables.RemoveAll(v => !Policy.ShowToDebugger(v.Value));

            return variables;
        }
    }
}
