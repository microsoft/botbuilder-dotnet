using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Steps
{
    public class SaveEntity : DialogCommand
    {
        public SaveEntity() : base()
        { }

        public SaveEntity(string entityName, string property)
            : base()
        {
            if (!string.IsNullOrEmpty(entityName))
            {
                this.EntityName = entityName;
            }

            if (!string.IsNullOrEmpty(property))
            {
                this.Property = property;
            }
        }

        public string EntityName { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dc.State.Entities.ContainsKey(EntityName))
            {
                var values = dc.State.Entities[EntityName];
                if (values.GetType() == typeof(JArray))
                {
                    dc.State.SetValue(Property, ((JArray)values)[0]);
                }
                else
                {
                    dc.State.SetValue(Property, values);
                }
            }
            return await dc.EndDialogAsync();
        }
    }
}
