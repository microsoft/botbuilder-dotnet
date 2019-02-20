using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class DataBinding
    {
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

        public static ITemplate<IMessageActivity> DefineMessageActivityProperty(this Dialog dialog, string name)
        {
            return new MessagePropertyTemplate(DataBinding.GetTypes(dialog.GetType()), name);
        }

        public static ITemplate<string> DefineStringProperty(this Dialog dialog, string name)
        {
            return new StringPropertyTemplate(DataBinding.GetTypes(dialog.GetType()), name);
        }
    }
}
