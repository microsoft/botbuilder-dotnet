// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class ReflectionDataModel<T> : DataModelBase<object, string, object>
    {
        public ReflectionDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 2;

        public override object this[object context, string name]
        {
            get => context.GetType().GetProperty(name).GetValue(context);
            set => context.GetType().GetProperty(name).SetValue(context, value);
        }

        public override IEnumerable<string> Names(object context) =>
            context.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).Select(p => p.Name);
    }
}
