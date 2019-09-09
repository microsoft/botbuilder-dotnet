using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    internal class SimpleObjectScope : IMemoryScope
    {
        private object scope = null;

        public SimpleObjectScope(object scope)
        {
            this.scope = scope;
        }

        public (object value, string error) GetValue(string path)
        {
            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            object value = null;
            var curScope = scope;

            foreach (string part in parts)
            {
                string error = null;
                if (int.TryParse(part, out var idx))
                {
                    (value, error) = AccessIndex(curScope, idx);
                }
                else
                {
                    (value, error) = AccessProperty(curScope, part);
                }

                if (error != null)
                {
                    return (null, error);
                }
                curScope = value;
            }
            return (value, null);
        }

        // In this simple object scope, we don't allow you to set a path in which some parts in middle don't exist
        // for example
        // if you set dialog.a.b = x, but dialog.a don't exist, this will result in an error
        // because we can't and shouldn't smart create structure in the middle
        // you can implement a customzied Scope that support such behavior
        public (object value, string error) SetValue(string path, object value)
        {
            var parts = path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var curScope = scope;
            string error = null;

            // find the 2nd last value, ie, the container
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (int.TryParse(parts[i], out var index))
                {
                    (curScope, error) = AccessIndex(curScope, index);
                }
                else
                {
                    (curScope, error) = AccessProperty(curScope, parts[i]);
                }

                if (error != null)
                {
                    return (null, error);
                }
            }

            if (curScope == null)
            {
                return (null, $"Some parts in the middle of path doesn't exist: {path}");
            }

            // set the last value
            if (int.TryParse(parts.Last(), out var idx))
            {
                if (TryParseList(curScope, out var li))
                {
                    if (li is JArray)
                    {
                        value = JToken.FromObject(value);
                    }

                    if (idx > li.Count)
                    {
                        error = $"{idx} index out of range";
                    }
                    else if (idx == li.Count)
                    {
                        // expand for one
                        li.Add(value);
                    }
                    else
                    {
                        li[idx] = value;
                    }
                }
                else
                {
                    error = $"set value for an index to a non-list object";
                }

                if (error != null)
                {
                    return (null, error);
                }
            }
            else
            {
                SetProperty(curScope, parts.Last(), value);
            }

            return (ResolveValue(value), null);
        }

        private (object value, string error) AccessProperty(object instance, string property)
        {
            // NOTE: This returns null rather than an error if property is not present
            if (instance == null)
            {
                return (null, null);
            }

            object value = null;
            string error = null;
            property = property.ToLower();

            // NOTE: what about other type of TKey, TValue?
            if (instance is IDictionary<string, object> idict)
            {
                if (!idict.TryGetValue(property, out value))
                {
                    // fall back to case insensitive
                    var prop = idict.Keys.Where(k => k.ToLower() == property).SingleOrDefault();
                    if (prop != null)
                    {
                        idict.TryGetValue(prop, out value);
                    }
                }
            }
            else if (instance is IDictionary dict)
            {
                foreach (var p in dict.Keys)
                {
                    value = dict[property];
                }
            }
            else if (instance is JObject jobj)
            {
                value = jobj.GetValue(property, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                // Use reflection
                var type = instance.GetType();
                var prop = type.GetProperties().Where(p => p.Name.ToLower() == property).SingleOrDefault();
                if (prop != null)
                {
                    value = prop.GetValue(instance);
                }
            }

            value = ResolveValue(value);

            return (value, error);
        }

        private (object value, string error) AccessIndex(object instance, int index)
        {
            // NOTE: This returns null rather than an error if property is not present
            if (instance == null)
            {
                return (null, null);
            }

            object value = null;
            string error = null;

            var count = -1;
            if (TryParseList(instance, out var list))
            {
                count = list.Count;
            }
            var itype = instance.GetType();
            var indexer = itype.GetProperties().Except(itype.GetDefaultMembers().OfType<PropertyInfo>());
            if (count != -1 && indexer != null)
            {
                if (index >= 0 && count > index)
                {
                    dynamic idyn = instance;
                    value = idyn[index];
                }
                else
                {
                    error = $"{index} is out of range for ${instance}";
                }
            }
            else
            {
                error = $"{instance} is not a collection.";
            }

            value = ResolveValue(value);

            return (value, error);
        }

        private bool TryParseList(object value, out IList list)
        {
            var isList = false;
            list = null;
            if (!(value is JObject) && value is IList listValue)
            {
                list = listValue;
                isList = true;
            }
            return isList;
        }

        private object ResolveValue(object obj)
        {
            object value;
            if (!(obj is JValue jval))
            {
                value = obj;
            }
            else
            {
                value = jval.Value;
                if (jval.Type == JTokenType.Integer)
                {
                    value = jval.ToObject<int>();
                }
                else if (jval.Type == JTokenType.String)
                {
                    value = jval.ToObject<string>();
                }
                else if (jval.Type == JTokenType.Boolean)
                {
                    value = jval.ToObject<bool>();
                }
                else if (jval.Type == JTokenType.Float)
                {
                    value = jval.ToObject<float>();
                }
            }
            return value;
        }

        private object SetProperty(object instance, string property, object value)
        {
            object result = value;

            if (instance is IDictionary<string, object> idict)
            {
                idict[property] = value;
            }
            else if (instance is IDictionary dict)
            {
                dict[property] = value;
            }
            else if (instance is JObject jobj)
            {
                result = JToken.FromObject(value);
                jobj[property] = (JToken)result;
            }
            else
            {
                // Use reflection
                var type = instance.GetType();
                var prop = type.GetProperties().Where(p => p.Name.ToLower() == property).SingleOrDefault();
                if (prop != null)
                {
                    prop.SetValue(instance, value);
                }
            }
            return result;
        }
    }
}
