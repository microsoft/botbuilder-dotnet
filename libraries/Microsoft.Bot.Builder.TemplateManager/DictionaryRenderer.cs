using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// Map of Template Ids-> Template Function()
    /// </summary>
    public class TemplateIdMap : Dictionary<string, Func<ITurnContext, dynamic, object>>
    {
    }
    
    /// <summary>
    /// Map of language -> template functions
    /// </summary>
    public class LanguageTemplateDictionary : Dictionary<string, TemplateIdMap>
    {
    }

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
    ///   templateManager.Register(new DictionaryRenderer(myTemplates))
    /// </summary>
    public class DictionaryRenderer : ITemplateRenderer
    {
        private LanguageTemplateDictionary languages;

        public DictionaryRenderer(LanguageTemplateDictionary templates)
        {
            this.languages = templates;
        }

        public Task<object> RenderTemplate(ITurnContext context, string language, string templateId, object data)
        {
            if (this.languages.TryGetValue(language, out var templates))
            {
                if (templates.TryGetValue(templateId, out var template))
                {
                    dynamic result = template(context, data);
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
