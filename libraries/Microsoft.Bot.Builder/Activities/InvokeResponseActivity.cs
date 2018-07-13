// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public class InvokeResponseActivity : ActivityWithValue
    {
        public InvokeResponseActivity()
            : base(ActivityTypesEx.InvokeResponse)
        {
        }
    }
}
