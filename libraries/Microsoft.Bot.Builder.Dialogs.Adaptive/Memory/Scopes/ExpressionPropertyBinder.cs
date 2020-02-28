#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Properties;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    internal class ExpressionPropertyBinder : IDictionary<string, object>
    {
        private const string NOTSUPPORTED = "Changing dialog definitions at run time is not supported";
        private DialogContext dc;
        private object obj;

        public ExpressionPropertyBinder(DialogContext dc, object obj)
        {
            this.dc = dc;
            this.obj = obj;
        }

        public object this[string key] { get => GetValue(key); set => throw new NotSupportedException(NOTSUPPORTED); }

        public ICollection<string> Keys => ObjectPath.GetProperties(this.obj).ToList();

        public ICollection<object> Values => ObjectPath.GetProperties(this.obj).Select(propertyName => GetValue(propertyName)).ToList();

        public int Count => ObjectPath.GetProperties(this.obj).Count();

        public bool IsReadOnly => true;

        public void Add(string key, object value)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public void Clear()
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public override bool Equals(object other)
        {
            if (other is ExpressionPropertyBinder esp)
            {
                return this.obj.Equals(esp.obj);
            }

            return false;
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public bool ContainsKey(string key)
        {
            return ObjectPath.GetProperties(this.obj).Any(p => string.Compare(p, key, ignoreCase: true) == 0);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ObjectPath.GetProperties(this.obj).Select(propertyName => new KeyValuePair<string, object>(propertyName, GetValue(propertyName))).GetEnumerator();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public bool TryGetValue(string name, out object value)
        {
            if (ObjectPath.TryGetPathValue<object>(this.obj, name, out value))
            {
                if (value is IExpressionProperty ep)
                {
                    value = ep.GetObject(dc.GetState());
                    if (!value.GetType().IsValueType && !(value is string))
                    {
                        value = new ExpressionPropertyBinder(this.dc, value);
                    }
                }

                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private object GetValue(string name)
        {
            object val;
            if (TryGetValue(name, out val))
            {
                return val;
            }

            return null;
        }
    }
}
