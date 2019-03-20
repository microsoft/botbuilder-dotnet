using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class ChangeList : DialogCommand
    {
        public enum ChangeListType
        {
            Push,
            Pop,
            Take,
            Remove,
            Clear
        }


        public ChangeList()
            : base()
        {
        }

        protected override string OnComputeId()
        {
            return $"list[{ChangeType + ": " + ListProperty}]";
        }

        public ChangeListType ChangeType { get; set; }

        public string ListProperty { get; set; }

        public string ItemProperty { get; set; }

        public ChangeList(ChangeListType changeType, string listProperty = null, string itemProperty = null)
            : base()
        {
            this.ChangeType = changeType;

            if (!string.IsNullOrEmpty(listProperty))
            {
                this.ListProperty = listProperty;
            }

            if (!string.IsNullOrEmpty(itemProperty))
            {
                this.ItemProperty = itemProperty;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(ListProperty))
            {
                throw new Exception($"ChangeList: \"{ ChangeType }\" operation couldn't be performed because the listProperty wasn't specified.");
            }

            var list = dc.State.GetValue(ListProperty, new List<object>());

            object item = null;
            string serialized = string.Empty;
            object lastResult = null;

            switch (ChangeType)
            {
                case ChangeListType.Pop:
                    item = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    if (!string.IsNullOrEmpty(ItemProperty))
                    {
                        dc.State.SetValue(ItemProperty, item);
                    }
                    lastResult = item;
                    break;
                case ChangeListType.Push:
                    EnsureItemProperty();
                    item = dc.State.GetValue<object>(ItemProperty);
                    lastResult = item != null;
                    if ((bool)lastResult)
                    {
                        list.Add(item);
                    }
                    break;
                case ChangeListType.Take:
                    if (list.Count == 0)
                    {
                        break;
                    }
                    item = list[0];
                    list.RemoveAt(0);
                    if (!string.IsNullOrEmpty(ItemProperty))
                    {
                        dc.State.SetValue(ItemProperty, item);
                    }
                    lastResult = item;
                    break;
                case ChangeListType.Remove:
                    EnsureItemProperty();
                    item = dc.State.GetValue<object>(ItemProperty);
                    if (item != null)
                    {
                        lastResult = false;
                        list.Remove(item);
                    }
                    break;
                case ChangeListType.Clear:
                    lastResult = list.Count > 0;
                    list.Clear();
                    break;
            }

            dc.State.SetValue(ListProperty, list);
            dc.State.SetValue("dialog.lastResult", lastResult);
            return await dc.EndDialogAsync();
        }

        private void EnsureItemProperty()
        {
            if (string.IsNullOrEmpty(ItemProperty))
            {
                throw new Exception($"ChangeList: \"{ ChangeType }\" operation couldn't be performed for list \"{ListProperty}\" because an itemProperty wasn't specified.");
            }
        }

    }
}
