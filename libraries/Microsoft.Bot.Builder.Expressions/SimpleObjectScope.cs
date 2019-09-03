using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


        public object GetValue(string path)
        {
            var parts = path.Split('.');

            object value = null;
            string error = null;

            var curScope = scope;
            foreach (string part in parts)
            {
                (value, error) = AccessProperty(curScope, part);
                if (error != null)
                {
                    throw new Exception(error);
                }
                curScope = value;
            }
            return value;
        }

        public object SetValue(string path, object value)
        {
            return SetProperty(scope, path, value);
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
