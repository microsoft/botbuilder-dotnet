// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder.Templates
{
    public class TemplateIdMap : Dictionary<string, Func<IBotContext, dynamic, object>>
    {
    }

    public class TemplateDictionary : Dictionary<string, TemplateIdMap>
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
    ///   To use, simply add to your pipeline
    ///   bot.use(new DictionaryTemplateEngine(myTemplates))
    /// </summary>
    public class DictionaryRenderer : ITemplateRenderer, Middleware.IContextCreated
    {
        private TemplateDictionary languages;

        public DictionaryRenderer(TemplateDictionary templates)
        {
            this.languages = templates;
        }        

        public async Task ContextCreated(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            // context.TemplateManager.Register(this);
            await next().ConfigureAwait(false); 
        }

        public Task<object> RenderTemplate(IBotContext context, string language, string templateId, object data)
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

    public static class BotDictionaryTemplateExtensions
    {
        /// <summary>
        /// UseTemplates- Adds templateDictionary to templateManager
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        public static Bot UseTemplates(this Bot bot, TemplateDictionary templates)
        {
            return bot.Use(new DictionaryRenderer(templates));
        }
    }
}
