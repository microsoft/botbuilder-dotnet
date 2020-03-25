// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// Uses resourceExplorer to mount root lg and all language variants as a multi language generator.
    /// </summary>
    /// <remarks>
    /// Given file name like "foo.lg" this will generate a map of foo.{LOCALE}.lg files.
    /// </remarks>
    public class ResourceMultiLanguageGenerator : MultiLanguageGeneratorBase
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.ResourceMultiLanguageGenerator";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMultiLanguageGenerator"/> class.
        /// </summary>
        /// <param name="resourceId">foo.lg.</param>
        public ResourceMultiLanguageGenerator(string resourceId = null)
        {
            this.ResourceId = resourceId;
        }

        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Implementation of lookup by locale.  This uses resourceId and ResourceExplorer to lookup.
        /// </summary>
        /// <param name="context">context.</param>
        /// <param name="locale">locale to lookup.</param>
        /// <param name="languageGenerator">found LanguageGenerator.</param>
        /// <returns>true if found.</returns>
        public override bool TryGetGenerator(ITurnContext context, string locale, out LanguageGenerator languageGenerator)
        {
            var lgm = context.TurnState.Get<LanguageGeneratorManager>();
            var resourceId = string.IsNullOrEmpty(locale) ? this.ResourceId : this.ResourceId.Replace(".lg", $".{locale}.lg");
            return lgm.LanguageGenerators.TryGetValue(resourceId, out languageGenerator);
        }
    }
}
