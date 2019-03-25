using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class StaticChecker : LGFileParserBaseVisitor<List<string>>
    {
        public readonly EvaluationContext Context;

        public StaticChecker(EvaluationContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Return error messaages list
        /// </summary>
        /// <returns></returns>
        public List<string> Check()
        {
            var result = new List<string>();
            foreach (var template in Context.TemplateContexts)
            {
                result.AddRange(Visit(template.Value));
            }

            return result;
        }

        public override List<string> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var result = new List<string>();
            var templateName = context.templateNameLine().templateName().GetText();

            if (context.templateBody() == null)
            {
                result.Add($"There is no template body in template {templateName}");
            }
            else
            {
                result.AddRange(Visit(context.templateBody()));
            }

            return result;
        }
    }
}
