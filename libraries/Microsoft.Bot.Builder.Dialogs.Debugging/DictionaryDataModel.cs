// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class DictionaryDataModel<TKey, TValue> : DataModelBase<IDictionary<TKey, TValue>, TKey, TValue>
    {
        public DictionaryDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 6;

        public override TValue this[IDictionary<TKey, TValue> context, TKey name]
        {
            get => context[name];
            set => context[name] = value;
        }

        public override IEnumerable<TKey> Names(IDictionary<TKey, TValue> context) => context.Keys;
    }
}
