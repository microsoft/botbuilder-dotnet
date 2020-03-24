// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class EnumerableDataModel<T> : DataModelBase<IEnumerable<T>, int, T>
    {
        public EnumerableDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 3;

        public override T this[IEnumerable<T> context, int name]
        {
            get => context.ElementAt(name);
            set => throw new NotSupportedException();
        }

        public override IEnumerable<int> Names(IEnumerable<T> context) => context.Select((_, index) => index);
    }
}
