// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.CustomMiddleware
{
    public class ExampleMiddleware : IContextCreated, IReceiveActivity, ISendActivity
    {
        private string _name;
        private static object _syncRoot = new object();

        public ExampleMiddleware(string name)
        {
            _name = name;
        }

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            Write($"BEFORE ContextCreated");
            await next();
            Write($"AFTER ContextCreated");
        }

        public async Task ReceiveActivity(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            Write($"BEFORE ReceiveActivity {PrettyPrint(context.Request)}");
            await next();
            Write($"AFTER ReceiveActivity {PrettyPrint(context.Request)}");
        }

        public async Task SendActivity(IBotContext context, IList<Activity> activities, MiddlewareSet.NextDelegate next)
        {
            Write($"BEFORE SendActivity {PrettyPrint(context.Responses)}");
            await next();
            Write($"AFTER SendActivity {PrettyPrint(context.Responses)}");
        }

        private void Write(string message)
        {
            lock (_syncRoot)
            {
                using (var writer = new StreamWriter(@"C:\Users\Public\ExampleMiddleware.txt", true))
                {
                    writer.WriteLine($"{_name} {message}");
                }
            }
        }

        private string PrettyPrint(IActivity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                return $"{ActivityTypes.Message}({((IMessageActivity)activity).Text})";
            }
            return activity.Type;
        }

        private string PrettyPrint(IList<Activity> activities)
        {
            var s = new StringBuilder();
            foreach (var activity in activities)
            {
                s.AppendFormat($"{PrettyPrint(activity)} ");
            }
            return s.ToString();
        }
    }
}
