using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
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

        public static bool HasValue(object obj, string pathExpression)
        {
            return TryGetPathValue<object>(obj, pathExpression, out var value);
        }

        public static T GetPathValue<T>(object obj, string pathExpression)
        {
            if (TryGetPathValue<T>(obj, pathExpression, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(pathExpression);
        }

        public static T GetPathValue<T>(object obj, string path, T defaultValue)
        {
            if (TryGetPathValue<T>(obj, path, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static bool TryGetPathValue<T>(object obj, string pathExpression, out T value)
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
                value = MapValueTo<T>(obj);
                return true;
            }

            string[] segments = pathExpression.Split('.').Select(segment => segment.ToLower()).ToArray();
            dynamic current = obj;
            for (var i = 0; i < segments.Length; i++)
            {
                current = ResolveSegment(current, segments[i]);
                if (current == null)
                {
                    return false;
                }
            }

            value = MapValueTo<T>(current);
            return true;
        }

        public static void SetPathValue(object o, string pathExpression, object value, bool json = true)
        {
            string[] segments = pathExpression.Split('.').Select(segment => segment.ToLower()).ToArray();
            dynamic current = o;

            for (var i = 0; i < segments.Length - 1; i++)
            {
                dynamic next = ResolveSegment(current, segments[i], addMissing: true);
                current = next;
            }

            SetObjectProperty(current, segments.Last(), value);
        }

        public static void RemovePathValue(object o, string pathExpression)
        {
            string[] segments = pathExpression.Split('.').Select(segment => segment.ToLower()).ToArray();
            dynamic next = o;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                next = ResolveSegment(next, segments[i], addMissing: true);
            }

            if (next != null)
            {
                var segment = segments.Last();
                int iIndexerStart = segment.IndexOf('[');
                if (iIndexerStart > 0)
                {
                    var index = int.Parse(segment.Substring(iIndexerStart + 1).TrimEnd(']'));
                    segment = segment.Substring(0, iIndexerStart);
                    next = ObjectPath.GetObjectProperty(next, segment);
                    next[index] = null;
                }
                else
                {
                    try
                    {
                        next.Remove(segment);
                    }
                    catch (Exception)
                    {
                        ObjectPath.SetObjectProperty(next, segment, null);
                    }
                }
            }
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

        private static T MapValueTo<T>(object val)
        {
            if (val is JValue)
            {
                return ((JValue)val).ToObject<T>();
            }
            else if (val is JArray)
            {
                return ((JArray)val).ToObject<T>();
            }
            else if (val is JObject)
            {
                return ((JObject)val).ToObject<T>();
            }
            else if (typeof(T) == typeof(JObject))
            {
                return (T)(object)JObject.FromObject(val);
            }
            else if (typeof(T) == typeof(JArray))
            {
                return (T)(object)JArray.FromObject(val);
            }
            else if (typeof(T) == typeof(JValue))
            {
                return (T)(object)JValue.FromObject(val);
            }
            else if (val is T)
            {
                return (T)val;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(val, expressionCaseSettings));
            }
        }

        private static object GetObjectProperty(object obj, string property)
        {
            if (obj is IDictionary<string, object> dict)
            {
                var key = dict.Keys.Where(k => k.ToLower() == property.ToLower()).FirstOrDefault();
                if (key != null && dict.TryGetValue(key, out var value))
                {
                    return value;
                }

                return null;
            }

            if (obj is JObject jobj)
            {
                jobj.TryGetValue(property, StringComparison.InvariantCultureIgnoreCase, out var value);
                return value;
            }

            var prop = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == property.ToLower()).FirstOrDefault();
            if (prop != null)
            {
                return prop.GetValue(obj);
            }

            return null;
        }

        private static void SetObjectProperty(object obj, string property, object value, bool json = true)
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

            int iIndexerStart = property.IndexOf('[');
            if (iIndexerStart > 0)
            {
                var index = int.Parse(property.Substring(iIndexerStart + 1).TrimEnd(']'));
                property = property.Substring(0, iIndexerStart);

                dynamic array = GetObjectProperty(obj, property);
                if (array == null)
                {
                    SetObjectProperty(obj, property, new JArray());
                    array = GetObjectProperty(obj, property);
                }

                // expand nodes
                for (int i = ((ICollection)array).Count; i <= index; i++)
                {
                    array.Add(null);
                }

                array[index] = JToken.FromObject(val);
            }
            else
            {
                if (obj is IDictionary<string, object> dict)
                {
                    dict[property] = val;
                    return;
                }

                if (obj is JObject jobj)
                {
                    jobj[property] = (val != null) ? JToken.FromObject(val) : null;
                    return;
                }

                var prop = obj.GetType().GetProperty(property);
                if (prop != null)
                {
                    prop.SetValue(obj, val);
                }
            }
        }

        private static void RemoveObjectProperty(object obj, string property)
        {
            if (obj is IDictionary<string, object> dict)
            {
                var key = dict.Keys.Where(k => k.ToLower() == property.ToLower()).FirstOrDefault();
                if (key != null)
                {
                    dict.Remove(key);
                }

                return;
            }

            if (obj is JObject jobj)
            {
                var key = jobj.Properties().Where(p => p.Name.ToLower() == property.ToLower()).FirstOrDefault();
                if (key != null)
                {
                    jobj.Remove(key.Name);
                }

                return;
            }

            var prop = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == property.ToLower()).FirstOrDefault();
            if (prop != null)
            {
                try
                {
                    prop.SetValue(obj, null);
                }
                catch (Exception)
                {
                }
            }
        }

        private static dynamic ResolveSegment(dynamic node, string segment, bool addMissing = false)
        {
            dynamic next;
            int iIndexerStart = segment.IndexOf('[');
            if (iIndexerStart > 0)
            {
                var index = int.Parse(segment.Substring(iIndexerStart + 1).TrimEnd(']'));
                segment = segment.Substring(0, iIndexerStart);

                next = GetObjectProperty(node, segment);
                if (next == null)
                {
                    // then no array
                    if (addMissing)
                    {
                        var missing = new JArray();
                        SetObjectProperty(node, segment, missing);
                        next = GetObjectProperty(node, segment);
                    }
                    else
                    {
                        return null;
                    }
                }

                if (((ICollection)next).Count <= index)
                {
                    // then array is too small
                    if (addMissing)
                    {
                        // expand nodes
                        for (int i = ((ICollection)next).Count; i <= index; i++)
                        {
                            ((JArray)next)[i] = null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                next = next[index];
            }
            else
            {
                next = GetObjectProperty(node, segment);
                if (next == null)
                {
                    if (addMissing)
                    {
                        SetObjectProperty(node, segment, new JObject());
                        next = GetObjectProperty(node, segment);
                    }
                }
            }

            return next;
        }
    }
}
