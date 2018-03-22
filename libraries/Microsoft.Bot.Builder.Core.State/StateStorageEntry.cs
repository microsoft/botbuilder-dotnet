using System;

namespace Microsoft.Bot.Builder.Core.State
{
    public class StateStorageEntry : IStateStorageEntry
    {
        private string _namespace;
        private string _key;
        private string _eTag;
        private object _value;

        public StateStorageEntry(string stateNamespace, string key)
        {
            _namespace = stateNamespace ?? throw new ArgumentNullException(nameof(stateNamespace));
            _key = key ?? throw new ArgumentNullException(nameof(key));

        }

        public StateStorageEntry(string stateNamespace, string key, object value) : this(stateNamespace, key)
        {
            _value = value;
        }

        public StateStorageEntry(string stateNamespace, string key, string eTag) : this(stateNamespace, key)
        {
            _eTag = eTag;
        }

        public StateStorageEntry(string stateNamespace, string key, string eTag, object value) : this(stateNamespace, key, eTag)
        {
            _value = value;
        }

        public string Namespace => _namespace;

        public string Key => _key;

        public string ETag
        {
            get => _eTag;
            protected internal set
            {
                _eTag = value;
            }
        }

        public object RawValue
        {
            get => _value;
            protected internal set
            {
                _value = value;
            }
        }

        public virtual T GetValue<T>() where T : class, new() => (T)_value;

        public virtual void SetValue<T>(T value) where T : class => _value = value;
    }
}