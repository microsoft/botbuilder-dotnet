// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.DataModels
{
#pragma warning disable CA1812 // Supressing error due to internal being used as intended
    internal sealed class ReadOnlyDictionaryDataModel<TKey, TValue> : DataModelBase<IReadOnlyDictionary<TKey, TValue>, TKey, TValue>
#pragma warning restore CA1812 // Supressing error due to internal being used as intended
    {
        public ReadOnlyDictionaryDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 5;

        public override TValue this[IReadOnlyDictionary<TKey, TValue> context, TKey name]
        {
            get => context[name];
            set => throw new NotSupportedException();
        }

        public override IEnumerable<TKey> Names(IReadOnlyDictionary<TKey, TValue> context) => context.Keys;
    }
}
