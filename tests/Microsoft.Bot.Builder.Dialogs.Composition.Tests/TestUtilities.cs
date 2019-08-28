// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Tests
{
    public class TestUtilities
    {
        private static Lazy<Dictionary<string, string>> environmentKeys = new Lazy<Dictionary<string, string>>(() =>
        {
            try
            {
                return File.ReadAllLines(@"\\fusebox\private\sdk\UnitTestKeys-new.cmd")
                    .Where(l => l.StartsWith("@set", StringComparison.OrdinalIgnoreCase))
                    .Select(l => l.Replace("@set ", string.Empty, StringComparison.OrdinalIgnoreCase).Split('='))
                    .ToDictionary(pairs => pairs[0], pairs => pairs[1]);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
                return new Dictionary<string, string>();
            }
        });

        public static TurnContext CreateEmptyContext()
        {
            var b = new TestAdapter();
            var a = new Activity
            {
                Type = ActivityTypes.Message,
                ChannelId = "EmptyContext",
                From = new ChannelAccount
                {
                    Id = "empty@empty.context.org",
                },
                Conversation = new ConversationAccount()
                {
                    Id = "213123123123"
                }
            };
            var bc = new TurnContext(b, a);

            return bc;
        }

        public static string GetKey(string key)
        {
            if (!environmentKeys.Value.TryGetValue(key, out var value))
            {
                // fallback to environment variables
                value = Environment.GetEnvironmentVariable(key);
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = null;
                }
            }

            return value;
        }
    }
}
