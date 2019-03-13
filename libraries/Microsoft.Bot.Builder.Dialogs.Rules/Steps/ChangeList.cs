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
            push,
            pop,
            take,
            remove,
            clear
        }


        public ChangeList()
            : base()
        {
        }

        protected override string OnComputeId()
        {
            return $"list[{changeType + ": " + listProperty}]";
        }

        public ChangeListType changeType;

        public string listProperty;

        public string itemProperty;

        public ChangeList(ChangeListType changeType, string listProperty = null, string itemProperty = null)
            : base()
        {
            this.changeType = changeType;

            if (!string.IsNullOrEmpty(listProperty))
            {
                this.listProperty = listProperty;
            }

            if (!string.IsNullOrEmpty(itemProperty))
            {
                this.itemProperty = itemProperty;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(listProperty))
            {
                throw new Exception($"ChangeList: \"{ changeType }\" operation couldn't be performed because the listProperty wasn't specified.");
            }

            var list = dc.State.GetValue(listProperty, new List<object>());

            object item = null;
            string serialized = string.Empty;
            object lastResult = null;

            switch (changeType)
            {
                case ChangeListType.pop:
                    item = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    if (!string.IsNullOrEmpty(itemProperty))
                    {
                        dc.State.SetValue(itemProperty, item);
                    }
                    lastResult = item;
                    break;
                case ChangeListType.push:
                    ensureItemProperty();
                    item = dc.State.GetValue<object>(itemProperty);
                    lastResult = item != null;
                    if ((bool)lastResult)
                    {
                        list.Add(item);
                    }
                    break;
                case ChangeListType.take:
                    if (list.Count == 0)
                    {
                        break;
                    }
                    item = list[0];
                    list.RemoveAt(0);
                    if (!string.IsNullOrEmpty(itemProperty))
                    {
                        dc.State.SetValue(itemProperty, item);
                    }
                    lastResult = item;
                    break;
                case ChangeListType.remove:
                    ensureItemProperty();
                    item = dc.State.GetValue<object>(itemProperty);
                    if (item != null)
                    {
                        lastResult = false;
                        list.Remove(item);
                    }
                    break;
                case ChangeListType.clear:
                    lastResult = list.Count > 0;
                    list.Clear();
                    break;
            }

            dc.State.SetValue(listProperty, list);
            dc.State.SetValue("dialog.lastResult", lastResult);
            return await dc.EndDialogAsync();
        }

        private void ensureItemProperty()
        {
            if (string.IsNullOrEmpty(itemProperty))
            {
                throw new Exception($"ChangeList: \"{ changeType }\" operation couldn't be performed for list \"{listProperty}\" because an itemProperty wasn't specified.");
            }
        }

    }
}
