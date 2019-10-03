using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// This customzied memory scope is designed for allow sub template evaluation can refer
    /// to the orignial evaluation scope passed in by wrap the orignal one in globalScope field
    /// and inherit that for each sub evaluation 
    /// </summary>
    internal class CustomizedMemoryScope : IDictionary<string, object>
    {
        public CustomizedMemoryScope(object localScope, object globalScope)
        {
            this.LocalScope = localScope;
            this.GlobalScope = globalScope;
        }

        public object LocalScope { get; set; }

        public object GlobalScope { get; set; }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<object> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public object this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            var (result, error) = BuiltInFunctions.AccessProperty(this.LocalScope, key);
            if (result != null && error == null)
            {
                value = result;
                return true;
            }

            (result, error) = BuiltInFunctions.AccessProperty(this.GlobalScope, key);
            if (result != null && error == null)
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
