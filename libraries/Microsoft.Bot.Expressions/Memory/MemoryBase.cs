namespace Microsoft.Bot.Expressions.Memory
{
    public abstract class MemoryBase : IMemory
    {
        public abstract object GetValue(string path);

        public abstract object SetValue(string path, object value);

        public virtual bool TryGetValue(string path, out object value)
        {
            value = null;
            try
            {
                value = GetValue(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool TrySetValue(string path, object value)
        {
            try
            {
                SetValue(path, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual string Version()
        {
            return "0";
        }
    }
}
