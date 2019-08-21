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

        public DictionaryRenderer(LanguageTemplateDictionary templates)
        {
            _languages = templates;
        }

        public Task<object> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data)
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
