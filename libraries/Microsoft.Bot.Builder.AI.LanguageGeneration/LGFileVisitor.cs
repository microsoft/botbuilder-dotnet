using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    class LGFileVisitor: LGFileParserBaseVisitor<string>
    {
        private string TemplateName { get; set; }
        private object Scope { get; set; }
        private readonly TemplateEngine _engine;

        public LGFileVisitor(string templateName, object scope, TemplateEngine engine )
        {
            TemplateName = templateName;
            Scope = scope;
            _engine = engine;
        }

        public override string VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateName();
            if (templateNameContext.TEXT().GetText().Equals(TemplateName))
            {
                return Visit(context.templateBody());
            }
            return null;
        }

        public override string VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context)
        {
            return Visit(context.normalTemplateBody());
        }

        public override string VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var normalTemplateStrs = context.normalTemplateString();
            Random rd = new Random();
            return Visit(normalTemplateStrs[rd.Next(normalTemplateStrs.Length)]);
        }

        public override string VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var caseRules = context.conditionalTemplateBody().caseRule();
            foreach (var caseRule in caseRules)
            {
                var conditionExpression = caseRule.caseCondition().EXPRESSION().GetText();
                if (EvalCondition(conditionExpression))
                {
                    return Visit(caseRule.normalTemplateBody());
                }
            }
            return Visit(context.conditionalTemplateBody().defaultRule().normalTemplateBody());
        }
        

        public override string VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var builder = new StringBuilder();
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.EXPRESSION:
                        builder.Append(EvalExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        builder.Append(EvalTemplateRef(node.GetText()));
                        break;
                    default:
                        builder.Append(node.GetText());
                        break;
                }
            }
            return builder.ToString();
        }
        

        private bool EvalCondition(string exp)
        {
            try
            {
                exp = exp.TrimStart('{').TrimEnd('}');
                var result = ExpressionEngine.Evaluate(exp, Scope);

                if ((result is Boolean r1 && r1 == false) ||
                    (result is int r2 && r2 == 0))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Expression {exp} evaled as false due to exception");
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private string EvalExpression(string exp)
        {
            exp = exp.TrimStart('{').TrimEnd('}');
            var result = ExpressionEngine.Evaluate(exp, Scope);
            return result.ToString();
        }

        private string EvalTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();
            return _engine.Evaluate(exp, Scope);
        }
    }
}
