using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder
{
    public class DynamicContext : DynamicObject
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
                            _objectProperties = this.GetType().GetTypeInfo().DeclaredProperties
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
            foreach (var key in this.dynamicProperties.Keys)
                yield return key;
            yield break;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetMember(binder.Name, value);
        }

        public dynamic this[string name]
        {
            get
            {
                object value;
                this.TryGetMember(name, out value);
                return value;
            }
            set { this.dynamicProperties[name] = value; }
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
    }
}
