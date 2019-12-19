using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Bot.Expressions.Memory
{
    public abstract class MemoryBase : IMemory
    {
        public abstract object GetValue(string path);

        public abstract object SetValue(string path, object value);

        public abstract bool ContainsPath(string path);

        public virtual bool TryGetValue(string path, out object value)
        {
            value = default;
            if (ContainsPath(path))
            {
                value = GetValue(path);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool TrySetValue(string path, object value)
        {
            SetValue(path, value);
            return true;
        }

        public virtual string Version()
        {
            return "0";
        }
    }
}
