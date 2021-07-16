// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// A simple ISourceMap of objects -> SourceRange.
    /// </summary>
    public class SourceMap : ISourceMap
    {
        /// <summary>
        /// Initializes a read-only new instance of the <see cref="SourceMap"/>.
        /// </summary>
        public static readonly SourceMap Instance = new SourceMap();
        
        private readonly object _gate = new object();
        private readonly Dictionary<object, SourceRange> _items = new Dictionary<object, SourceRange>(ReferenceEquality<object>.Instance);
        private readonly Dictionary<SourceRange, object> _reverseLookup = new Dictionary<SourceRange, object>(new SourceRangeEqualityComparer());

        void ISourceMap.Add(object item, SourceRange range)
        {
            if (range.Path != null && !Path.IsPathRooted(range.Path))
            {
                throw new ArgumentOutOfRangeException(range.Path);
            }

            lock (_gate)
            {
                if (_reverseLookup.TryGetValue(range, out object foundRef))
                {
                    _items.Remove(foundRef);
                    _reverseLookup.Remove(range);
                }

                _items.Add(item, range);
                _reverseLookup.Add(range, item);
            }
        }

        bool ISourceMap.TryGetValue(object item, out SourceRange range)
        {
            if (item != null)
            {
                lock (_gate)
                {
                    return _items.TryGetValue(item, out range);
                }
            }

            range = default;
            return false;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    internal sealed class ReferenceEquality<T> : IEqualityComparer<T>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public static readonly IEqualityComparer<T> Instance = new ReferenceEquality<T>();

        private ReferenceEquality()
        {
        }

        bool IEqualityComparer<T>.Equals(T x, T y) => ReferenceEquals(x, y);

        int IEqualityComparer<T>.GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }

#pragma warning disable SA1402 // File may only contain a single type
    internal class SourceRangeEqualityComparer : EqualityComparer<SourceRange>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public override bool Equals(SourceRange x, SourceRange y)
        {
            return x.Equals(y);
        }

        public override int GetHashCode(SourceRange obj)
        {
            return obj.GetHashCode();
        }
    }
}
