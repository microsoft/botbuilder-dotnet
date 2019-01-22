using System;
using System.Collections.Generic;
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

        public override string VisitFile([NotNull] LGFileParser.FileContext context)
        {
            // return the first template matched result
            return context.paragraph().Select(p => VisitParagraph(p))
                                      .First(s => !string.IsNullOrEmpty(s));
        }

        public override string VisitParagraph([NotNull] LGFileParser.ParagraphContext context)
        {
            var templateDef = context.templateDefinition();
            return templateDef == null ? null : VisitTemplateDefinition(templateDef);
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
            var normalTemplateStrs = context.normalTemplateBody().normalTemplateString();
            Random rd = new Random();
            return Visit(normalTemplateStrs[rd.Next(normalTemplateStrs.Length)]);
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
        

        private string EvalExpression(string exp)
        {
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
