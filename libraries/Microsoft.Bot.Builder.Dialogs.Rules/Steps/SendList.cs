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
            return $"sendList[{listProperty}]";
        }

        public string listProperty;

        public string messageTemplate
        {
            set; get;
        }
        
        public string itemTemplate
        {
            set; get;
        }

        public SendList(string listProperty, string messageTemplate = null, string itemTemplate = null)
        {
          
            if (!string.IsNullOrEmpty(listProperty))
            {
                this.listProperty = listProperty;
            }

            if (!string.IsNullOrEmpty(messageTemplate))
            {
                this.messageTemplate = messageTemplate;
            }

            if (!string.IsNullOrEmpty(itemTemplate))
            {
                this.itemTemplate = itemTemplate;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(messageTemplate))
            {
                messageTemplate = "{list}";
            }
            else if (messageTemplate.IndexOf("{list") < 0)
            {
                messageTemplate += "\n\n{list}";
            }

            if (string.IsNullOrEmpty(itemTemplate))
            {
                itemTemplate = "- {item}\n";
            }
            else if (this.itemTemplate.IndexOf("{item") < 0)
            {
                itemTemplate += " {item}\n";
            }

            var list = string.Empty;
            var value = dc.State.GetValue<List<object>>(listProperty);

            foreach (var v in value)
            {
                list += itemTemplate.Replace("{item}", v.ToString());
            }

            var activity = messageTemplate.Replace("{list}", list);

            if (!string.IsNullOrEmpty(activity))
            {
                var result = await dc.Context.SendActivityAsync(activity);
                return await dc.EndDialogAsync(result);
            }
            return await dc.EndDialogAsync();
        }

    }
}
