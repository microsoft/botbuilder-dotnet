using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// DynamicObject which seemlessly blends dictionary properties with real properties
    /// </summary>
    public class FlexObject : DynamicObject, ICloneable
    {
        private Dictionary<string, object> dynamicProperties { get; set; } = new Dictionary<string, object>();
        private string[] _objectProperties = null;

        protected string[] objectProperties
        {
            get
            {
                if (_objectProperties == null)
                {
                    lock (dynamicProperties)
                    {
                        if (_objectProperties == null)
                            _objectProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(pi => pi.Name != "Item")
                                   .Select(pi => pi.Name)
                                   .ToArray();
                    }
                }
                return _objectProperties;
            }
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var p in this.objectProperties)
                yield return p;
            foreach (var key in this.dynamicProperties.Keys.Where(key => !this.objectProperties.Contains(key)))
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
                object value = null;
                this.TryGetMember(name, out value);
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
            else if (this.objectProperties.Contains(name))
            {
                var prop = this.GetType().GetTypeInfo().GetDeclaredProperty(name);
                result = prop.GetValue(this);
                return true;
            }

            result = null;
            return false;
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
            foreach (var y in this.objectProperties)
                yield return new KeyValuePair<string, object>(y, ((dynamic)this)[y]);
        }

        public object Clone()
        {
            return JsonConvert.DeserializeObject<StoreItem>(JsonConvert.SerializeObject(this));
        }
    }
}
