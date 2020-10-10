﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AdaptiveExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Delegate for resolving resource id of imported lg file.
    /// </summary>
    /// <param name="resource">Original resource.</param>
    /// <param name="resourceId">Resource id to resolve.</param>
    /// <returns>Resolved resource.</returns>
    public delegate LGResource ImportResolverDelegate(LGResource resource, string resourceId);

    /// <summary>
    /// Parser to turn lg content into a <see cref="Templates"/>.
    /// </summary>
    internal static class TemplatesParser
    {
        /// <summary>
        /// Inline text id.
        /// </summary>
        public const string InlineContentId = "inline content";

        /// <summary>
        /// option regex.
        /// </summary>
        public static readonly Regex OptionRegex = new Regex(@">\s*!#(.*)");

        /// <summary>
        /// Import regex.
        /// </summary>
        public static readonly Regex ImportRegex = new Regex(@"\[([^]]*)\]\(([^)]*)\)");

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="filePath">Absolut path of a LG file.</param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">Expression parser for parsing expressions.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        public static Templates ParseFile(
            string filePath,
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            var fullPath = Path.GetFullPath(filePath.NormalizePath());
            var content = File.ReadAllText(fullPath);

            var resource = new LGResource(fullPath, fullPath, content);
            return ParseResource(resource, importResolver, expressionParser);
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">Id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">Expression parser for parsing expressions.</param>
        /// <returns>New <see cref="Templates"/> entity.</returns>
        [Obsolete("This method will soon be deprecated. Use ParseResource instead.")]
        public static Templates ParseText(
            string content,
            string id = "",
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            var resource = new LGResource(id, id, content);
            return ParseResource(resource, importResolver, expressionParser);
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/> based on the original LGFile.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="lg">Original LGFile.</param>
        /// <returns>New <see cref="Templates"/> entity.</returns>
        public static Templates ParseTextWithRef(string content, Templates lg)
        {
            if (lg == null)
            {
                throw new ArgumentNullException(nameof(lg));
            }

            var newLG = new Templates(content: content, id: InlineContentId, source: InlineContentId, importResolver: lg.ImportResolver, options: lg.Options);
            try
            {
                var resource = new LGResource(InlineContentId, InlineContentId, content);
                newLG = new TemplatesTransformer(newLG).Transform(AntlrParseTemplates(resource));
                newLG.References = GetReferences(newLG)
                        .Union(lg.References)
                        .Union(new List<Templates> { lg })
                        .ToList();

                new StaticChecker(newLG).Check().ForEach(u => newLG.Diagnostics.Add(u));
            }
            catch (TemplateException ex)
            {
                ex.Diagnostics.ToList().ForEach(u => newLG.Diagnostics.Add(u));
            }

            return newLG;
        }

        /// <summary>
        /// Parse LG content and achieve the AST.
        /// </summary>
        /// <param name="resource">LG resource.</param>
        /// <returns>The abstract syntax tree of lg file.</returns>
        public static IParseTree AntlrParseTemplates(LGResource resource)
        {
            if (string.IsNullOrEmpty(resource.Content))
            {
                return null;
            }

            var input = new AntlrInputStream(resource.Content);
            var lexer = new LGFileLexer(input);
            lexer.RemoveErrorListeners();

            var tokens = new CommonTokenStream(lexer);
            var parser = new LGFileParser(tokens);
            parser.RemoveErrorListeners();
            var listener = new ErrorListener(resource.FullName);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.file();
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="resource">LG resource.</param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">Expression parser for parsing expressions.</param>
        /// <param name="cachedTemplates">Give the file path and templates to avoid parsing and to improve performance.</param>
        /// <param name="history">Traceability chain of the visited templates.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        public static Templates ParseResource(
            LGResource resource,
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null,
            Dictionary<string, Templates> cachedTemplates = null,
            Stack<Templates> history = null)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            cachedTemplates = cachedTemplates ?? new Dictionary<string, Templates>();
            history = history ?? new Stack<Templates>();
            if (cachedTemplates.ContainsKey(resource.Id))
            {
                return cachedTemplates[resource.Id];
            }

            importResolver = importResolver ?? DefaultFileResolver;
            var lg = new Templates(content: resource.Content, id: resource.Id, source: resource.FullName, importResolver: importResolver, expressionParser: expressionParser);

            try
            {
                lg = new TemplatesTransformer(lg).Transform(AntlrParseTemplates(resource));
                lg.References = GetReferences(lg, cachedTemplates, history);
                new StaticChecker(lg).Check().ForEach(u => lg.Diagnostics.Add(u));
            }
            catch (TemplateException ex)
            {
                ex.Diagnostics.ToList().ForEach(u => lg.Diagnostics.Add(u));
            }

            return lg;
        }

        /// <summary>
        /// Default import resolver, using relative/absolute file path to access the file content.
        /// </summary>
        /// <param name="resource">Original Resource.</param>
        /// <param name="resourceId">Import path.</param>
        /// <returns>Target content id.</returns>
        private static LGResource DefaultFileResolver(LGResource resource, string resourceId)
        {
            // import paths are in resource files which can be executed on multiple OS environments
            // normalize to map / & \ in importPath -> OSPath
            var importPath = resourceId.NormalizePath();

            if (!Path.IsPathRooted(importPath))
            {
                // get full path for importPath relative to path which is doing the import.
                importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(resource.FullName), resourceId));
            }

            return new LGResource(importPath, importPath, File.ReadAllText(importPath));
        }

        private static IList<Templates> GetReferences(Templates file, Dictionary<string, Templates> cachedTemplates = null, Stack<Templates> history = null)
        {
            var resourcesFound = new HashSet<Templates>();
            ResolveImportResources(file, resourcesFound, cachedTemplates ?? new Dictionary<string, Templates>(), history ?? new Stack<Templates>());

            resourcesFound.Remove(file);
            return resourcesFound.ToList();
        }

        private static void ResolveImportResources(Templates start, HashSet<Templates> resourcesFound, Dictionary<string, Templates> cachedTemplates, Stack<Templates> history)
        {
            resourcesFound.Add(start);
            history.Push(start);
            foreach (var import in start.Imports)
            {
                LGResource resource;
                try
                {
                    var originalResource = new LGResource(start.Id, start.Source, start.Content);
                    resource = start.ImportResolver(originalResource, import.Id);
                }
                catch (Exception e)
                {
                    var diagnostic = new Diagnostic(import.SourceRange.Range, e.Message, DiagnosticSeverity.Error, start.Source);
                    throw new TemplateException(e.Message, new List<Diagnostic>() { diagnostic });
                }

                // Cycle reference would throw exception to avoid infinite Loop.
                // Import self is allowed, and would ignore it.
                if (history.Peek().Id != resource.Id && history.Any(u => u.Id == resource.Id))
                {
                    var errorMsg = $"{TemplateErrors.LoopDetected} {resource.Id} => {start.Id}";
                    var diagnostic = new Diagnostic(import.SourceRange.Range, errorMsg, DiagnosticSeverity.Error, start.Source);
                    throw new TemplateException(errorMsg, new List<Diagnostic>() { diagnostic });
                }

                if (resourcesFound.All(u => u.Id != resource.Id))
                {
                    Templates childResource;
                    if (cachedTemplates.ContainsKey(resource.Id))
                    {
                        childResource = cachedTemplates[resource.Id];
                    }
                    else
                    {
                        childResource = ParseResource(resource, start.ImportResolver, start.ExpressionParser, cachedTemplates, history);
                        cachedTemplates.Add(resource.Id, childResource);
                    }

                    ResolveImportResources(childResource, resourcesFound, cachedTemplates, history);
                }
            }

            history.Pop();
        }

        /// <summary>
        /// Templates transformer. Fulfill more details and body context into the templates object.
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible (we can't move this nested type without breaking binary compat)
        public class TemplatesTransformer : LGFileParserBaseVisitor<object>
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private static readonly Regex IdentifierRegex = new Regex(@"^[0-9a-zA-Z_]+$");
            private static readonly Regex TemplateNamePartRegex = new Regex(@"^[a-zA-Z_][0-9a-zA-Z_]*$");
            private readonly Templates _templates;

            /// <summary>
            /// Initializes a new instance of the <see cref="TemplatesTransformer"/> class.
            /// </summary>
            /// <param name="templates">Templates to transform.</param>
            public TemplatesTransformer(Templates templates)
            {
                _templates = templates;
            }

            /// <summary>
            /// Transform the parse tree into templates.
            /// </summary>
            /// <param name="parseTree">Input abstract syntax tree.</param>
            /// <returns>Templates.</returns>
            public Templates Transform(IParseTree parseTree)
            {
                if (parseTree != null)
                {
                    Visit(parseTree);
                }

                for (var i = 0; i < _templates.Count - 1; i++)
                {
                    _templates[i].Body = RemoveTrailingNewline(_templates[i].Body);
                }

                return _templates;
            }

            /// <inheritdoc/>
            public override object VisitErrorDefinition([NotNull] LGFileParser.ErrorDefinitionContext context)
            {
                var lineContent = context.INVALID_LINE().GetText();
                if (!string.IsNullOrWhiteSpace(lineContent))
                {
                    _templates.Diagnostics.Add(BuildTemplatesDiagnostic(TemplateErrors.SyntaxError($"Unexpected content: '{lineContent}'"), context));
                }

                return null;
            }

            /// <inheritdoc/>
            public override object VisitImportDefinition([NotNull] LGFileParser.ImportDefinitionContext context)
            {
                var importStr = context.IMPORT().GetText();

                var matchResult = ImportRegex.Match(importStr);
                if (matchResult.Success && matchResult.Groups.Count == 3)
                {
                    var description = matchResult.Groups[1].Value?.Trim();
                    var id = matchResult.Groups[2].Value?.Trim();

                    var sourceRange = new SourceRange(context, _templates.Source);
                    var import = new TemplateImport(description, id, sourceRange);
                    _templates.Imports.Add(import);
                }

                return null;
            }

            /// <inheritdoc/>
            public override object VisitOptionDefinition([NotNull] LGFileParser.OptionDefinitionContext context)
            {
                var originalText = context.OPTION().GetText();
                var result = string.Empty;
                if (!string.IsNullOrWhiteSpace(originalText))
                {
                    var matchResult = OptionRegex.Match(originalText);
                    if (matchResult.Success && matchResult.Groups.Count == 2)
                    {
                        result = matchResult.Groups[1].Value?.Trim();
                    }
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    _templates.Options.Add(result);
                }

                return null;
            }

            /// <inheritdoc/>
            public override object VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
            {
                var startLine = context.Start.Line;
                var stopLine = context.Stop.Line;

                var templateNameLine = context.templateNameLine().TEMPLATE_NAME_LINE().GetText();
                var (templateName, parameters) = ExtractTemplateNameLine(templateNameLine);

                if (_templates.Any(u => u.Name == templateName))
                {
                    var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.DuplicatedTemplateInSameTemplate(templateName), context.templateNameLine());
                    _templates.Diagnostics.Add(diagnostic);
                }
                else
                {
                    var templateBody = context.templateBody().GetText();

                    var sourceRange = new SourceRange(context, _templates.Source);
                    var template = new Template(templateName, parameters, templateBody, sourceRange);

                    CheckTemplateName(templateName, context.templateNameLine());
                    CheckTemplateParameters(parameters, context.templateNameLine());
                    CheckTemplateBody(template, context.templateBody(), startLine);

                    _templates.Add(template);
                }

                return null;
            }

            private LGTemplateParser.BodyContext CheckTemplateBody(Template template, LGFileParser.TemplateBodyContext context, int startLine)
            {
                if (string.IsNullOrWhiteSpace(template.Body))
                {
                    var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.NoTemplateBody(template.Name), context, DiagnosticSeverity.Warning);
                    _templates.Diagnostics.Add(diagnostic);
                }
                else
                {
                    try
                    {
                        var templateBodyContext = AntlrParseTemplate(template.Body, startLine);
                        if (templateBodyContext != null)
                        {
                            template.TemplateBodyParseTree = templateBodyContext;
                            new TemplateBodyTransformer(template).Transform();
                        }
                    }
                    catch (TemplateException e)
                    {
                        e.Diagnostics.ToList().ForEach(u => _templates.Diagnostics.Add(u));
                    }
                }

                return null;
            }

            private void CheckTemplateParameters(List<string> parameters, LGFileParser.TemplateNameLineContext context)
            {
                foreach (var parameter in parameters)
                {
                    if (!IdentifierRegex.IsMatch(parameter))
                    {
                        var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.InvalidParameter(parameter), context);
                        _templates.Diagnostics.Add(diagnostic);
                    }
                }
            }

            private void CheckTemplateName(string templateName, ParserRuleContext context)
            {
                var functionNameSplitDot = templateName.Split('.');
                foreach (var id in functionNameSplitDot)
                {
                    if (!TemplateNamePartRegex.IsMatch(id))
                    {
                        var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.InvalidTemplateName(templateName), context);
                        _templates.Diagnostics.Add(diagnostic);
                        break;
                    }
                }
            }

            private (string templateName, List<string> parameters) ExtractTemplateNameLine(string templateNameLine)
            {
                var hashIndex = templateNameLine.IndexOf('#');

                templateNameLine = templateNameLine.Substring(hashIndex + 1).Trim();

                var templateName = templateNameLine;
                var parameters = new List<string>();
                var leftBracketIndex = templateNameLine.IndexOf("(", StringComparison.Ordinal);
                if (leftBracketIndex >= 0 && templateNameLine.EndsWith(")", StringComparison.Ordinal))
                {
                    templateName = templateNameLine.Substring(0, leftBracketIndex).Trim();
                    var paramString = templateNameLine.Substring(leftBracketIndex + 1, templateNameLine.Length - leftBracketIndex - 2);
                    if (!string.IsNullOrWhiteSpace(paramString))
                    {
                        parameters = paramString.Split(',').Select(u => u.Trim()).ToList();
                    }
                }

                return (templateName, parameters);
            }

            private string RemoveTrailingNewline(string line)
            {
                // remove the end newline of middle template.
                var result = line;

                if (result.EndsWith("\n", StringComparison.Ordinal))
                {
                    result = result.Substring(0, result.Length - 1);
                    if (result.EndsWith("\r", StringComparison.Ordinal))
                    {
                        result = result.Substring(0, result.Length - 1);
                    }
                }

                return result;
            }

            private LGTemplateParser.BodyContext AntlrParseTemplate(string templateBody, int lineOffset)
            {
                var input = new AntlrInputStream(templateBody);
                var lexer = new LGTemplateLexer(input);
                lexer.RemoveErrorListeners();

                var tokens = new CommonTokenStream(lexer);
                var parser = new LGTemplateParser(tokens);
                parser.RemoveErrorListeners();
                var listener = new ErrorListener(_templates.Source, lineOffset);

                parser.AddErrorListener(listener);
                parser.BuildParseTree = true;

                return parser.context().body();
            }

            private Diagnostic BuildTemplatesDiagnostic(string errorMessage, ParserRuleContext context, DiagnosticSeverity severity = DiagnosticSeverity.Error)
            {
                return new Diagnostic(context.ConvertToRange(), errorMessage, severity, _templates.Source);
            }
        }

        private class TemplateBodyTransformer : LGTemplateParserBaseVisitor<object>
        {
            private readonly Template _template;

            public TemplateBodyTransformer(Template template)
            {
                this._template = template;
            }

            public void Transform()
            {
                Visit(_template.TemplateBodyParseTree);
            }

            public override object VisitNormalTemplateBody([NotNull] LGTemplateParser.NormalTemplateBodyContext context)
            {
                foreach (var templateStr in context.templateString())
                {
                    var errorTemplateStr = templateStr.errorTemplateString();
                    if (errorTemplateStr == null)
                    {
                        return Visit(templateStr.normalTemplateString());
                    }
                }

                return null;
            }

            public override object VisitStructuredTemplateBody([NotNull] LGTemplateParser.StructuredTemplateBodyContext context)
            {
                if (context.structuredBodyNameLine().errorStructuredName() == null
                    && context.structuredBodyEndLine() != null
                    && (context.errorStructureLine() == null || context.errorStructureLine().Length == 0)
                    && (context.structuredBodyContentLine() != null && context.structuredBodyContentLine().Length > 0))
                {
                    var bodys = context.structuredBodyContentLine();
                    foreach (var body in bodys)
                    {
                        if (body.expressionInStructure() != null)
                        {
                            FillInExpression(body.expressionInStructure());
                        }
                        else
                        {
                            // KeyValueStructuredLine
                            var structureValues = body.keyValueStructureLine().keyValueStructureValue();
                            foreach (var structureValue in structureValues)
                            {
                                foreach (var expression in structureValue.expressionInStructure())
                                {
                                    FillInExpression(expression);
                                }
                            }
                        }
                    }
                }

                return null;
            }

            public override object VisitIfElseBody([NotNull] LGTemplateParser.IfElseBodyContext context)
            {
                var ifRules = context.ifElseTemplateBody().ifConditionRule();
                for (var idx = 0; idx < ifRules.Length; idx++)
                {
                    var conditionNode = ifRules[idx].ifCondition();
                    var ifExpr = conditionNode.IF() != null;
                    var elseIfExpr = conditionNode.ELSEIF() != null;
                    var elseExpr = conditionNode.ELSE() != null;

                    var node = ifExpr ? conditionNode.IF() :
                               elseIfExpr ? conditionNode.ELSEIF() :
                               conditionNode.ELSE();

                    if (node.GetText().Count(u => u == ' ') > 1
                        || (idx > 0 && ifExpr)
                        || (idx > 0 && idx < ifRules.Length - 1 && !elseIfExpr))
                    {
                        return null;
                    }

                    if (!elseExpr && (ifRules[idx].ifCondition().expression().Length == 1))
                    {
                        FillInExpression(conditionNode.expression(0));
                    }

                    if (ifRules[idx].normalTemplateBody() != null)
                    {
                        Visit(ifRules[idx].normalTemplateBody());
                    }
                }

                return null;
            }

            public override object VisitSwitchCaseBody([NotNull] LGTemplateParser.SwitchCaseBodyContext context)
            {
                var switchCaseRules = context.switchCaseTemplateBody().switchCaseRule();
                var length = switchCaseRules.Length;
                for (var idx = 0; idx < length; idx++)
                {
                    var switchCaseNode = switchCaseRules[idx].switchCaseStat();
                    var switchExpr = switchCaseNode.SWITCH() != null;
                    var caseExpr = switchCaseNode.CASE() != null;
                    var defaultExpr = switchCaseNode.DEFAULT() != null;
                    var node = switchExpr ? switchCaseNode.SWITCH() :
                               caseExpr ? switchCaseNode.CASE() :
                               switchCaseNode.DEFAULT();

                    if (node.GetText().Count(u => u == ' ') > 1
                        || (idx == 0 && !switchExpr)
                        || (idx > 0 && switchExpr)
                        || (idx > 0 && idx < length - 1 && !caseExpr))
                    {
                        return null;
                    }

                    if ((switchExpr || caseExpr) && switchCaseNode.expression().Length == 1)
                    {
                        FillInExpression(switchCaseNode.expression(0));
                    }

                    if ((caseExpr || defaultExpr) && switchCaseRules[idx].normalTemplateBody() != null)
                    {
                        Visit(switchCaseRules[idx].normalTemplateBody());
                    }
                }

                return null;
            }

            public override object VisitNormalTemplateString([NotNull] LGTemplateParser.NormalTemplateStringContext context)
            {
                foreach (var expression in context.expression())
                {
                    FillInExpression(expression);
                }

                return null;
            }

            private void FillInExpression(ParserRuleContext expressionContext)
            {
                if (expressionContext == null)
                {
                    return;
                }

                var exp = expressionContext.GetText().TrimExpression();

                var lineOffset = this._template.SourceRange.Range.Start.Line;
                var sourceRange = new SourceRange(expressionContext, _template.SourceRange.Source, lineOffset);
                var expressionRef = new ExpressionRef(exp, sourceRange);
                _template.Expressions.Add(expressionRef);
            }
        }
    }
}
