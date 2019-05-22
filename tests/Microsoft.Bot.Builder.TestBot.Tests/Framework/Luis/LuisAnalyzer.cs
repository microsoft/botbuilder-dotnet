// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples.Tests.Framework.Luis
{
    public class LuisAnalyzer
    {
        public LuisAnalyticsResult Analyze(string luFile)
        {
            return new LuisAnalyticsResult(new List<string>(), new List<string>(), new List<string>());
        }
    }
}
