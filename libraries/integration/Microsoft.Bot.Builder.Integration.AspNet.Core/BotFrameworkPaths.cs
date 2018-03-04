// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkPaths
    {
        public BotFrameworkPaths()
        {
            this.BasePath = "/api";
            this.ActivitiesPath = "/messages";
            this.ProactivePath = "/messages/proactive";
        }

        public PathString BasePath { get; set; }
        public PathString ActivitiesPath { get; set; }
        public PathString ProactivePath { get; set; }
    }
}