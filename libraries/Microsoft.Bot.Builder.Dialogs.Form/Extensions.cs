// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public static class Extensions
    {
        public static bool IsPropertyValue(this JToken value, string name) =>
            value.Parent is JProperty jprop && jprop.Name == name;

        public static T Dequeue<T>(this List<T> queue)
        {
            var result = default(T);
            if (queue.Count > 0)
            {
                result = queue[0];
                queue.RemoveAt(0);
            }

            return result;
        }
    }
}
