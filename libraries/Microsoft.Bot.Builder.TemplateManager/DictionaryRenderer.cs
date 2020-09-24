// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    ///   This is a simple template engine which has a resource map of template functions
    ///  let myTemplates  = {
    ///       "en" : {
    ///         "templateId": (context, data) => $"your name  is {data.name}",
    ///         "templateId": (context, data) => { return new Activity(); }
    ///     }`
    ///  }
    ///  }
    ///   To use, simply register with templateManager
    ///   templateManager.Register(new DictionaryRenderer(myTemplates)).
    /// </summary>
    public class DictionaryRenderer : ITemplateRenderer
    {
        private LanguageTemplateDictionary _languages;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryRenderer"/> class.
        /// </summary>
        /// <param name="templates">The language template dictionary to use.</param>
        public DictionaryRenderer(LanguageTemplateDictionary templates)
        {
            _languages = templates;
        }

        /// <summary>
        /// Renders a template to an activity or string.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="language">The language to render.</param>
        /// <param name="templateId">The template to render.</param>
        /// <param name="data">The data object to use to render.</param>
        /// <returns>Task.</returns>
#pragma warning disable UseAsyncSuffix // Use Async suffix (we can't change this without breaking compat)
        public Task<object> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data)
#pragma warning restore UseAsyncSuffix // Use Async suffix
        {
            if (_languages.TryGetValue(language, out var templates))
            {
                if (templates.TryGetValue(templateId, out var template))
                {
                    dynamic result = template(turnContext, data);
                    if (result != null)
                    {
                        return Task.FromResult(result as object);
                    }
                }
            }

            return Task.FromResult((object)null);
        }
    }
}
