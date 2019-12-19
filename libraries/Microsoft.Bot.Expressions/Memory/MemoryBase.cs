using System;

namespace Microsoft.Bot.Expressions.Memory
{
    public abstract class MemoryBase : IMemory
    {
        public abstract object GetValue(string path);

        public abstract object SetValue(string path, object value);

        public (object value, string error) TryGetValue(string path)
        {
            try
            {
                var value = GetValue(path);
                return (value, null);
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public (object value, string error) TrySetValue(string path, object value)
        {
            try
            {
                var result = SetValue(path, value);
                return (result, null);
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        public virtual string Version()
        {
            return "0";
        }
    }
}
