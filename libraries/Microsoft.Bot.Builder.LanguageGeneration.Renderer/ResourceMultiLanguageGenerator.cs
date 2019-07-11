using System;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Uses resourceExplorer to mount root lg and all variants as a language generator
    /// </summary>
    public class ResourceMultiLanguageGenerator : MultiLanguageGeneratorBase
    {
        /// <summary>
        /// Create MultiLanguageGeneartor based on Root lg file and language variants
        /// </summary>
        /// <remarks>
        /// Given file name like "foo.lg" this will generate a map of foo.{LOCALE}.lg files
        /// </remarks>
        /// <param name="resourceId">foo.lg</param>
        public ResourceMultiLanguageGenerator(string resourceId=null)
        {
            this.ResourceId = resourceId;
        }

        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        public override bool TryGetGenerator(ITurnContext context, string locale, out ILanguageGenerator languageGenerator)
        {
            var lgm = context.TurnState.Get<LanguageGeneratorManager>();
            var resourceId = (String.IsNullOrEmpty(locale)) ? this.ResourceId : this.ResourceId.Replace(".lg", $".{locale}.lg");
            return lgm.LanguageGenerators.TryGetValue(resourceId, out languageGenerator);
        }
    }
}
