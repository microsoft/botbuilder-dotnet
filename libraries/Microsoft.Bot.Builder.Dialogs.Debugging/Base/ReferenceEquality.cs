// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class ReferenceEquality<T> : IEqualityComparer<T>
    {
        public static readonly IEqualityComparer<T> Instance = new ReferenceEquality<T>();

        private ReferenceEquality()
        {
        }

        bool IEqualityComparer<T>.Equals(T x, T y) => object.ReferenceEquals(x, y);

        int IEqualityComparer<T>.GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
