using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Debugging
{
    public sealed class SourceContext : IDisposable
    {
        private readonly Stack<SourceRange> context;

        public SourceContext(Stack<SourceRange> context, SourceRange range)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.context.Push(range);
        }

        public void Dispose()
        {
            this.context.Pop();
        }

        internal static (JToken, SourceRange) ReadTokenRange(JsonReader reader, Stack<SourceRange> context)
        {
            var range = context.Count > 0
                ? context.Peek().DeepClone()
                : new SourceRange();

            var token = SourcePoint.ReadObjectWithSourcePoints(reader, JToken.Load, out var start, out var end);
            range.StartPoint = start;
            range.EndPoint = end;

            var designer = token.SelectToken("$designer", errorWhenNoMatch: false);
            if (designer != null)
            {
                range.Designer = designer;
            }

            return (token, range);
        }
    }
}
