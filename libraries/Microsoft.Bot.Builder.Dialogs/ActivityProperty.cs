using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{

    public class ActivityProperty : IActivityTemplate
    {

        public ActivityProperty()
        {
        }

        // Fixed text constructor
        public ActivityProperty(IEnumerable<string> types, string property)
        {
            this.Types = types.ToList();
            this.Property = property ?? throw new ArgumentNullException(nameof(Property));
        }

        public List<string> Types { get; set; }

        public string Property { get; set; }

        public async Task<Activity> BindToActivity(ITurnContext context, object data)
        {
            IMessageGenerator messageGenerator = context.TurnState.Get<IMessageGenerator>();
            if (messageGenerator != null)
            {
                var result = await messageGenerator.Generate(
                    context.Activity.Locale,
                    inlineTemplate: null,
                    id: this.Property,
                    data: data,
                    tags: null,
                    types: this.Types.ToArray()).ConfigureAwait(false);
                return (Activity)result;
            }
            return null;
        }

        public static List<string> GetTypes(Type type)
        {
            var types = new List<string>();
            while (type != typeof(object))
            {
                types.Add($"{type.Namespace}.{type.Name}");
                type = type.BaseType;
            }

            return types;
        }
    }
}
