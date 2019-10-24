// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IDataModel
    {
        int Rank { get; }

        object this[object context, object name]
        {
            get;
            set;
        }

        bool IsScalar(object context);

        IEnumerable<object> Names(object context);

        string ToString(object context);
    }
}
