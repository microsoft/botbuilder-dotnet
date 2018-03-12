// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// DynamicObject which seemlessly blends dictionary properties with real properties
    /// </summary>
    public class FlexObject : DynamicObject, ICloneable
    {
        public static JsonSerializerSettings SerializationSettings = new JsonSerializerSettings()
        {
            // we use all so that we get typed roundtrip out of storage, but we don't use validation because we don't know what types are valid
            TypeNameHandling = TypeNameHandling.All
        };

        [JsonExtensionData]
        private Dictionary<string, object> dynamicProperties { get; set; } = new Dictionary<string, object>();

        /// <remarks>
        /// If the JsonNameAttribute is used on the method name, that "revised" name is stored here
        /// so that serialization happens correctly. It's important in that case to serialize the "revised"
        /// name ("@id"), yet maintain the original name ("id") so the value can be retrieved. 
        /// 
        /// The primary scenario here is serialization to JSON. 
        /// </remarks>
        private Dictionary<string, string> _objectProperties = null;

        protected IDictionary<string, string> objectProperties
        {
            get
            {
                if (_objectProperties == null)
                {
                    lock (dynamicProperties)
                    {
                        if (_objectProperties == null)
                        {
                            Dictionary<string, string> temp = new Dictionary<string, string>();
                            foreach (var pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                            {
                                if (pi.Name == "Item")
                                    continue;

                                var customJsonAttributes = pi.GetCustomAttributes<JsonPropertyAttribute>();

                                // If there is a customJsonAttribute, then use the user
                                // specified name as the method name. This is what will be serialized
                                // out to JSON ("@id").
                                // If the Json override is not present, then the actual property
                                // name is used ("id").                                  
                                var attributeOrNull = customJsonAttributes.FirstOrDefault();
                                if (attributeOrNull != null)
                                    temp[attributeOrNull.PropertyName] = pi.Name;
                                else
                                    temp[pi.Name] = null;
                            }
                            _objectProperties = temp;
                        }
                    }
                }
                return _objectProperties;
            }
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var p in this.objectProperties)
                yield return p.Key;
            foreach (var key in this.dynamicProperties.Keys.Where(key => !this.objectProperties.ContainsKey(key)))
                yield return key;
            yield break;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return base.TryDeleteMember(binder);
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetMember(binder.Name, value);
        }
        public dynamic this[string name]
        {
            get
            {
                this.TryGetMember(name, out object value);
                return value;
            }
            set { this.TrySetMember(name, value); }
        }

        private bool TryGetMember(string name, out object result)
        {
            if (this.dynamicProperties.ContainsKey(name))
            {
                result = this.dynamicProperties[name];
                return true;
            }
            else if (this.objectProperties.ContainsKey(name))
            {
                if (this.objectProperties[name] == null)
                {
                    var prop = this.GetType().GetTypeInfo().GetDeclaredProperty(name);
                    result = prop.GetValue(this);
                }
                else
                {
                    string actualPropertyName = this.objectProperties[name];
                    var prop = this.GetType().GetTypeInfo().GetDeclaredProperty(actualPropertyName);
                    result = prop.GetValue(this);
                }
                return true;
            }

            result = null;
            return true;
        }

        private bool TrySetMember(string name, object value)
        {
            var prop = this.GetType().GetTypeInfo().GetDeclaredProperty(name);
            if (prop != null)
            {
                prop.SetValue(this, value);
                return true;
            }
            dynamicProperties[name] = value;
            return true;
        }

        public void Add(string key, object value)
        {
            this.TrySetMember(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.GetDynamicMemberNames().Contains(key);
        }

        public bool Remove(string key)
        {
            var prop = this.GetType().GetTypeInfo().GetDeclaredProperty(key);
            if (prop != null)
            {
                prop.SetValue(this, null);
                return true;
            }
            return this.dynamicProperties.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.TryGetMember(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.TrySetMember(item.Key, item.Value);
        }

        public void Clear()
        {
            this.dynamicProperties.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.dynamicProperties.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this.dynamicProperties.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var x in this.dynamicProperties)
                yield return x;
            foreach (var y in this.objectProperties.Keys)
                yield return new KeyValuePair<string, object>(y, ((dynamic)this)[y]);
        }

        public object Clone()
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this, SerializationSettings),SerializationSettings);
        }

        public static T Clone<T>(object obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, SerializationSettings), SerializationSettings);
        }

        public static object Clone(object obj)
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, SerializationSettings), SerializationSettings);
        }
    }
}
