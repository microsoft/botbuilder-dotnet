using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    public static class ObjectPath
    {
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

        public static bool TryGetValue<T>(object o, string pathExpression, out T value)
        {
            value = default(T);

            if (pathExpression == null)
            {
                return false;
            }

            // if JPath expression
            JToken result = null;
            if (pathExpression.StartsWith("$."))
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

                if (val == null)
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
                else
                {
                    try
                    {
                        value = (T)Convert.ChangeType(val, typeof(T));
                        return true;
                    }
                    catch (Exception)
                    {
                        // we don't know what type it is?
                    }
                }
            }

            if (result != null)
            {
                value = result.ToObject<T>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Equivalent to javascripts ObjectPath.Assign, creates a new object from startObject overlaying any non-null values from the overlay object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startObject">intial object</param>
        /// <param name="overlayObject">overlay object</param>
        /// <returns>merged object</returns>
        public static T Merge<T>(T startObject, T overlayObject)
            where T : class
        {
            return Assign<T>(startObject, overlayObject);
        }


        /// <summary>
        /// Equivalent to javascripts ObjectPath.Assign, creates a new object from startObject overlaying any non-null values from the overlay object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startObject">intial object of any type</param>
        /// <param name="overlayObject">overlay object of any type</param>
        /// <returns>merged object</returns>
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

    }
}
