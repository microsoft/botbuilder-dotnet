// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkPaths
    {
        public BotFrameworkPaths()
        {
            this.BasePath = "api/";
            this.MessagesPath = "messages";
            this.ProactiveMessagesPath = "messages/proactive";
        }

        public string BasePath { get; set; }
        public string MessagesPath { get; set; }
        public string ProactiveMessagesPath { get; set; }
    }
}