// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        private const string Notsupported = "This object is readonly.";
        private readonly object _obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObject"/> class.
        /// </summary>
        /// <param name="obj">object to wrap.  Any expression properties on it will be evaluated using the dc.</param>
        public ReadOnlyObject(object obj)
        {
            _obj = obj;
        }

        public ICollection<string> Keys => ObjectPath.GetProperties(_obj).ToList();

        public ICollection<object> Values => ObjectPath.GetProperties(_obj).Select(propertyName => GetValue(propertyName)).ToList();

        public int Count => ObjectPath.GetProperties(_obj).Count();

        public bool IsReadOnly => true;

        public object this[string key] { get => GetValue(key); set => throw new NotSupportedException(Notsupported); }

        public void Add(string key, object value)
        {
            throw new NotSupportedException(Notsupported);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(Notsupported);
        }

        public void Clear()
        {
            throw new NotSupportedException(Notsupported);
        }

        public override bool Equals(object other)
        {
            if (other is ReadOnlyObject esp)
            {
                return _obj.Equals(esp._obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _obj.GetHashCode();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(Notsupported);
        }

        public bool ContainsKey(string key)
        {
            return ObjectPath.GetProperties(_obj).Any(propertyName => propertyName.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException(Notsupported);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ObjectPath.GetProperties(_obj).Select(propertyName => new KeyValuePair<string, object>(propertyName, GetValue(propertyName))).GetEnumerator();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException(Notsupported);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException(Notsupported);
        }

        public bool TryGetValue(string name, out object value)
        {
            return ObjectPath.TryGetPathValue(_obj, name, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private object GetValue(string name)
        {
            if (TryGetValue(name, out var val))
            {
                return new ReadOnlyObject(val);
            }

            return null;
        }
    }
}
