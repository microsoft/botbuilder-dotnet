using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class SendList : DialogCommand
    {
        public SendList()
            : base()
        {
        }

        protected override string OnComputeId()
        {
            return $"sendList[{ListProperty}]";
        }

        public string ListProperty { set; get; }

        public string MessageTemplate { set; get; }
        
        public string ItemTemplate { set; get; }

        public SendList(string listProperty, string messageTemplate = null, string itemTemplate = null)
        {
            this.ListProperty = listProperty;

            if (!string.IsNullOrEmpty(messageTemplate))
            {
                this.MessageTemplate = messageTemplate;
            }

            if (!string.IsNullOrEmpty(itemTemplate))
            {
                this.ItemTemplate = itemTemplate;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(MessageTemplate))
            {
                MessageTemplate = "{list}";
            }
            else if (MessageTemplate.IndexOf("{list") < 0)
            {
                MessageTemplate += "\n\n{list}";
            }

            if (string.IsNullOrEmpty(ItemTemplate))
            {
                ItemTemplate = "- {item}\n";
            }
            else if (this.ItemTemplate.IndexOf("{item") < 0)
            {
                ItemTemplate += " {item}\n";
            }

            var list = string.Empty;
            var value = dc.State.GetValue<List<object>>(ListProperty);

            foreach (var v in value)
            {
                list += ItemTemplate.Replace("{item}", v.ToString());
            }

            var activity = MessageTemplate.Replace("{list}", list);

            if (!string.IsNullOrEmpty(activity))
            {
                var result = await dc.Context.SendActivityAsync(activity);
                return await dc.EndDialogAsync(result);
            }
            return await dc.EndDialogAsync();
        }

    }
}
