using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class ObjectPath
    {
        private static JsonSerializerSettings cloneSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        private static JsonSerializerSettings expressionCaseSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static bool HasValue(object obj, Expression pathExpression)
        {
            return TryGetValue<object>(obj, pathExpression, out var value);
        }

        public static T GetValue<T>(object obj, Expression pathExpression)
        {
            if (TryGetValue<T>(obj, pathExpression, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(pathExpression.ToString());
        }

        public static T GetValue<T>(object obj, Expression pathExpression, T defaultValue)
        {
            if (TryGetValue<T>(obj, pathExpression, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static bool TryGetValue<T>(object o, Expression expression, out T value)
        {
            value = default(T);

            if (expression == null)
            {
                return false;
            }

            // normal expression
            var (val, error) = expression.TryEvaluate(o);
            if (error != null)
            {
                return false;
            }

            if (val == null)
            {
                return false;
            }

            value = MapTo<T>(val);
            return true;
        }

        public static bool HasValue(object obj, string pathExpression)
        {
            return TryGetValue<object>(obj, pathExpression, out var value);
        }

        public static T GetValue<T>(object obj, string pathExpression)
        {
            if (TryGetValue<T>(obj, pathExpression, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(pathExpression);
        }

        public static T GetValue<T>(object obj, string pathExpression, T defaultValue)
        {
            if (TryGetValue<T>(obj, pathExpression, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static bool TryGetValue<T>(object obj, string pathExpression, out T value)
        {
            value = default(T);

            if (obj == null)
            {
                return false;
            }

            if (pathExpression == null)
            {
                return false;
            }

            if (pathExpression == string.Empty)
            {
                value = MapTo<T>(obj);
                return true;
            }

            // if JPath expression
            JToken result = null;
            if (pathExpression.StartsWith("$."))
            {
                // jpath
                if (obj != null && obj.GetType() == typeof(JArray))
                {
                    int index = 0;
                    if (int.TryParse(pathExpression, out index) && index < JArray.FromObject(obj).Count)
                    {
                        result = JArray.FromObject(obj)[index];
                    }
                }
                else if (obj != null && obj is JObject)
                {
                    result = ((JObject)obj).SelectToken(pathExpression);
                }
                else
                {
                    result = JToken.FromObject(obj).SelectToken(pathExpression);
                }
            }
            else
            {
                // normal expression
                var exp = new ExpressionEngine().Parse(pathExpression);
                return TryGetValue<T>(obj, exp, out value);
            }

            return false;
        }

        public static string SetValue(object o, string pathExpression, object value)
        {
            return SetValue(o, new ExpressionEngine().Parse(pathExpression), value);
        }

        public static string SetValue(object o, Expression pathExpression, object value, bool json = true)
        {
            object val;
            if (json)
            {
                if (value is JToken || value is JObject || value is JArray)
                {
                    val = (JToken)value;
                }
                else if (value == null)
                {
                    val = null;
                }
                else if (value is string || value is byte || value is bool ||
                        value is short || value is int || value is long ||
                        value is ushort || value is uint || value is ulong ||
                        value is decimal || value is float || value is double)
                {
                    val = JValue.FromObject(value);
                }
                else
                {
                    val = (JToken)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, expressionCaseSettings));
                }
            }
            else
            {
                val = value;
            }

            var (result, error) = Expression.SetPathToValue(pathExpression, val).TryEvaluate(o);
            return error;
        }

        public static void RemoveProperty(object o, string pathExpression)
        {
            // TODO-- use expression library to resolve pathexpression. 

            // If the json path does not exist
            string[] segments = pathExpression.Split('.').Select(segment => segment.ToLower()).ToArray();
            dynamic current = o;

            var deleted = false;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                if (current is IDictionary<string, object> curDict)
                {
                    if (!curDict.TryGetValue(segment, out var segVal) || segVal == null)
                    {
                        deleted = true;
                        break;
                    }

                    current = curDict[segment];
                }
            }

            if (!deleted)
            {
                current.Remove(segments.Last());
            }
        }

        public static void RemoveProperty(object o, Expression pathExpression)
        {
            // TODO-- use expression library to resolve pathexpression.
            RemoveProperty(o, pathExpression.ToString());
        }

        /// <summary>
        /// Clone an object.
        /// </summary>
        /// <typeparam name="T">Type to clone.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>The object as Json.</returns>
        public static T Clone<T>(T obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, ObjectPath.cloneSettings), ObjectPath.cloneSettings);
        }

        /// <summary>
        /// Equivalent to javascripts ObjectPath.Assign, creates a new object from startObject overlaying any non-null values from the overlay object.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="startObject">Intial object.</param>
        /// <param name="overlayObject">Overlay object.</param>
        /// <returns>merged object.</returns>
        public static T Merge<T>(T startObject, T overlayObject)
            where T : class
        {
            return Assign<T>(startObject, overlayObject);
        }

        /// <summary>
        /// Equivalent to javascripts ObjectPath.Assign, creates a new object from startObject overlaying any non-null values from the overlay object.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="startObject">intial object of any type.</param>
        /// <param name="overlayObject">overlay object of any type.</param>
        /// <returns>merged object.</returns>
        public static T Assign<T>(object startObject, object overlayObject)
            where T : class
        {
            return (T)Assign(startObject, overlayObject, typeof(T));
        }

        public static object Assign(object startObject, object overlayObject, Type type)
        {
            if (startObject != null && overlayObject != null)
            {
                // make a deep clone JObject of the startObject
                JObject jsMerged = (startObject is JObject) ? (JObject)(startObject as JObject).DeepClone() : JObject.FromObject(startObject);

                // get a JObject of the overlay object
                JObject jsOverlay = (overlayObject is JObject) ? (overlayObject as JObject) : JObject.FromObject(overlayObject);

                jsMerged.Merge(jsOverlay, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                });

                return jsMerged.ToObject(type);
            }

            var singleObject = startObject ?? overlayObject;
            if (singleObject != null)
            {
                if (singleObject is JObject)
                {
                    return (singleObject as JObject).ToObject(type);
                }

                return singleObject;
            }

            return (Type)Activator.CreateInstance(type);
        }

        private static T MapTo<T>(object val)
        {
            if (val is T)
            {
                return (T)val;
            }
            else if (typeof(T) == typeof(JArray))
            {
                return (T)(object)JArray.FromObject(val);
            }
            else if (typeof(T) == typeof(JObject))
            {
                return (T)(object)JObject.FromObject(val);
            }
            else if (typeof(T) == typeof(JToken))
            {
                return (T)(object)JToken.FromObject(val);
            }
            else if (val is JToken)
            {
                return ((JToken)val).ToObject<T>();
            }
            else
            {
                return (T)Convert.ChangeType(val, typeof(T));
            }
        }
    }
}
