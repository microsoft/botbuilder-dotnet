// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
    /// <param name="sourceId">The id or path of source file.</param>
    /// <param name="resourceId">Resource id to resolve.</param>
    /// <returns>Resolved resource content and unique id.</returns>
    public delegate (string content, string id) ImportResolverDelegate(string sourceId, string resourceId);

    /// <summary>
    /// Parser to turn lg content into a <see cref="Templates"/>.
    /// </summary>
    public static class TemplatesParser
    {
        /// <summary>
        /// option regex.
        /// </summary>
        private static readonly Regex OptionRegex = new Regex(@"^> *!#(.*)$");

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="filePath"> absolut path of a LG file.</param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">expressionEngine Expression engine for evaluating expressions.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        public static Templates ParseFile(
            string filePath,
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            var fullPath = Path.GetFullPath(filePath.NormalizePath());
            var content = File.ReadAllText(fullPath);

            return InnerParseText(content, fullPath, importResolver, expressionParser);
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">expressionEngine parser engine for parsing expressions.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        public static Templates ParseText(
            string content,
            string id = "",
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null)
        {
            return InnerParseText(content, id, importResolver, expressionParser);
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/> based on the original LGFile.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="lg">original LGFile.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        public static Templates ParseTextWithRef(string content, Templates lg)
        {
            if (lg == null)
            {
                throw new ArgumentNullException(nameof(lg));
            }

            var id = "inline content";
            var newLG = new Templates(content: content, id: id, importResolver: lg.ImportResolver, options: lg.Options);
            try
            {
                newLG = new TemplatesTransformer(newLG).Transform(AntlrParseTemplates(content, id));
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
            catch (Exception err)
            {
                newLG.Diagnostics.Add(BuildDiagnostic(err.Message, source: id));
            }

            return newLG;
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">expressionEngine parser engine for parsing expressions.</param>
        /// <param name="cachedTemplates">give the file path and templates to avoid parsing and to improve performance.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        private static Templates InnerParseText(
            string content,
            string id = "",
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null,
            Dictionary<string, Templates> cachedTemplates = null)
        {
            if (id.Contains("sandwichTest.en-us.lg"))
            {
                var s = "s";
            }

            cachedTemplates = cachedTemplates ?? new Dictionary<string, Templates>();
            if (cachedTemplates.ContainsKey(id))
            {
                return cachedTemplates[id];
            }

            importResolver = importResolver ?? DefaultFileResolver;
            var lg = new Templates(content: content, id: id, importResolver: importResolver, expressionParser: expressionParser);

            try
            {
                lg = new TemplatesTransformer(lg).Transform(AntlrParseTemplates(content, id));
                lg.References = GetReferences(lg, cachedTemplates);
                new StaticChecker(lg).Check().ForEach(u => lg.Diagnostics.Add(u));
            }
            catch (TemplateException ex)
            {
                ex.Diagnostics.ToList().ForEach(u => lg.Diagnostics.Add(u));
            }
            catch (Exception err)
            {
                lg.Diagnostics.Add(BuildDiagnostic(err.Message, source: id));
            }

            return lg;
        }

        /// <summary>
        /// Default import resolver, using relative/absolute file path to access the file content.
        /// </summary>
        /// <param name="sourceId">default is file path.</param>
        /// <param name="resourceId">import path.</param>
        /// <returns>target content id.</returns>
        private static (string content, string id) DefaultFileResolver(string sourceId, string resourceId)
        {
            // import paths are in resource files which can be executed on multiple OS environments
            // normalize to map / & \ in importPath -> OSPath
            var importPath = resourceId.NormalizePath();

            if (!Path.IsPathRooted(importPath))
            {
                // get full path for importPath relative to path which is doing the import.
                importPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceId), resourceId));
            }

            return (File.ReadAllText(importPath), importPath);
        }

        private static Diagnostic BuildDiagnostic(string errorMessage, ParserRuleContext context = null, string source = null)
        {
            errorMessage = TemplateErrors.StaticFailure + "- " + errorMessage;
            var startPosition = context == null ? new Position(0, 0) : new Position(context.Start.Line, context.Start.Column);
            var stopPosition = context == null ? new Position(0, 0) : new Position(context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
            return new Diagnostic(new Range(startPosition, stopPosition), errorMessage, source: source);
        }

        /// <summary>
        /// Get parsed tree nodes from text by antlr4 engine.
        /// </summary>
        /// <param name="text">Original text which will be parsed.</param>
        /// <returns>Parsed tree nodes.</returns>
        private static IParseTree AntlrParseTemplates(string text, string id)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var input = new AntlrInputStream(text);
            var lexer = new LGFileLexer(input);
            lexer.RemoveErrorListeners();

            var tokens = new CommonTokenStream(lexer);
            var parser = new LGFileParser(tokens);
            parser.RemoveErrorListeners();
            var listener = new ErrorListener(id);

            parser.AddErrorListener(listener);
            parser.BuildParseTree = true;

            return parser.file();
        }

        private static IList<Templates> GetReferences(Templates file, Dictionary<string, Templates> cachedTemplates = null)
        {
            var resourcesFound = new HashSet<Templates>();
            ResolveImportResources(file, resourcesFound, cachedTemplates ?? new Dictionary<string, Templates>());

            resourcesFound.Remove(file);
            return resourcesFound.ToList();
        }

        private static void ResolveImportResources(Templates start, HashSet<Templates> resourcesFound, Dictionary<string, Templates> cachedTemplates)
        {
            var resourceIds = start.Imports.Select(lg => lg.Id);
            resourcesFound.Add(start);

            foreach (var id in resourceIds)
            {
                var (content, path) = start.ImportResolver(start.Id, id);
                if (resourcesFound.All(u => u.Id != path))
                {
                    Templates childResource;
                    if (cachedTemplates.ContainsKey(path))
                    {
                        childResource = cachedTemplates[path];
                    }
                    else
                    {
                        childResource = InnerParseText(content, path, start.ImportResolver, start.ExpressionParser, cachedTemplates);
                        cachedTemplates.Add(path, childResource);
                    }

                    ResolveImportResources(childResource, resourcesFound, cachedTemplates);
                }
            }
        }

        private class TemplatesTransformer : LGFileParserBaseVisitor<object>
        {
            private static readonly Regex IdentifierRegex = new Regex(@"^[0-9a-zA-Z_]+$");
            private readonly Templates templates;

            public TemplatesTransformer(Templates templates)
            {
                this.templates = templates;
            }

            public Templates Transform(IParseTree parseTree)
            {
                Visit(parseTree);
                return this.templates;
            }

            public override object VisitErrorDefinition([NotNull] LGFileParser.ErrorDefinitionContext context)
            {
                var lineContent = context.INVALID_LINE().GetText();
                if (!string.IsNullOrWhiteSpace(lineContent))
                {
                    var errorMessage = TemplateErrors.StaticFailure + "- " + context.INVALID_LINE().GetText();
                    var startPosition = new Position(context.Start.Line, context.Start.Column);
                    var stopPosition = new Position(context.Start.Line, context.Stop.Column + context.Stop.Text.Length);
                    var diagnostic = new Diagnostic(new Range(startPosition, stopPosition), errorMessage, source: templates.Id);
                    this.templates.Diagnostics.Add(diagnostic);
                }

                return null;
            }

            public override object VisitImportDefinition([NotNull] LGFileParser.ImportDefinitionContext context)
            {
                var importStr = context.IMPORT().GetText();
                var openSquareBracketIndex = importStr.IndexOf('[');
                var closeSquareBracketIndex = importStr.IndexOf(']');
                var description = importStr.Substring(openSquareBracketIndex + 1, closeSquareBracketIndex - openSquareBracketIndex - 1);

                var lastOpenBracketIndex = importStr.LastIndexOf('(');
                var lastCloseBracketIndex = importStr.LastIndexOf(')');
                var id = importStr.Substring(lastOpenBracketIndex + 1, lastCloseBracketIndex - lastOpenBracketIndex - 1);

                var import = new TemplateImport(description, id, this.templates.Id);
                this.templates.Imports.Add(import);
                return null;
            }

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
                    this.templates.Options.Add(result);
                }

                return null;
            }

            public override object VisitTemplateDefinition([NotNull] LGFileParser.TemplateDefinitionContext context)
            {
                var startLine = context.Start.Line;
                var stopLine = context.Stop.Line;

                var file = context.Parent.Parent as LGFileParser.FileContext;
                var isLast = file.paragraph().Select(u => u.templateDefinition()).Where(u => u != null).Last() == context;

                var templateNameLine = context.templateNameLine().TEMPLATE_NAME_LINE().GetText();
                var hashIndex = templateNameLine.IndexOf('#');
                templateNameLine = templateNameLine.Substring(hashIndex + 1).Trim();

                var templateName = templateNameLine;
                var parameters = new List<string>();
                var leftBracketIndex = templateNameLine.IndexOf("(");
                if (leftBracketIndex >= 0 && templateNameLine.EndsWith(")"))
                {
                    templateName = templateNameLine.Substring(0, leftBracketIndex).Trim();
                    var paramString = templateNameLine.Substring(leftBracketIndex + 1, templateNameLine.Length - leftBracketIndex - 2);
                    if (!string.IsNullOrWhiteSpace(paramString))
                    {
                        parameters = paramString.Split(',').Select(u => u.Trim()).ToList();
                    }
                }

                if (this.templates.Any(u => u.Name == templateName))
                {
                    var diagnostic = BuildTemplateDiagnostic(TemplateErrors.DuplicatedTemplateInSameTemplate(templateName), context.templateNameLine());
                    this.templates.Diagnostics.Add(diagnostic);
                }
                else
                {
                    var templateBody = context.templateBody().GetText();

                    if (!isLast && templateBody.EndsWith("\n"))
                    {
                        templateBody = templateBody.Substring(0, templateBody.Length - 1);
                        if (templateBody.EndsWith("\r"))
                        {
                            templateBody = templateBody.Substring(0, templateBody.Length - 1);
                        }
                    }

                    var template = new Template(templateName, parameters, templateBody, startLine, stopLine, this.templates.Id);

                    // check template name
                    var functionNameSplitDot = templateName.Split('.');
                    foreach (var id in functionNameSplitDot)
                    {
                        if (!IdentifierRegex.IsMatch(id))
                        {
                            var diagnostic = BuildTemplateDiagnostic(TemplateErrors.InvalidTemplateName, context.templateNameLine());
                            this.templates.Diagnostics.Add(diagnostic);
                        }
                    }

                    // check template parameters

                    foreach (var parameter in parameters)
                    {
                        if (!IdentifierRegex.IsMatch(parameter))
                        {
                            var diagnostic = BuildTemplateDiagnostic(TemplateErrors.InvalidTemplateName, context.templateNameLine());
                            this.templates.Diagnostics.Add(diagnostic);
                        }
                    }

                    // check template body
                    if (string.IsNullOrWhiteSpace(templateBody))
                    {
                        var diagnostic = BuildTemplateDiagnostic(TemplateErrors.NoTemplateBody(templateName), context.templateBody(), DiagnosticSeverity.Warning);
                        this.templates.Diagnostics.Add(diagnostic);
                    }
                    else
                    {
                        try
                        {
                            var parseTree = AntlrParseTemplate(templateBody, startLine);
                            template.TemplateBodyParseTree = parseTree;
                        }
                        catch (TemplateException e)
                        {
                            e.Diagnostics.ToList().ForEach(u => this.templates.Diagnostics.Add(u));
                        }
                    }

                    this.templates.Add(template);
                }

                return null;
            }

            private LGTemplateParser.TemplateBodyContext AntlrParseTemplate(string templateBody, int lineOffset)
            {
                var input = new AntlrInputStream(templateBody);
                var lexer = new LGTemplateLexer(input);
                lexer.RemoveErrorListeners();

                var tokens = new CommonTokenStream(lexer);
                var parser = new LGTemplateParser(tokens);
                parser.RemoveErrorListeners();
                var listener = new ErrorListener(this.templates.Id, lineOffset);

                parser.AddErrorListener(listener);
                parser.BuildParseTree = true;

                return parser.templateBody();
            }

            private Diagnostic BuildTemplateDiagnostic(string errorMessage, ParserRuleContext context, DiagnosticSeverity severity = DiagnosticSeverity.Error)
            {
                var startPosition = new Position(context.Start.Line, context.Start.Column);
                var stopPosition = new Position(context.Stop.Line, context.Stop.Column + context.Stop.Text.Length);
                return new Diagnostic(new Range(startPosition, stopPosition), errorMessage, severity, this.templates.Id);
            }
        }
    }
}
