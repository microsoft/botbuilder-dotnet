// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using static LUFileParser;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Class for ModelInfo sections.
    /// </summary>
    public class ModelInfoSection : Section
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelInfoSection"/> class.
        /// </summary>
        /// <param name="parseTree">The model info section context from the parse tree.</param>
        public ModelInfoSection(ModelInfoSectionContext parseTree)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException(nameof(parseTree));
            }

            SectionType = SectionType.ModelInfoSection;
            ModelInfo = parseTree.modelInfoDefinition().GetText();
            Errors = new List<Error>();
            string secTypeStr = $"{SectionType}";
            Id = $"{char.ToLower(secTypeStr[0], System.Globalization.CultureInfo.InvariantCulture) + secTypeStr.Substring(1)}_{ModelInfo}";
            Position startPosition = new Position { Line = parseTree.Start.Line, Character = parseTree.Start.Column };
            Position stopPosition = new Position { Line = parseTree.Stop.Line, Character = parseTree.Stop.Column + parseTree.Stop.Text.Length };
            Range = new Range();
            Range.Start = startPosition;
            Range.End = stopPosition;
        }
    }
}
