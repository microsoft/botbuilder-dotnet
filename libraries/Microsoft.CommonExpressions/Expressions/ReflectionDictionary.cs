using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class ReflectionDictionary : IReadOnlyDictionary<string, object>
    {
        public ReflectionDictionary(object instance)
        {
            Instance = instance;
        }

        private readonly object Instance;

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public IEnumerable<object> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public object this[string key] => throw new NotImplementedException();

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            var found = false;
            var type = Instance.GetType();
            var prop = type.GetProperty(key);
            if (prop != null)
            {
                value = prop.GetValue(Instance);
                found = true;
            }
            return found;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
