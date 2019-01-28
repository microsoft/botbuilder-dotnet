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
        private readonly Dictionary<string, LGFileParser.TemplateDefinitionContext> _templates = null;


        public LGFileVisitor(string templateName, object scope, TemplateEngine engine, Dictionary<string, LGFileParser.TemplateDefinitionContext> templates)
        {
            TemplateName = templateName;
            Scope = scope;
            _engine = engine;
            _templates = templates;
        }

        public override string VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateName();
            if (templateNameContext.IDENTIFIER().GetText().Equals(TemplateName))
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
                var result = ExpressionEngine.Evaluate(exp, Scope, null, ExtendedFunctions.ExtendedMethod);

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
            var result = ExpressionEngine.Evaluate(exp, Scope, null, ExtendedFunctions.ExtendedMethod);
            return result.ToString();
        }

        private string EvalTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');
            if (argsStartPos > 0) // Do have args
            {
                // Evaluate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos+1)
                {
                    throw new Exception($"Not a valid template ref: {exp}");
                }
                var argExpressions = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(",");
                var args = argExpressions.Select(x => ExpressionEngine.Evaluate(x, Scope, null, ExtendedFunctions.ExtendedMethod)).ToList();

                // Construct a new Scope for this template reference
                // Bind all arguments to parameters
                var templateName = exp.Substring(0, argsStartPos);
                var paramters = ExtractParameters(templateName);

                if (paramters.Count != args.Count)
                {
                    throw new Exception($"Arguments count mismatch for template ref {exp}, expected {paramters.Count}, actual {args.Count}");
                }

                var newScope = paramters.Zip(args, (k, v) => new { k, v })
                                        .ToDictionary(x => x.k, x => x.v);

                return _engine.Evaluate(templateName, newScope);
                
            }
            return _engine.Evaluate(exp, Scope);
        }

        private List<string> ExtractParameters(string templateName)
        {
            if (!_templates.ContainsKey(templateName))
            {
                throw new Exception($"No such template: {templateName}");
            }

            var context = _templates[templateName];
            return context.templateName().parameters().IDENTIFIER().Select(x => x.GetText()).ToList();
        }

    }
}
