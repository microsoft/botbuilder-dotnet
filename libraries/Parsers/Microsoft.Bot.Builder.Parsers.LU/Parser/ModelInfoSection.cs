// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.Generic;
using static LUFileParser;

namespace Microsoft.Bot.Builder.Parsers.LU.Parser
{
    public class ModelInfoSection : Section
    {
        public ModelInfoSection(ModelInfoSectionContext parseTree)
        {
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
