using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{

    class EvaluationTarget
    {
        public EvaluationTarget(string templateName, object scope)
        {
            TemplateName = templateName;
            Scope = scope;
        }

        public string TemplateName { get; set; }
        public object Scope { get; set; }
    }

    class TemplateEvaluator : LGFileParserBaseVisitor<string>
    {
        public readonly EvaluationContext Context;

        private readonly IGetMethod GetMethodX;
        private readonly IGetValue GetValueX;

        private Stack<EvaluationTarget> evaluationTargetStack = new Stack<EvaluationTarget>();

        public TemplateEvaluator(EvaluationContext context, IGetMethod getMethod, IGetValue getValue)
        {
            Context = context;
            GetMethodX = getMethod ?? new GetMethodExtensions(this);
            GetValueX = getValue ?? new GetValueExtensions(this);
        }

        public string EvaluateTemplate(string templateName, object scope)
        {
            if (!Context.TemplateContexts.ContainsKey(templateName))
            {
                throw new LGEvaluatingException($"No such template: {templateName}");
            }

            if (evaluationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new Exception($"Loop detected: {String.Join(" => ", evaluationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evalution trace
            evaluationTargetStack.Push(new EvaluationTarget(templateName, scope));
            string result = Visit(Context.TemplateContexts[templateName]);
            evaluationTargetStack.Pop();

            return result;
        }

        public override string VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateNameLine();
            if (templateNameContext.templateName().GetText().Equals(CurrentTarget().TemplateName))
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
                var conditionExpression = caseRule.caseCondition().EXPRESSION(0).GetText();
                if (EvalCondition(conditionExpression))
                {
                    return Visit(caseRule.normalTemplateBody());
                }
            }

            if (context?.conditionalTemplateBody()?.defaultRule() != null)
            {
                return Visit(context.conditionalTemplateBody().defaultRule().normalTemplateBody());
            }
            else
            {
                return null;
            }
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
                    case LGFileParser.ESCAPE_CHARACTER:
                        builder.Append(EvalEscapeCharacter(node.GetText()));
                        break;
                    case LGFileParser.EXPRESSION:
                        builder.Append(EvalExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        builder.Append(EvalTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        builder.Append(EvalMultiLineText(node.GetText()));
                        break;
                    default:
                        builder.Append(node.GetText());
                        break;
                }
            }
            return builder.ToString();
        }

        private string EvalEscapeCharacter(string exp)
        {
            var validCharactersDict = new Dictionary<string, string> {
                //Top four items :C# later render engine will treat them as escape characters, so the format is unchanged
                { @"\r","\r"},
                { @"\n","\n"},
                { @"\t","\t"},
                { @"\\","\\"},

                { @"\[","["},
                { @"\]","]"},
                { @"\{","{"},
                { @"\}","}"},
            };

            return validCharactersDict[exp];
        }

        private bool EvalCondition(string exp)
        {
            try
            {
                exp = exp.TrimStart('{').TrimEnd('}');
                var (result, error) = EvalByExpressionEngine(exp, CurrentTarget().Scope);

                if (error != null
                    || result == null
                    || (result is Boolean r1 && r1 == false) 
                    || (result is int r2 && r2 == 0))
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
            var result = EvalByExpressionEngine(exp, CurrentTarget().Scope).value;
            return result?.ToString();
        }

        private string EvalTemplateRef(string exp)
        {
            exp = exp.TrimStart('[').TrimEnd(']').Trim();

            var argsStartPos = exp.IndexOf('(');
            if (argsStartPos > 0) // Do have args
            {
                // EvaluateTemplate all arguments using ExpressoinEngine
                var argsEndPos = exp.LastIndexOf(')');
                if (argsEndPos < 0 || argsEndPos < argsStartPos + 1)
                {
                    throw new Exception($"Not a valid template ref: {exp}");
                }
                var argExpressions = exp.Substring(argsStartPos + 1, argsEndPos - argsStartPos - 1).Split(',');
                var args = argExpressions.Select(x => EvalByExpressionEngine(x, CurrentTarget().Scope).value).ToList();

                // Construct a new Scope for this template reference
                // Bind all arguments to parameters
                var templateName = exp.Substring(0, argsStartPos);
                var newScope = ConstructScope(templateName, args);

                return EvaluateTemplate(templateName, newScope);

            }
            return EvaluateTemplate(exp, CurrentTarget().Scope);
        }

        private EvaluationTarget CurrentTarget()
        {
            // just don't want to write evaluationTargetStack.Peek() everywhere
            return evaluationTargetStack.Peek();
        }

        private string EvalMultiLineText(string exp)
        {
            exp = exp.Substring(3, exp.Length - 6); //remove ``` ```
            var reg = @"@\{[^{}]+\}";
            var evalutor = new MatchEvaluator(m =>
            {
                var newExp = m.Value.Substring(1); // remove @
                if (newExp.StartsWith("{[") && newExp.EndsWith("]}"))
                {
                    return EvalTemplateRef(newExp.Substring(2, newExp.Length - 4))?
                            .Replace("\"", "\'"); ;//[ ]
                }
                else
                {
                    return EvalExpression(newExp)?.Replace("\"","\'");//{ }
                }
            });

            return Regex.Replace(exp, reg, evalutor);
        }
        private List<string> ExtractParameters(string templateName)
        {
            bool hasParameters = Context.TemplateParameters.TryGetValue(templateName, out List<string> parameters);
            return hasParameters ? parameters : new List<string>();
        }

        private (object value, string error) EvalByExpressionEngine(string exp, object scope)
        {
            var parse = new ExpressionEngine(GetMethodX.GetMethodX).Parse(exp);
            return parse.TryEvaluate(scope);
        }

        public object ConstructScope(string templateName, List<object> args)
        {
            if (args.Count == 1 &&
                !Context.TemplateParameters.ContainsKey(templateName))
            {
                // Special case, if no parameters defined, and only one arg, don't wrap
                return args[0];
            }

            var paramters = ExtractParameters(templateName);

            var newScope = paramters.Zip(args, (k, v) => new { k, v })
                                    .ToDictionary(x => x.k, x => x.v);

            return newScope;
        }

    }
}
