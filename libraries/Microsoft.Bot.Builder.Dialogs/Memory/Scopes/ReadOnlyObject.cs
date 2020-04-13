#pragma warning disable SA1201 // Elements should appear in the correct order

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// ReadOnlyObject is a wrapper around any object to prevent setting of properties on the object.
    /// </summary>
    /// <remarks>
    /// Any complex objects that are returned from this are further wrapped in an ReadOnlyObject, so that you can
    /// get ExpressionProperty binding for properties in a complex hierarchy of objects.
    /// </remarks>
    internal class ReadOnlyObject : IDictionary<string, object>
    {
        private const string NOTSUPPORTED = "This object is readonly.";
        private readonly object obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObject"/> class.
        /// </summary>
        /// <param name="dialogContext">dialog context for evalutation of expression properties.</param>
        /// <param name="obj">object to wrap.  Any expression properties on it will be evaluated using the dc.</param>
        public ReadOnlyObject(object obj)
        {
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
            if (other is ReadOnlyObject esp)
            {
                return this.obj.Equals(esp.obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.obj.GetHashCode();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(NOTSUPPORTED);
        }

        public bool ContainsKey(string key)
        {
            return ObjectPath.GetProperties(this.obj).Any(propertyName => propertyName.Equals(key, StringComparison.InvariantCultureIgnoreCase));
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
            return ObjectPath.TryGetPathValue<object>(this.obj, name, out value);
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
                return new ReadOnlyObject(val);
            }

            return null;
        }
    }
}
