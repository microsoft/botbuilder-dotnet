using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Helper methods for working with dynamic json objects.
    /// </summary>
    public static class ObjectPath
    {
        private const string SingleQuote = "\'";
        private const string DoubleQuote = "\"";
        private static JsonSerializerSettings cloneSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        private static JsonSerializerSettings expressionCaseSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Does an object have a subpath.
        /// </summary>
        /// <param name="obj">object.</param>
        /// <param name="path">path to evaluate.</param>
        /// <returns>true if the path is there.</returns>
        public static bool HasValue(object obj, string path)
        {
            return TryGetPathValue<object>(obj, path, out var value);
        }

        /// <summary>
        /// Get the value for a path relative to an object.
        /// </summary>
        /// <typeparam name="T">type to return.</typeparam>
        /// <param name="obj">object to start with.</param>
        /// <param name="path">path to evaluate.</param>
        /// <returns>value or default(T).</returns>
        public static T GetPathValue<T>(object obj, string path)
        {
            if (TryGetPathValue<T>(obj, path, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(path);
        }

        /// <summary>
        /// Get the value for a path relative to an object.
        /// </summary>
        /// <typeparam name="T">type to return.</typeparam>
        /// <param name="obj">object to start with.</param>
        /// <param name="path">path to evaluate.</param>
        /// <param name="defaultValue">default value to use if any part of the path is missing.</param>
        /// <returns>value or default(T).</returns>
        public static T GetPathValue<T>(object obj, string path, T defaultValue)
        {
            if (TryGetPathValue<T>(obj, path, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the value for a path relative to an object.
        /// </summary>
        /// <typeparam name="T">type to return.</typeparam>
        /// <param name="obj">object to start with.</param>
        /// <param name="path">path to evaluate.</param>
        /// <param name="value">value for the path.</param>
        /// <returns>true if successful.</returns>
        public static bool TryGetPathValue<T>(object obj, string path, out T value)
        {
            value = default(T);

            if (obj == null)
            {
                return false;
            }

            if (path == null)
            {
                return false;
            }

            if (path == string.Empty)
            {
                value = MapValueTo<T>(obj);
                return true;
            }

            if (!TryResolveBracketValues(obj, ref path))
            {
                return false;
            }

            // at this point we have clean dotted path with numerical array indexers: user[user.name][user.age] ==> user['tom'][52]
            dynamic current = obj;
            var segments = SplitSegments(path).ToArray();
            for (int i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                var nextSegment = segments[i + 1];

                current = ResolveSegment(current, segment, nextSegment);
                if (current == null)
                {
                    return false;
                }
            }

            current = ResolveSegment(current, segments.Last(), null);
            if (current == null)
            {
                return false;
            }

            value = MapValueTo<T>(current);
            return true;
        }

        /// <summary>
        /// Given an object evaluate a path to set the value.
        /// </summary>
        /// <param name="obj">object to start with.</param>
        /// <param name="path">path to evaluate.</param>
        /// <param name="value">value to store.</param>
        /// <param name="json">if true, sets the value as primitive JSON objects.</param>
        public static void SetPathValue(object obj, string path, object value, bool json = true)
        {
            if (!TryResolveBracketValues(obj, ref path))
            {
                return;
            }

            string[] segments = SplitSegments(path).ToArray();
            dynamic current = obj;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                var nextSegment = segments[i + 1];
                current = ResolveSegment(current, segment, nextSegment, addMissing: true);
            }

            var lastSegment = segments.Last();
            SetObjectProperty(current, lastSegment, value);
        }

        public static void RemovePathValue(object obj, string path)
        {
            if (!TryResolveBracketValues(obj, ref path))
            {
                return;
            }

            string[] segments = SplitSegments(path).ToArray();
            dynamic next = obj;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                var nextSegment = segments[i + 1];
                next = ResolveSegment(next, segment, nextSegment);

                if (next == null)
                {
                    return;
                }
            }

            if (next != null)
            {
                var lastSegment = segments.Last();
                if (IsArraySegment(lastSegment))
                {
                    var indexArgs = GetIndexArg(lastSegment);
                    if (int.TryParse(indexArgs, out int index))
                    {
                        next[index] = null;
                        return;
                    }
                    else
                    {
                        try
                        {
                            next.Remove(indexArgs);
                        }
                        catch (Exception)
                        {
                            ObjectPath.SetObjectProperty(next, indexArgs, null);
                        }
                    }
                }
                else
                {
                    try
                    {
                        next.Remove(lastSegment);
                    }
                    catch (Exception)
                    {
                        ObjectPath.SetObjectProperty(next, lastSegment, null);
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

        /// <summary>
        /// Equivalent to javascripts ObjectPath.Assign, creates a new object from startObject overlaying any non-null values from the overlay object.
        /// </summary>
        /// <param name="startObject">intial object of any type.</param>
        /// <param name="overlayObject">overlay object of any type.</param>
        /// <param name="type">type to output.</param>
        /// <returns>merged object.</returns>
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

        /// <summary>
        /// Convert a generic object to a typed object.
        /// </summary>
        /// <typeparam name="T">type to convert to.</typeparam>
        /// <param name="val">value to convert.</param>
        /// <returns>converted value.</returns>
        public static T MapValueTo<T>(object val)
        {
            if (val is JValue)
            {
                return ((JValue)val).ToObject<T>();
            }
            else if (typeof(T) == typeof(object))
            {
                return (T)(object)val;
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

        /// <summary>
        /// Get a property or array element from an object.
        /// </summary>
        /// <param name="obj">object.</param>
        /// <param name="property">property or array segment to get relative to the object.</param>
        /// <returns>the value or null if not found.</returns>
        private static object GetObjectProperty(object obj, string property)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is IDictionary<string, object> dict)
            {
                var key = dict.Keys.Where(k => k.ToLower() == property.ToLower()).FirstOrDefault() ?? property;
                if (dict.TryGetValue(key, out var value))
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

            if (obj is JValue jval)
            {
                // in order to make things like "this.value.Length" work, when "this.value" is a string.
                return GetObjectProperty(jval.Value, property);
            }

            var prop = obj.GetType().GetProperties().Where(p => p.Name.ToLower() == property.ToLower()).FirstOrDefault();
            if (prop != null)
            {
                return prop.GetValue(obj);
            }

            return null;
        }

        /// <summary>
        /// Given an object, set a property or array element on it with a value.
        /// </summary>
        /// <param name="obj">object to modify.</param>
        /// <param name="property">property or array segment to put the value in.</param>
        /// <param name="value">value to store.</param>
        /// <param name="json">if true, value will be normalized to JSON primitive objects.</param>
        private static void SetObjectProperty(object obj, string property, object value, bool json = true)
        {
            object val;

            val = GetNormalizedValue(value, json);

            if (IsArraySegment(property))
            {
                property = GetIndexArg(property);
                if (int.TryParse(property, out int index))
                {
                    var jar = obj as JArray;
                    for (int i = jar.Count; i <= index; i++)
                    {
                        jar.Add(null);
                    }

                    jar[index] = JToken.FromObject(val);
                    return;
                }
            }

            if (obj is IDictionary<string, object> dict)
            {
                var key = dict.Keys.Where(k => k.ToLower() == property.ToLower()).FirstOrDefault() ?? property;
                dict[key] = val;
                return;
            }

            if (obj is JObject jobj)
            {
                var key = jobj.Properties().Where(p => p.Name.ToLower() == property.ToLower()).FirstOrDefault()?.Name ?? property;
                jobj[key] = (val != null) ? JToken.FromObject(val) : null;
                return;
            }

            var prop = obj.GetType().GetProperty(property);
            if (prop != null)
            {
                prop.SetValue(obj, val);
            }
        }

        /// <summary>
        /// Normalize value as json objects.
        /// </summary>
        /// <param name="value">value to normalize.</param>
        /// <param name="json">normalize as json objects.</param>
        /// <returns>normalized value.</returns>
        private static object GetNormalizedValue(object value, bool json)
        {
            object val;
            if (json)
            {
                if (value is JToken || value is JObject || value is JArray)
                {
                    val = Clone((JToken)value);
                }
                else if (value == null)
                {
                    val = null;
                }
                else if (value is string || value is byte || value is bool ||
                       value is DateTime || value is DateTimeOffset ||
                       value is short || value is int || value is long ||
                       value is ushort || value is uint || value is ulong ||
                       value is decimal || value is float || value is double)
                {
                    val = JValue.FromObject(value);
                }
                else
                {
                    val = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, expressionCaseSettings));
                }
            }
            else
            {
                val = value;
            }

            return val;
        }

        /// <summary>
        /// Given an object and a property segment, remove the property segment from the object.
        /// </summary>
        /// <param name="obj">object.</param>
        /// <param name="property">property or arraysegment.</param>
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

        /// <summary>
        /// Is the segment an array segment [xxxx].
        /// </summary>
        /// <param name="segment">segment.</param>
        /// <returns>true if it has [].</returns>
        private static bool IsArraySegment(string segment)
        {
            return segment.StartsWith("[") && segment.EndsWith("]");
        }

        /// <summary>
        /// Get the indexArg from an array segment ['foo'] => foo, [0] => 0.
        /// </summary>
        /// <param name="segment">segment.</param>
        /// <returns>normalized array argument as a string.</returns>
        private static string GetIndexArg(string segment)
        {
            return segment.TrimStart('[').TrimEnd(']').Trim('\'', '\"');
        }

        /// <summary>
        /// Given a node and a segment (and nextSegment if we are adding mising elemets) return the subproperty/element.
        /// </summary>
        /// <param name="node">current node.</param>
        /// <param name="segment">oath segment.</param>
        /// <param name="nextSegment">next segment (so we can initialize with JArray or JObject appropriately).</param>
        /// <param name="addMissing">if true, missing path members will be initialized appropriately.</param>
        /// <returns>leaf node.</returns>
        private static dynamic ResolveSegment(dynamic node, string segment, string nextSegment, bool addMissing = false)
        {
            // if it is a [0] or a ['string'] or a ["string"]
            if (IsArraySegment(segment))
            {
                var indexArg = GetIndexArg(segment);

                if (int.TryParse(indexArg, out int index))
                {
                    if (((ICollection)node).Count <= index)
                    {
                        // then array is too small
                        if (addMissing)
                        {
                            // expand nodes
                            for (int i = ((ICollection)node).Count; i <= index; i++)
                            {
                                ((JArray)node)[i] = null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }

                    // return x[0]
                    return node[index];
                }
                else
                {
                    // return x['string']
                    return GetObjectProperty(node, indexArg);
                }
            }
            else
            {
                dynamic next = GetObjectProperty(node, segment);
                if (next == null)
                {
                    if (addMissing)
                    {
                        if (IsArraySegment(nextSegment))
                        {
                            var indexArg = GetIndexArg(nextSegment);
                            if (int.TryParse(indexArg, out int index))
                            {
                                SetObjectProperty(node, segment, new JArray());
                                return GetObjectProperty(node, segment);
                            }
                        }

                        SetObjectProperty(node, segment, new JObject());
                        return GetObjectProperty(node, segment);
                    }
                }

                return next;
            }
        }

        /// <summary>
        /// Given a path this will enumerate paired brackets, which is used to do the SplitSegments 
        /// x[y[z]].blah[p] => "[y[z]]","[p]".
        /// </summary>
        /// <param name="path">path.</param>
        /// <returns>collection of bracketed content.</returns>
        private static IEnumerable<string> MatchBrackets(string path)
        {
            StringBuilder sb = new StringBuilder();
            int nest = 0;
            foreach (char ch in path)
            {
                if (ch == '[')
                {
                    nest++;
                }
                else if (ch == ']')
                {
                    nest--;
                }

                if (nest > 0)
                {
                    sb.Append(ch);
                }
                else if (sb.Length > 0)
                {
                    sb.Append(ch);
                    yield return sb.ToString();
                    sb.Clear();
                }
            }

            yield break;
        }

        /// <summary>
        /// Split path x.y.z[user.name][13] => "x","y","z","[user.name]","13".
        /// </summary>
        /// <param name="path">path to split.</param>
        /// <returns>split segments.</returns>
        private static IEnumerable<string> SplitSegments(string path)
        {
            StringBuilder sb = new StringBuilder();
            bool inBracket = false;
            foreach (char ch in path)
            {
                if (!inBracket)
                {
                    if (ch == '[')
                    {
                        yield return sb.ToString();
                        sb.Clear();
                        sb.Append(ch);
                        inBracket = true;
                    }
                    else if (ch == '.')
                    {
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                        }

                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else if (inBracket)
                {
                    if (ch == ']')
                    {
                        inBracket = false;
                        sb.Append(ch);
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        /// <summary>
        /// Given an root object and property path, resolve any nested bracket values.  conversation[user.name][user.age] => conversation['joe'][32].
        /// </summary>
        /// <param name="obj">root object.</param>
        /// <param name="propertyPath">property path to resolve.</param>
        /// <returns>true if it was able to resolve all nested references.</returns>
        private static bool TryResolveBracketValues(object obj, ref string propertyPath)
        {
            foreach (string bracket in MatchBrackets(propertyPath))
            {
                string bracketPath = bracket.Substring(1, bracket.Length - 2);

                // if it's not a number, or quoted string
                if (!int.TryParse(bracketPath, out int index) &&
                    !(bracketPath.StartsWith(SingleQuote) && bracketPath.EndsWith(SingleQuote)) &&
                    !(bracketPath.StartsWith(DoubleQuote) && bracketPath.EndsWith(DoubleQuote)))
                {
                    // then evaluate the path (NOTE: this is where nested [] will get resolved recursively)
                    if (TryGetPathValue<string>(obj, bracketPath, out string bracketValue))
                    {
                        if (int.TryParse(bracketValue, out index))
                        {
                            // if it's an intent we keep array syntax [#]
                            propertyPath = propertyPath.Replace(bracket, $"[{index}]");
                        }
                        else
                        {
                            // otherwise we replace with found property, meaning user[name] => user['tom']
                            propertyPath = propertyPath.Replace(bracket, $"['{bracketValue}']");
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
