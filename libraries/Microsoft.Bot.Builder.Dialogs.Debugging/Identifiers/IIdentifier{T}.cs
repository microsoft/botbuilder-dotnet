using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IIdentifier<T> : IEnumerable<KeyValuePair<ulong, T>>
    {
        IEnumerable<T> Items
        {
            get;
        }

        T this[ulong code]
        {
            get;
        }

        ulong this[T item]
        {
            get;
        }

        bool TryGetValue(ulong code, out T item);

        bool TryGetValue(T item, out ulong code);

        ulong Add(T item);

        void Remove(T item);
    }
}
