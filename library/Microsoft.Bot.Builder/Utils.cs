using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    public static class SetField
    {
        public static void NotNull<T>(out T field, string name, T value)
            where T : class
        {
            if (value != null)
            {
                field = value;
            }
            else
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void CheckNull<T>(string name, T value)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
