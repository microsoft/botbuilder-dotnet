namespace Microsoft.Bot.Builder.Core.State
{
    public abstract class DeferredValueStateStorageEntry : StateStorageEntry
    {
        private bool _isValueMaterialized = false;

        public DeferredValueStateStorageEntry(string stateNamespace, string key) : base(stateNamespace, key)
        {

        }

        public override T GetValue<T>()
        {
            if (_isValueMaterialized)
            {
                return base.GetValue<T>();
            }

            var value = this.MaterializeValue<T>();

            SetValue(value);

            return value;
        }

        public override void SetValue<T>(T value)
        {
            _isValueMaterialized = true;

            base.SetValue(value);
        }

        public override object RawValue
        {
            get => base.RawValue;
            protected internal set
            {
                base.RawValue = value;

                _isValueMaterialized = false;
            }
        }

        protected abstract T MaterializeValue<T>() where T : class, new();
    }
}