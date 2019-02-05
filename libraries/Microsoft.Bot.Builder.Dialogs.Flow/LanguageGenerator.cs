using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.AI.LanguageGeneration;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    public class LangugageGenerator : ILanguageGenerator
    {
        private readonly TemplateEngine templateEngine;

        public LangugageGenerator(string lgFilePath)
        {
            if (string.IsNullOrEmpty(lgFilePath))
            {
                throw new ArgumentException(nameof(lgFilePath));
            }

            this.templateEngine = TemplateEngine.FromFile(lgFilePath);
        }

        public string Apply(string template, object data, string locale = null)
        {
            return templateEngine.Evaluate(template, data);
        }
    }
}
