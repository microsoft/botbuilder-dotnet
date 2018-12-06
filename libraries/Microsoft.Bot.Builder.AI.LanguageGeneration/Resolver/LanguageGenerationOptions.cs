
namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver
{
    public class LanguageGenerationOptions
    {
        /// <summary>
        /// Gets or sets application resolver API endpoint.
        /// </summary>
        /// <value>
        /// Resolver API endpoint.
        /// </value>
        public string ResolverApiEndpoint { get; set; }

        /// <summary>
        /// Gets or sets token generation API endpoint.
        /// </summary>
        /// <value>
        /// Token generation API endpoint.
        /// </value>
        public string TokenGenerationApiEndpoint { get; set; }
    }
}
