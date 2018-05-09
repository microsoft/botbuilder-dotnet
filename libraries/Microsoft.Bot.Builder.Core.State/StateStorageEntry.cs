using System;

namespace Microsoft.Bot.Builder.Core.State
{
    public class StateStorageEntry : IStateStorageEntry
    {
        private readonly string _namespace;
        private readonly string _key;
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

        public string Namespace => _namespace;

        public string Key => _key;

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