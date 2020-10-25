// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>Import Section class.</summary>
    public class ImportSection : Section
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSection"/> class.
        /// </summary>
        /// <param name="parseTree">The new import section context from the parse tree.</param>
        public ImportSection(LUFileParser.ImportSectionContext parseTree)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException(nameof(parseTree));
            }

            Errors = new List<Error>();
            SectionType = SectionType.ImportSection;
            var result = ExtractDescriptionAndPath(parseTree);
            Description = result.description;
            Path = result.path;
            string secTypeStr = $"{SectionType}";
            Id = $"{char.ToLowerInvariant(secTypeStr[0]) + secTypeStr.Substring(1)}_{Path}";
            var startPosition = new Position { Line = parseTree.Start.Line, Character = parseTree.Start.Column };
            var stopPosition = new Position { Line = parseTree.Stop.Line, Character = parseTree.Stop.Column + parseTree.Stop.Text.Length };
            Range = new Range { Start = startPosition, End = stopPosition };
        }

        public (string description, string path) ExtractDescriptionAndPath(LUFileParser.ImportSectionContext parseTree)
        {
            var importStr = parseTree.importDefinition().IMPORT().GetText();

            string description = null;
            string path = null;

            // TODO: check this regex correct logic
            var regMatch = Regex.Match(importStr, @"\[([^\]]*)\]\(([^\)]*)\)");

            if (regMatch.Success && regMatch.Groups.Count == 3)
            {
                description = regMatch.Groups[1].ToString().Trim();
                path = regMatch.Groups[2].ToString().Trim();

                if (string.IsNullOrEmpty(path))
                {
                    var errorMsg = $"LU file reference path is empty: \"{parseTree.GetText()}\"";
                    var error = Diagnostic.BuildDiagnostic(message: errorMsg, context: parseTree);

                    Errors.Add(error);
                }
            }

            return (description, path);
        }
    }
}
