// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public static class TranscriptUtilities
    {
        public static IEnumerable<IActivity> GetFromTestContext(TestContext context)
        {
            var transcriptsRootFolder = TestUtilities.GetKey("TranscriptsRootFolder") ?? @"..\..\..\..\..\transcripts";
            var directory = Path.Combine(transcriptsRootFolder, context.FullyQualifiedTestClassName.Split('.').Last());
            var fileName = $"{context.TestName}.transcript";
            var path = Path.Combine(directory, fileName);
            if (!File.Exists(path))
            {
                Assert.Fail($"Required transcript file '{path}' does not exists in '{transcriptsRootFolder}' folder. Review the 'TranscriptsRootFolder' environment variable value.");
            }

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Activity[]>(content);
        }

        public static ConversationReference GetConversationReference(this IActivity activity)
        {
            bool IsReply(IActivity act) => string.Equals("bot", act.From?.Role, StringComparison.InvariantCultureIgnoreCase);
            var bot = IsReply(activity) ? activity.From : activity.Recipient;
            var user = IsReply(activity) ? activity.Recipient : activity.From;
            return new ConversationReference
            {
                User = user,
                Bot = bot,
                Conversation = activity.Conversation,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl
            };
        }
    }
}
