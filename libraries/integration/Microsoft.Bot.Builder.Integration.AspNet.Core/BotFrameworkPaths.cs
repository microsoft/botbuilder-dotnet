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
            this.MessagesPath = "/messages";
            this.ProactiveMessagesPath = "/messages/proactive";
        }

        public PathString BasePath { get; set; }
        public PathString MessagesPath { get; set; }
        public PathString ProactiveMessagesPath { get; set; }
    }
}