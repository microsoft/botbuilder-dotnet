using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Templates CRUD operator class.
    /// </summary>
    public class TemplatesOperators
    {
        private Templates _templates;
        private readonly string _newLine = Environment.NewLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatesOperators"/> class.
        /// </summary>
        /// <param name="templates">Original <see cref="Templates"/>.</param>
        public TemplatesOperators(Templates templates)
        {
            _templates = templates;
        }

        /// <summary>
        /// Updates an existing template in current Templates instance.
        /// </summary>
        /// <param name="templateName">Original template name. The only id of a template.</param>
        /// <param name="newTemplateName">New template Name.</param>
        /// <param name="parameters">New params.</param>
        /// <param name="templateBody">New template body.</param>
        /// <returns>Updated LG file.</returns>
        public Templates UpdateTemplate(string templateName, string newTemplateName, List<string> parameters, string templateBody)
        {
            var template = _templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                ClearDiagnostics();

                var templateNameLine = BuildTemplateNameLine(newTemplateName, parameters);
                var newTemplateBody = ConvertTemplateBody(templateBody);
                var content = $"{templateNameLine}{_newLine}{newTemplateBody}";

                // update content
                _templates.Content = ReplaceRangeContent(
                    _templates.Content,
                    template.SourceRange.Range.Start.Line - 1,
                    template.SourceRange.Range.End.Line - 1,
                    content);

                var updatedTemplates = new Templates(content: string.Empty, id: _templates.Id, importResolver: _templates.ImportResolver, expressionParser: _templates.ExpressionParser);
                updatedTemplates = new TemplatesParser.TemplatesTransformer(updatedTemplates).Transform(TemplatesParser.AntlrParseTemplates(content, _templates.Id));

                var originStartLine = template.SourceRange.Range.Start.Line - 1;
                AppendDiagnosticsWithOffset(updatedTemplates.Diagnostics, originStartLine);

                var newTemplate = updatedTemplates.FirstOrDefault();
                if (newTemplate != null)
                {
                    AdjustRangeForUpdateTemplate(template, newTemplate);
                    new StaticChecker(_templates).Check().ForEach(u => _templates.Diagnostics.Add(u));
                }
            }

            return _templates;
        }

        /// <summary>
        /// Adds a new template and returns the updated Templates instance.
        /// </summary>
        /// <param name="templateName">New template name.</param>
        /// <param name="parameters">New params.</param>
        /// <param name="templateBody">New template body.</param>
        /// <returns>Updated LG file.</returns>
        public Templates AddTemplate(string templateName, List<string> parameters, string templateBody)
        {
            var template = _templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                throw new Exception(TemplateErrors.TemplateExist(templateName));
            }

            ClearDiagnostics();

            var templateNameLine = BuildTemplateNameLine(templateName, parameters);
            var newTemplateBody = ConvertTemplateBody(templateBody);
            var content = $"{templateNameLine}{_newLine}{newTemplateBody}";

            var originStartLine = GetLinesOfText(_templates.Content).Length;

            // update content
            _templates.Content = $"{_templates.Content}{_newLine}{templateNameLine}{_newLine}{newTemplateBody}";

            var newTemplates = new Templates(content: string.Empty, id: _templates.Id, importResolver: _templates.ImportResolver, expressionParser: _templates.ExpressionParser);
            newTemplates = new TemplatesParser.TemplatesTransformer(newTemplates).Transform(TemplatesParser.AntlrParseTemplates(content, _templates.Id));

            AppendDiagnosticsWithOffset(newTemplates.Diagnostics, originStartLine);

            var newTemplate = newTemplates.FirstOrDefault();
            if (newTemplate != null)
            {
                AdjustRangeForAddTemplate(newTemplate, originStartLine);
                _templates.Add(newTemplate);
                new StaticChecker(_templates).Check().ForEach(u => _templates.Diagnostics.Add(u));
            }

            return _templates;
        }

        /// <summary>
        /// Removes an existing template in current Templates instances.
        /// </summary>
        /// <param name="templateName">Which template should delete.</param>
        /// <returns>Updated LG file.</returns>
        public Templates DeleteTemplate(string templateName)
        {
            var template = _templates.FirstOrDefault(u => u.Name == templateName);
            if (template != null)
            {
                ClearDiagnostics();

                var startLine = template.SourceRange.Range.Start.Line - 1;
                var stopLine = template.SourceRange.Range.End.Line - 1;
                _templates.Content = ReplaceRangeContent(_templates.Content, startLine, stopLine, null);

                AdjustRangeForDeleteTemplate(template);
                _templates.Remove(template);
                new StaticChecker(_templates).Check().ForEach(u => _templates.Diagnostics.Add(u));
            }

            return _templates;
        }

        private void AppendDiagnosticsWithOffset(IList<Diagnostic> diagnostics, int offset)
        {
            if (diagnostics != null)
            {
                diagnostics.ToList().ForEach(u =>
                {
                    u.Range.Start.Line += offset;
                    u.Range.End.Line += offset;
                    _templates.Diagnostics.Add(u);
                });
            }
        }

        private void AdjustRangeForUpdateTemplate(Template oldTemplate, Template newTemplate)
        {
            var newRange = newTemplate.SourceRange.Range.End.Line - newTemplate.SourceRange.Range.Start.Line;
            var oldRange = oldTemplate.SourceRange.Range.End.Line - oldTemplate.SourceRange.Range.Start.Line;
            var lineOffset = newRange - oldRange;

            var hasFound = false;

            for (var i = 0; i < _templates.Count; i++)
            {
                if (hasFound)
                {
                    _templates[i].SourceRange.Range.Start.Line += lineOffset;
                    _templates[i].SourceRange.Range.End.Line += lineOffset;
                }
                else if (_templates[i].Name == oldTemplate.Name)
                {
                    hasFound = true;
                    newTemplate.SourceRange.Range.Start.Line = oldTemplate.SourceRange.Range.Start.Line;
                    newTemplate.SourceRange.Range.End.Line = oldTemplate.SourceRange.Range.End.Line + lineOffset;
                    _templates[i] = newTemplate;
                }
            }
        }

        private void AdjustRangeForAddTemplate(Template newTemplate, int lineOffset)
        {
            var lineLength = newTemplate.SourceRange.Range.End.Line - newTemplate.SourceRange.Range.Start.Line;
            newTemplate.SourceRange.Range.Start.Line = lineOffset + 1;
            newTemplate.SourceRange.Range.End.Line = lineLength + lineOffset + 1;
        }

        private void AdjustRangeForDeleteTemplate(Template oldTemplate)
        {
            var lineOffset = oldTemplate.SourceRange.Range.End.Line - oldTemplate.SourceRange.Range.Start.Line + 1;
            var hasFound = false;
            for (var i = 0; i < _templates.Count; i++)
            {
                if (hasFound)
                {
                    _templates[i].SourceRange.Range.Start.Line -= lineOffset;
                    _templates[i].SourceRange.Range.End.Line -= lineOffset;
                }
                else if (_templates[i].Name == oldTemplate.Name)
                {
                    hasFound = true;
                }
            }
        }

        private void ClearDiagnostics()
        {
            _templates.Diagnostics = new List<Diagnostic>();
        }

        private string ReplaceRangeContent(string originString, int startLine, int stopLine, string replaceString)
        {
            var originList = originString.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            if (startLine < 0 || startLine > stopLine || stopLine >= originList.Length)
            {
                throw new Exception("index out of range.");
            }

            var destList = new List<string>();

            destList.AddRange(originList.Take(startLine));

            if (replaceString != null)
            {
                destList.Add(replaceString);
            }

            destList.AddRange(originList.Skip(stopLine + 1));

            return string.Join(_newLine, destList);
        }

        private string ConvertTemplateBody(string templateBody)
        {
            var lines = GetLinesOfText(templateBody);
            var destList = lines.Select(u =>
            {
                return u.TrimStart().StartsWith("#", StringComparison.Ordinal) ? $"- {u.TrimStart()}" : u;
            });

            return string.Join(_newLine, destList);
        }

        private string[] GetLinesOfText(string text)
        {
            if (text == null)
            {
                return Array.Empty<string>();
            }

            return text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        private string BuildTemplateNameLine(string templateName, List<string> parameters)
        {
            if (parameters == null)
            {
                return $"# {templateName}";
            }
            else
            {
                return $"# {templateName}({string.Join(", ", parameters)})";
            }
        }
    }
}
