// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Debugging.Base;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers
{
    /// <summary>
    /// This class maintains an integer identifier for C# memory within the debug adapter
    /// that is referenced in the debug adapter protocol with Visual Studio Code.
    /// Examples include stack frames, values, and breakpoints.
    /// Ideally, identifiers fit within a 53 bit JavaScript Number.
    /// Ideally, identifiers can be recycled at some point.
    /// Some identifiers have a lifetime scoped to a thread (e.g. values or stack frames)
    /// For these combined identifiers, we use 7 bit encoding.
    /// </summary>
    /// <typeparam name="T">Data type of the stored items.</typeparam>
    internal sealed class Identifier<T> : IIdentifier<T>
    {
        private readonly Dictionary<T, ulong> _codeByItem = new Dictionary<T, ulong>(ReferenceEquality<T>.Instance);
        private readonly Dictionary<ulong, T> _itemByCode = new Dictionary<ulong, T>();
        private ulong _last;

        IEnumerable<T> IIdentifier<T>.Items
            => _itemByCode.Values.ToArray();

        T IIdentifier<T>.this[ulong code]
            => _itemByCode[code];

        ulong IIdentifier<T>.this[T item]
            => _codeByItem[item];

        bool IIdentifier<T>.TryGetValue(ulong code, out T item)
            => _itemByCode.TryGetValue(code, out item);

        bool IIdentifier<T>.TryGetValue(T item, out ulong code)
            => _codeByItem.TryGetValue(item, out code);

        void IIdentifier<T>.Clear()
        {
            // do not reset the last code to avoid any risk of https://en.wikipedia.org/wiki/ABA_problem
            _itemByCode.Clear();
            _codeByItem.Clear();
        }

        ulong IIdentifier<T>.Add(T item)
        {
            if (!_codeByItem.TryGetValue(item, out var code))
            {
                // avoid false values
                code = ++_last;
                _codeByItem.Add(item, code);
                _itemByCode.Add(code, item);
            }

            return code;
        }

        void IIdentifier<T>.Remove(T item)
        {
            var code = _codeByItem[item];
            _itemByCode.Remove(code);
            _codeByItem.Remove(item);
        }

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator()
            => _itemByCode.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _itemByCode.GetEnumerator();
    }
}
