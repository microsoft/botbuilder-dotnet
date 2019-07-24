using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public static class JObjectExtensions
    {
        public static bool IsPropertyValue(this JToken value, string name) =>
            value.Parent is JProperty jprop && jprop.Name == name;
    }
}
