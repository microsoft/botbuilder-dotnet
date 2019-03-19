using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers
{
    public class IdRefResolver : IRefResolver
    {
        private const string RefPropertyName = "$copy";

        private readonly JToken rootDocument;
        private readonly IBotResourceProvider resourceProvider;

        public IdRefResolver(IBotResourceProvider resourceProvider, JToken rootDocument = null)
        {
            this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
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

            var targetFragments = refTarget.Split('#');

            string jsonFile;
            string jsonPointer = null;

            var dialogResources = await resourceProvider.GetResources("dialog").ConfigureAwait(false);
            var refResources = dialogResources?.Where(r => r.Name == targetFragments[0]).ToList();

            // Ref target must exist
            if (refResources == null || refResources.Count == 0)
            {
                throw new Exception($"Reference {targetFragments[0]} could not be resolved.");
            }

            // Ref target should be unique
            if (refResources.Count > 1)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"Multiple resources found for id {targetFragments[0]}. Please ensure unique names to be able to reference them. Conflicts: ");

                refResources.ForEach(r => builder.AppendLine($"Name: {r.Name}. Path: .{r.Id}"));

                throw new Exception();
            }

            var text = await refResources.First().GetTextAsync().ConfigureAwait(false);
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
                ?.Value<string>();
        }
    }
}
