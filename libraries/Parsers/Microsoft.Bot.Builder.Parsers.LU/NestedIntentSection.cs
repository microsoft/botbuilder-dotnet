// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Class for Nested Intent sections.
    /// </summary>
    public class NestedIntentSection : Section
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NestedIntentSection"/> class.
        /// </summary>
        /// <param name="parseTree">The nested intent section context from the parse tree.</param>
        public NestedIntentSection(LUFileParser.NestedIntentSectionContext parseTree)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException(nameof(parseTree));
            }

            SectionType = SectionType.NestedIntentSection;
            Name = ExtractName(parseTree);
            Body = string.Empty;
            SimpleIntentSections = ExtractSimpleIntentSections(parseTree);
            Errors = new List<Error>();
            if (SimpleIntentSections != null && SimpleIntentSections.Count > 0)
            {
                SimpleIntentSections.ForEach(section =>
                {
                    Errors.AddRange(section.Errors);
                });
            }

            var secTypeStr = $"{SectionType}";
            Id = $"{char.ToLower(secTypeStr[0], System.Globalization.CultureInfo.InvariantCulture) + secTypeStr.Substring(1)}_{Name}";
            var startPosition = new Position { Line = parseTree.Start.Line, Character = parseTree.Start.Column };
            var stopPosition = new Position { Line = parseTree.Stop.Line, Character = parseTree.Stop.Column + parseTree.Stop.Text.Length };
            Range = new Range { Start = startPosition, End = stopPosition };
        }

        private string ExtractName(LUFileParser.NestedIntentSectionContext parseTree)
        {
            return parseTree.nestedIntentNameLine().nestedIntentName().GetText().Trim();
        }

        private List<SimpleIntentSection> ExtractSimpleIntentSections(LUFileParser.NestedIntentSectionContext parseTree)
        {
            var simpleIntentSections = new List<SimpleIntentSection>();
            foreach (var subIntentDefinition in parseTree.nestedIntentBodyDefinition().subIntentDefinition())
            {
                var simpleIntentSection = new SimpleIntentSection(subIntentDefinition.simpleIntentSection());
                simpleIntentSection.Range.Start.Character = 0;
                simpleIntentSections.Add(simpleIntentSection);
            }

            return simpleIntentSections;
        }
    }
}
