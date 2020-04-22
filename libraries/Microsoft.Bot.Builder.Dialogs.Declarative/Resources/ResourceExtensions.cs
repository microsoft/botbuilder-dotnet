// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Debugging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public static class ResourceExtensions
    {
        public static async Task<(JToken, SourceRange)> ReadTokenRangeAsync(this IResource resource, SourceContext sourceContext)
        {
            var text = await resource.ReadTextAsync();
            using (var readerText = new StringReader(text))
            using (var readerJson = new JsonTextReader(readerText))
            {
                var (token, range) = SourceScope.ReadTokenRange(readerJson, sourceContext);

                if (resource is FileResource fileResource)
                {
                    range.Path = fileResource.FullName;
                }

                return (token, range);
            }
        }
    }
}
