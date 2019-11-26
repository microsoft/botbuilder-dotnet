// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class NullDataModel : IDataModel
    {
        public static readonly IDataModel Instance = new NullDataModel();

        private NullDataModel()
        {
        }

        int IDataModel.Rank => 0;

        object IDataModel.this[object context, object name] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        bool IDataModel.IsScalar(object context) => true;

        IEnumerable<object> IDataModel.Names(object context) => Enumerable.Empty<object>();

        string IDataModel.ToString(object context) => "null";
    }
}
