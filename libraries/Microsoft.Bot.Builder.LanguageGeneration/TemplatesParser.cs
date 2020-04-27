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

            return InnerParseText(content, fullPath, importResolver, expressionParser);
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">Id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">Expression parser for parsing expressions.</param>
        /// <returns>New <see cref="Templates"/> entity.</returns>
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
        /// <param name="lg">Original LGFile.</param>
        /// <returns>New <see cref="Templates"/> entity.</returns>
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

            return newLG;
        }

        /// <summary>
        /// Parser to turn lg content into a <see cref="Templates"/>.
        /// </summary>
        /// <param name="content">Text content contains lg templates.</param>
        /// <param name="id">Id is the identifier of content. If importResolver is null, id must be a full path string. </param>
        /// <param name="importResolver">Resolver to resolve LG import id to template text.</param>
        /// <param name="expressionParser">Expression parser for parsing expressions.</param>
        /// <param name="cachedTemplates">Give the file path and templates to avoid parsing and to improve performance.</param>
        /// <returns>new <see cref="Templates"/> entity.</returns>
        private static Templates InnerParseText(
            string content,
            string id = "",
            ImportResolverDelegate importResolver = null,
            ExpressionParser expressionParser = null,
            Dictionary<string, Templates> cachedTemplates = null)
        {
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

            return lg;
        }

        /// <summary>
        /// Default import resolver, using relative/absolute file path to access the file content.
        /// </summary>
        /// <param name="sourceId">Default is file path.</param>
        /// <param name="resourceId">Import path.</param>
        /// <returns>Target content id.</returns>
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
            resourcesFound.Add(start);

            foreach (var import in start.Imports)
            {
                string content;
                string path;
                try
                {
                    (content, path) = start.ImportResolver(start.Id, import.Id);
                }
                catch (Exception e)
                {
                    var diagnostic = new Diagnostic(import.SourceRange.ParseTree.ConvertToRange(), e.Message, DiagnosticSeverity.Error, start.Id);
                    throw new TemplateException(e.Message, new List<Diagnostic>() { diagnostic });
                }

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
                if (parseTree != null)
                {
                    Visit(parseTree);
                }

                return this.templates;
            }

            public override object VisitErrorDefinition([NotNull] LGFileParser.ErrorDefinitionContext context)
            {
                var lineContent = context.INVALID_LINE().GetText();
                if (!string.IsNullOrWhiteSpace(lineContent))
                {
                    this.templates.Diagnostics.Add(BuildTemplatesDiagnostic(TemplateErrors.SyntaxError, context));
                }

                return null;
            }

            public override object VisitImportDefinition([NotNull] LGFileParser.ImportDefinitionContext context)
            {
                var importStr = context.IMPORT().GetText();

                var matchResult = ImportRegex.Match(importStr);
                if (matchResult.Success && matchResult.Groups.Count == 3)
                {
                    var description = matchResult.Groups[1].Value?.Trim();
                    var id = matchResult.Groups[2].Value?.Trim();

                    var sourceRange = new SourceRange(context, this.templates.Id);
                    var import = new TemplateImport(description, id, sourceRange);
                    this.templates.Imports.Add(import);
                }

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

                var templateNameLine = context.templateNameLine().TEMPLATE_NAME_LINE().GetText();
                var (templateName, parameters) = ExtractTemplateNameLine(templateNameLine);

                if (this.templates.Any(u => u.Name == templateName))
                {
                    var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.DuplicatedTemplateInSameTemplate(templateName), context.templateNameLine());
                    this.templates.Diagnostics.Add(diagnostic);
                }
                else
                {
                    var templateBody = context.templateBody().GetText();
                    var file = context.Parent.Parent as LGFileParser.FileContext;
                    var isLastTemplate = file.paragraph().Select(u => u.templateDefinition()).Where(u => u != null).Last() == context;
                    if (!isLastTemplate)
                    {
                        templateBody = RemoveTrailingNewline(templateBody);
                    }

                    var sourceRange = new SourceRange(context, this.templates.Id);
                    var template = new Template(templateName, parameters, templateBody, sourceRange);

                    CheckTemplateName(templateName, context.templateNameLine());
                    CheckTemplateParameters(parameters, context.templateNameLine());
                    template.TemplateBodyParseTree = CheckTemplateBody(templateName, templateBody, context.templateBody(), startLine);

                    this.templates.Add(template);
                }

                return null;
            }

            private LGTemplateParser.BodyContext CheckTemplateBody(string templateName, string templateBody, LGFileParser.TemplateBodyContext context, int startLine)
            {
                if (string.IsNullOrWhiteSpace(templateBody))
                {
                    var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.NoTemplateBody(templateName), context, DiagnosticSeverity.Warning);
                    this.templates.Diagnostics.Add(diagnostic);
                }
                else
                {
                    try
                    {
                        return AntlrParseTemplate(templateBody, startLine);
                    }
                    catch (TemplateException e)
                    {
                        e.Diagnostics.ToList().ForEach(u => this.templates.Diagnostics.Add(u));
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
                        var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.InvalidTemplateName, context);
                        this.templates.Diagnostics.Add(diagnostic);
                    }
                }
            }

            private void CheckTemplateName(string templateName, ParserRuleContext context)
            {
                var functionNameSplitDot = templateName.Split('.');
                foreach (var id in functionNameSplitDot)
                {
                    if (!IdentifierRegex.IsMatch(id))
                    {
                        var diagnostic = BuildTemplatesDiagnostic(TemplateErrors.InvalidTemplateName, context);
                        this.templates.Diagnostics.Add(diagnostic);
                    }
                }
            }

            private (string templateName, List<string> parameters) ExtractTemplateNameLine(string templateNameLine)
            {
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
                var listener = new ErrorListener(this.templates.Id, lineOffset);

                parser.AddErrorListener(listener);
                parser.BuildParseTree = true;

                return parser.context().body();
            }

            private Diagnostic BuildTemplatesDiagnostic(string errorMessage, ParserRuleContext context, DiagnosticSeverity severity = DiagnosticSeverity.Error)
            {
                return new Diagnostic(context.ConvertToRange(), errorMessage, severity, this.templates.Id);
            }
        }
    }
}
