using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TemplateManager;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class LanguageGenerationRenderer : ITemplateRenderer
    {
        private readonly TemplateEngine templateEngine;

        public LanguageGenerationRenderer(string lgFilePath)
        {
            if (string.IsNullOrEmpty(lgFilePath))
            {
                throw new ArgumentException(nameof(lgFilePath));
            }

            this.templateEngine = new TemplateEngine().AddFile(lgFilePath);
        }

        public LanguageGenerationRenderer(TemplateEngine templateEngine)
        {
            this.templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        }

        public async Task<object> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data)
        {
            return await Task.Run(() => templateEngine.EvaluateTemplate(templateId, data, null));
        }
    }
}
