// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class TestUtilities
    {
        public static BotContext CreateEmptyContext()
        {
            TestAdapter b = new TestAdapter();
            Activity a = new Activity
            {
                Type = ActivityTypes.Message
            };
            BotContext bc = new BotContext(b, a);

            return bc;
        }

        public static T CreateEmptyContext<T>() where T:IBotContext
        {
            TestAdapter b = new TestAdapter();
            Activity a = new Activity();
            if (typeof(T).IsAssignableFrom(typeof(IBotContext)))
            {
                IBotContext bc = new BotContext(b, a);
                return (T)bc;
            }
            else
                throw new ArgumentException($"Unknown Type {typeof(T).Name}");            
        }

        static Lazy<Dictionary<string, string>> environmentKeys = new Lazy<Dictionary<string, string>>(()=>
        {
            try
            {
                return File.ReadAllLines(@"\\fusebox\private\sdk\UnitTestKeys.cmd")
                    .Where(l => l.StartsWith("@set"))
                    .Select(l => l.Replace("@set ", "").Split('='))
                    .ToDictionary(pairs => pairs[0], pairs => pairs[1]);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.Message);
                return new Dictionary<string, string>();
            }
        });

        public static string GetKey(string key)
        {
            environmentKeys.Value.TryGetValue(key, out string value);
            return value;
        }   
    }
}
