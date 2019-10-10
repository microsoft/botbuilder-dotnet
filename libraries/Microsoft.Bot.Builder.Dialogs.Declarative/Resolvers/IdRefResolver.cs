// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers
{
    public class IdRefResolver : IRefResolver
    {
        private const string RefPropertyName = "$copy";

        private readonly JToken rootDocument;
        private readonly ResourceExplorer resourceExplorer;
        private readonly ISourceMap sourceMap;

        public IdRefResolver(ResourceExplorer resourceExplorer, ISourceMap sourceMap, JToken rootDocument = null)
        {
            this.resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
            this.sourceMap = sourceMap;
            this.rootDocument = rootDocument;
        }

        public bool IsRef(JToken token)
        {
            return !string.IsNullOrEmpty(GetRefTarget(token));
        }

        public async Task<JToken> ResolveAsync(JToken refToken)
        {
            var refTarget = GetRefTarget(refToken);

            if (string.IsNullOrEmpty(refTarget))
            {
                throw new InvalidOperationException("Failed to resolve reference, $copy property not present");
            }

            var resource = resourceExplorer.GetResource($"{refTarget}.dialog");
            string text = await resource.ReadTextAsync().ConfigureAwait(false);
            var json = JToken.Parse(text);

            foreach (JProperty prop in refToken.Children<JProperty>())
            {
                if (prop.Name != "$ref")
                {
                    // JToken is an object, so we merge objects
                    if (json[prop.Name] != null && json[prop.Name].Type == JTokenType.Object)
                    {
                        JObject targetProperty = json[prop.Name] as JObject;
                        targetProperty.Merge(prop.Value);
                    }

                    // JToken is an object, so we merge objects
                    else if (json[prop.Name] != null && json[prop.Name].Type == JTokenType.Array)
                    {
                        JArray targetArray = json[prop.Name] as JArray;
                        targetArray.Merge(prop.Value);
                    }

                    // JToken is a value, simply assign
                    else
                    {
                        json[prop.Name] = prop.Value;
                    }
                }
            }

            // if we have a source path for the resource, then make it available to InterfaceConverter
            if (resource is FileResource fileResource)
            {
                sourceMap.Add(json, new SourceRange() { Path = fileResource.FullName });
            }

            return json;
        }

        private string GetRefTarget(JToken token)
        {
            // If we expect an instance of IMyInterface and we find a string,
            // we assume that it is an implicit reference
            if (token.Type == JTokenType.String)
            {
                return token.Value<string>();
            }

            // Else try to get a reference from the token
            return token?
                .Children<JProperty>()
                .FirstOrDefault(jProperty => jProperty.Name == RefPropertyName)
                ?.Value.ToString();
        }
    }
}
