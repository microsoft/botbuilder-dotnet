// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public static class JsonExtensions
    {
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

        /// <summary>
        /// Achieve deep data binding with recursion.
        /// </summary>
        /// <param name="token">Input token.</param>
        /// <param name="state">Memory scope.</param>
        /// <returns>Deep data binding result.</returns>
        public static JToken ReplaceJTokenRecursively(this JToken token, object state)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    // NOTE: ToList() is required because JToken.Replace will break the enumeration.
                    foreach (var child in token.Children<JProperty>().ToList())
                    {
                        child.Replace(child.ReplaceJTokenRecursively(state));
                    }

                    break;

                case JTokenType.Array:
                    // NOTE: ToList() is required because JToken.Replace will break the enumeration.
                    foreach (var child in token.Children().ToList())
                    {
                        child.Replace(child.ReplaceJTokenRecursively(state));
                    }

                    break;

                case JTokenType.Property:
                    JProperty property = (JProperty)token;
                    property.Value = property.Value.ReplaceJTokenRecursively(state);
                    break;

                default:
                    if (token.Type == JTokenType.String)
                    {
                        // if it is a "{bindingpath}" then run through expression parser and treat as a value
                        var (result, error) = new ValueExpression(token).TryGetValue(state);
                        if (error == null)
                        {
                            token = JToken.FromObject(result);
                        }
                    }

                    break;
            }

            return token;
        }
    }
}
