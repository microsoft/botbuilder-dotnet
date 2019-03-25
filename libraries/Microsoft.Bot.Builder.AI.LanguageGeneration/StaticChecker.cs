using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class StaticChecker : LGFileParserBaseVisitor<int>
    {
        public readonly EvaluationContext Context;

        public StaticChecker(EvaluationContext context)
        {
            Context = context;
        }

        public void Check()
        {
            foreach (var template in Context.TemplateContexts)
            {
                Visit(template.Value);
            }
        }

        public override int VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateName = context.templateNameLine().templateName().GetText();

            if (context.templateBody() == null)
            {
                throw new LGParsingException($"There is no template body in template {templateName}");
            }

            Visit(context.templateBody());
            return 0;
        }

        public override int VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            return 0;
        }


        public override int VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            
            return 0; ;
        }

    }
}
