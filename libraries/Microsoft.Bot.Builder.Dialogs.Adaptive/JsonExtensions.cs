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
    /// <summary>
    /// Implements JSON extensions.
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Removes first element of a queue.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="queue">List from where to remove first element.</param>
        /// <returns>Element removed.</returns>
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
        /// Replaces the binding paths in a JSON Token value with the evaluated results recursively. Returns the final JSON Token value.
        /// </summary>
        /// <param name="token">A JSON Token value which may have some binding paths.</param>
        /// <param name="state">A scope for looking up variables.</param>
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

        /// <summary>
        /// Evaluate ValueExpression according the value type.
        /// </summary>
        /// <param name="valExpr">Input ValueExpression.</param>
        /// <param name="state">A scope for looking up variables.</param>
        /// <returns>Deep data binding result.</returns>
        public static object EvaluateExpression(this ValueExpression valExpr, object state)
        {
            return valExpr.ExpressionText == null ?
                JToken.FromObject(valExpr.Value).DeepClone().ReplaceJTokenRecursively(state)
                : valExpr.GetValue(state);
        }
    }
}
