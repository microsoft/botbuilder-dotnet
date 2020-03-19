// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// This class maintains an integer identifier for C# memory within the debug adapter
    /// that is referenced in the debug adapter protocol with Visual Studio Code.
    /// Examples include stack frames, values, and breakpoints.
    /// Ideally, identitiers fit within a 53 bit JavaScript Number.
    /// Ideally, identifiers can be recycled at some point.
    /// Some identifiers have a lifetime scoped to a thread (e.g. values or stack frames)
    /// For these combined identifiers, we use 7 bit encoding.
    /// </summary>
    /// <typeparam name="T">Datatype of the stored items.</typeparam>
    public sealed class Identifier<T> : IIdentifier<T>
    {
        private readonly Dictionary<T, ulong> codeByItem = new Dictionary<T, ulong>(ReferenceEquality<T>.Instance);
        private readonly Dictionary<ulong, T> itemByCode = new Dictionary<ulong, T>();
        private ulong last = 0;

        IEnumerable<T> IIdentifier<T>.Items
            => this.itemByCode.Values.ToArray();

        T IIdentifier<T>.this[ulong code]
            => this.itemByCode[code];

        ulong IIdentifier<T>.this[T item]
            => this.codeByItem[item];

        bool IIdentifier<T>.TryGetValue(ulong code, out T item)
            => this.itemByCode.TryGetValue(code, out item);

        bool IIdentifier<T>.TryGetValue(T item, out ulong code)
            => this.codeByItem.TryGetValue(item, out code);

        void IIdentifier<T>.Clear()
        {
            // do not reset the last code to avoid any risk of https://en.wikipedia.org/wiki/ABA_problem
            this.itemByCode.Clear();
            this.codeByItem.Clear();
        }

        ulong IIdentifier<T>.Add(T item)
        {
            if (!this.codeByItem.TryGetValue(item, out var code))
            {
                // avoid false values
                code = ++last;
                this.codeByItem.Add(item, code);
                this.itemByCode.Add(code, item);
            }

            return code;
        }

        void IIdentifier<T>.Remove(T item)
        {
            var code = this.codeByItem[item];
            this.itemByCode.Remove(code);
            this.codeByItem.Remove(item);
        }

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator()
            => this.itemByCode.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.itemByCode.GetEnumerator();
    }
}
