// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;

namespace Microsoft.BotBuilderSamples
{
    public static class LanguageGenerationUtilities
    {
        /// <summary>
        /// Creates an instance of <see cref="LanguageGenerationResolver"/>.
        /// </summary>
        /// <param name="applicationId">Language generation application id.</param>
        /// <param name="applicationRegion">Language generation application region.</param>
        /// <param name="applicationLocale">Language generation application locale.</param>
        /// <param name="applicationVersion">Language generation application version.</param>
        /// <param name="subscriptionKey">The language generation service subscription key.</param>
        /// <returns>An instance of the language generation resolver.</returns>
        public static LanguageGenerationResolver CreateResolver(string applicationId, string applicationRegion, string applicationLocale, string applicationVersion, string subscriptionKey)
        {
            var application = new LanguageGenerationApplication(applicationId, applicationRegion, applicationLocale, applicationVersion, subscriptionKey);

            return new LanguageGenerationResolver(application, languageGenerationOptions: null);
        }
    }
}
