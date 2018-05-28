// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    public class TestUtilities
    {
        public static TurnContext CreateEmptyContext()
        {
            TestAdapter b = new TestAdapter();
            Activity a = new Activity
            {
                Type = ActivityTypes.Message
            };
            TurnContext bc = new TurnContext(b, a);

            return bc;
        }

        public static T CreateEmptyContext<T>() where T:ITurnContext
        {
            TestAdapter b = new TestAdapter();
            Activity a = new Activity();
            if (typeof(T).IsAssignableFrom(typeof(ITurnContext)))
            {
                ITurnContext bc = new TurnContext(b, a);
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
                    .Where(l => l.StartsWith("@set", StringComparison.OrdinalIgnoreCase))
                    .Select(l => l.Replace("@set ", "", StringComparison.OrdinalIgnoreCase).Split('='))
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
            if (!environmentKeys.Value.TryGetValue(key, out string value))
            {
                // fallback to environment variables
                value = Environment.GetEnvironmentVariable(key);
                if (String.IsNullOrWhiteSpace(value))
                    value = null;
            }
            return value;
        }
    }
}
