using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Checker
{
    public class LGFileChecker : LGFileParserBaseVisitor<int>
    {
        public readonly EvaluationContext Context;

        public LGFileChecker(EvaluationContext context)
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
