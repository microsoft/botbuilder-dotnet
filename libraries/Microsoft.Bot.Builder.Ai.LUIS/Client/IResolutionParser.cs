// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    public interface IResolutionParser
    {
        bool TryParse(IDictionary<string, object> properties, out Resolution resolution);
    }
}
