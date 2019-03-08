using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{

    /// <summary>
    /// List all analyzers like dependencies
    /// 
    /// </summary>
    class Analyzer : LGFileParserBaseVisitor<List<string>>
    {
        public readonly EvaluationContext Context;

        private Stack<EvaluationTarget> evalutationTargetStack = new Stack<EvaluationTarget>();
        private EvaluationTarget currentTarget()
        {
            // just don't want to write evaluationTargetStack.Peek() everywhere
            return evalutationTargetStack.Peek();
        }


        public Analyzer(EvaluationContext context)
        {
            Context = context;
        }

        public List<string> AnalyzeTemplate(string templateName)
        {
            if (!Context.TemplateContexts.ContainsKey(templateName))
            {
                throw new Exception($"No such template: {templateName}");
            }

            if (evalutationTargetStack.Any(e => e.TemplateName == templateName))
            {
                throw new Exception($"Loop deteced: {String.Join(" => ", evalutationTargetStack.Reverse().Select(e => e.TemplateName))} => {templateName}");
            }

            // Using a stack to track the evalution trace
            evalutationTargetStack.Push(new EvaluationTarget(templateName, null));
            var result = Visit(Context.TemplateContexts[templateName]);
            var innerVariables = ExtractParameters(templateName);
            var finalResult = result.Where(u => !innerVariables.Contains(u)).ToList();
            
            result = Distinct(finalResult);
            evalutationTargetStack.Pop();

            return result;
        }

        public override List<string> VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
        {
            var templateNameContext = context.templateNameLine();
            if (templateNameContext.templateName().GetText().Equals(currentTarget().TemplateName))
            {
                return Visit(context.templateBody());
            }
            return null;
        }

        public override List<string> VisitNormalBody([NotNull] LGFileParser.NormalBodyContext context)
        {
            return Visit(context.normalTemplateBody());
        }

        public override List<string> VisitNormalTemplateBody([NotNull] LGFileParser.NormalTemplateBodyContext context)
        {
            var result = new List<string>();

            foreach(var templateStr in context.normalTemplateString())
            {
                var item = Visit(templateStr);
                result.AddRange(item);
            }

            return result;
        }

        public override List<string> VisitConditionalBody([NotNull] LGFileParser.ConditionalBodyContext context)
        {
            var result = new List<string>();

            var caseRules = context.conditionalTemplateBody().caseRule();
            foreach (var caseRule in caseRules)
            {
                var conditionExpression = caseRule.caseCondition().EXPRESSION().GetText();
                var childConditionResult = AnalyzeExpression(conditionExpression);
                result.AddRange(childConditionResult);

                var childTemplateBodyResult = Visit(caseRule.normalTemplateBody());
                result.AddRange(childTemplateBodyResult);
            }

            if (context?.conditionalTemplateBody()?.defaultRule() != null)
            {
                var childDefaultRuleResult = Visit(context.conditionalTemplateBody().defaultRule().normalTemplateBody());
                result.AddRange(childDefaultRuleResult);
            }

            return result;
        }


        public override List<string> VisitNormalTemplateString([NotNull] LGFileParser.NormalTemplateStringContext context)
        {
            var result = new List<string>();
            foreach (ITerminalNode node in context.children)
            {
                switch (node.Symbol.Type)
                {
                    case LGFileParser.DASH:
                        break;
                    case LGFileParser.EXPRESSION:
                        result.AddRange(AnalyzeExpression(node.GetText()));
                        break;
                    case LGFileParser.TEMPLATE_REF:
                        result.AddRange(AnalyzeTemplateRef(node.GetText()));
                        break;
                    case LGFileLexer.MULTI_LINE_TEXT:
                        result.AddRange(AnalyzeMultiLineText(node.GetText()));
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private List<string> AnalyzeExpression(string exp)
        {
            exp = exp.TrimStart('{').TrimEnd('}');
            var term = ExpressionEngine.Parse(exp);
            return GetAnalyzersFromTerm(term);
        }


        private List<string> AnalyzeTemplateRef(string exp)
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

                
                var templateName = exp.Substring(0, argsStartPos);
                //TODO support parames,remove params variables

                return AnalyzeTemplate(templateName);

            }
            else
            {
                return AnalyzeTemplate(exp);
            }
        }

        private List<string> AnalyzeMultiLineText(string exp)
        {
            var result = new List<string>();
            exp = exp.Substring(3, exp.Length - 6); //remove ``` ```

            var matches = Regex.Matches(exp, @"@\{[^{}]+\}");
            foreach (Match matchItem in matches)
            {
                if (matchItem.Success)
                {
                    var value = matchItem.Value.Substring(1);// remove @

                    if (value.StartsWith("{[") && value.EndsWith("]}"))
                    {
                        result.AddRange(AnalyzeTemplateRef(value.Substring(2, value.Length - 4)));//[ ]
                    }
                    else
                    {
                        result.AddRange(AnalyzeExpression(value));//{ }
                    }
                }
            }

            return result;
        }
        
       
        private List<string> Distinct(List<string> input)
        {
            if (input == null || input.Count <= 1)
                return input;

            input = input.Select(u => GetRootVariable(u)).ToList();
            return input.Distinct().ToList();
        }

        private string GetRootVariable(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var dotIndex = input.IndexOf(".");
            if(dotIndex > 0) //if start with . , we use as a valid variable 
            {
                input = input.Substring(0, dotIndex);
            }

            var BracketIndex = input.IndexOf("[");
            if (BracketIndex >= 0) 
            {
                input = input.Substring(0, BracketIndex);
            }

            return input;
        }

        
        private List<string> GetAnalyzersFromTerm(Term term)
        {
            var result = new List<string>();

            var token = term.Token;
            var value = token.Value;

            // token value is identifier -> look up binding in scope
            var identifier = value as Lexer.Identifier;
            if (identifier != null)
            {
                return new List<string> { identifier.Name };
            }

            // otherwise token literal value should be evaluated to a constant value
            if (value != null)
            {
                return result;
            }

            // special handling for operators
            // 1. without eagerly evaluated operands, or
            // 2. require access to the environment
            switch (token.Input)
            {
                case ".":
                    {
                        return GetAnalyzersFromTerm(term.Terms[0]);
                    }
                case "[":
                    {
                        return GetAnalyzersFromTerm(term.Terms[0]);
                    }
                case "(":
                    {
                        var name = term.Terms[0].Token.Input;

                        if (name.Equals("."))
                        {
                            result.AddRange(GetAnalyzersFromTerm(term.Terms[0].Terms[0]));
                           
                            foreach(var item in term.Terms.Skip(1))
                            {
                                if (Context.TemplateContexts.ContainsKey(item.Token.Input))
                                {
                                    result.AddRange(AnalyzeTemplate(item.Token.Input));
                                }
                                else
                                {
                                    result.AddRange(GetAnalyzersFromTerm(item));
                                }
                                
                            }
                            return result;
                        }
                        else
                        {
                            foreach (var item in term.Terms.Skip(1))
                            {
                                if (Context.TemplateContexts.ContainsKey(item.Token.Input))
                                {
                                    result.AddRange(AnalyzeTemplate(item.Token.Input));
                                }
                                else
                                {
                                    result.AddRange(GetAnalyzersFromTerm(item));
                                }
                            }
                            return result;
                        }
                    }
            }

            // otherwise look in table for operators with eagerly evaluated operands
            var entry = term.Entry;
            if (entry != null)
            {
                foreach (var item in term.Terms)
                {
                    result.AddRange(GetAnalyzersFromTerm(item));
                }
                return result;
            }

            throw new NotImplementedException();
        }

        private List<string> ExtractParameters(string templateName)
        {
            bool hasParameters = Context.TemplateParameters.TryGetValue(templateName, out List<string> parameters);
            return hasParameters ? parameters : new List<string>();
        }



    }
}
