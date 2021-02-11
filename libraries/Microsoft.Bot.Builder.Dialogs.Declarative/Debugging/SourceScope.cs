// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Debugging
{
    internal sealed class SourceScope : IDisposable
    {
        private readonly SourceContext sourceContext;

        internal SourceScope(SourceContext sourceContext, SourceRange range)
        {
            this.sourceContext = sourceContext ?? throw new ArgumentNullException(nameof(sourceContext));
            this.sourceContext.CallStack.Push(range);
        }

        public void Dispose()
        {
            this.sourceContext.CallStack.Pop();
        }

        internal static (JToken, SourceRange) ReadTokenRange(JsonReader reader, SourceContext sourceContext)
        {
            var range = sourceContext.CallStack.Count > 0
                ? sourceContext.CallStack.Peek().DeepClone()
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
