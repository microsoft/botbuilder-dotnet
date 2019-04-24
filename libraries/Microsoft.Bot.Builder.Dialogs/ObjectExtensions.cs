using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class ObjectExtensions
    {
        public static bool HasValue<T>(this object obj, string pathExpression)
        {
            return obj.TryGetValue<T>(pathExpression, out var value);
        }

        public static T GetValue<T>(this object obj, string pathExpression)
        {
            if (obj.TryGetValue<T>(pathExpression, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(pathExpression);
        }

        public static bool TryGetValue<T>(this object o, string pathExpression, out T value)
        {
            value = default(T);

            if (pathExpression == null)
            {
                return false;
            }

            JToken result = null;
            if (pathExpression.StartsWith("$"))
            {
                // jpath
                if (o != null && o.GetType() == typeof(JArray))
                {
                    int index = 0;
                    if (int.TryParse(pathExpression, out index) && index < JArray.FromObject(o).Count)
                    {
                        result = JArray.FromObject(o)[index];
                    }
                }
                else if (o != null && o is JObject)
                {
                    result = ((JObject)o).SelectToken(pathExpression);
                }
                else
                {
                    result = JToken.FromObject(o).SelectToken(pathExpression);
                }
            }
            else
            {
                // normal expression
                var exp = new ExpressionEngine().Parse(pathExpression);
                var (val, error) = exp.TryEvaluate(o);
                if (error != null)
                {
                    return false;
                }

                if (val is JToken)
                {
                    result = (JToken)val;
                }
                else if (val is T)
                {
                    value = (T)val;
                    return true;
                }

            }

            if (result != null)
            {
                value = result.ToObject<T>();
                return true;
            }

            return false;
        }
    }
}
