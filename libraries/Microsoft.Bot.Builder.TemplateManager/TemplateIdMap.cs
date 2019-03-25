// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// Map of Template Ids-> Template Function().
    /// </summary>
    public class TemplateIdMap : Dictionary<string, Func<ITurnContext, dynamic, object>>
    {
    }
}
