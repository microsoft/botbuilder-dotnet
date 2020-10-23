// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>SectionEntity class.</summary>
    public partial class SectionEntity : Section
    {
        // TODO: pass this constant to a helper class.
        private readonly char[] _invalidCharsInIntentOrEntityName = { '<', '>', '*', '%', '&', ':', '\\', '$' };

        public SectionEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionEntity"/> class.
        /// </summary>
        /// <param name="parseTree">The entity context from the parse tree.</param>
        public SectionEntity(LUFileParser.EntitySectionContext parseTree)
        {
            if (parseTree == null)
            {
                throw new ArgumentNullException(nameof(parseTree));
            }

            SectionType = SectionType.EntitySection;
            Errors = new List<Error>();
            Name = ExtractName(parseTree);
            Type = ExtractType(parseTree);
            SynonymsOrPhraseList = ExtractSynonymsOrPhraseList(parseTree);
            string secTypeStr = $"{SectionType}";
            Id = $"{char.ToLowerInvariant(secTypeStr[0]) + secTypeStr.Substring(1)}_{Name}";
            var startPosition = new Position { Line = parseTree.Start.Line, Character = parseTree.Start.Column };
            var stopPosition = new Position { Line = parseTree.Stop.Line, Character = parseTree.Stop.Column + parseTree.Stop.Text.Length };
            Range = new Range { Start = startPosition, End = stopPosition };
        }

        private string ExtractName(LUFileParser.EntitySectionContext parseTree)
        {
            var entityName = string.Empty;
            if (parseTree.entityDefinition().entityLine().entityName() != null)
            {
                entityName = parseTree.entityDefinition().entityLine().entityName().GetText().Trim();
            }
            else
            {
                Errors.Add(
                    Diagnostic.BuildDiagnostic(
                        message: "Invalid entity line, did you miss entity name after $?",
                        context: parseTree.entityDefinition().entityLine()));
            }

            if (!string.IsNullOrEmpty(entityName) && entityName.IndexOfAny(_invalidCharsInIntentOrEntityName) >= 0)
            {
                Errors.Add(
                    Diagnostic.BuildDiagnostic(
                        message: $"Invalid entity line, entity name {entityName} cannot contain any of the following characters: [<, >, *, %, &, :, \\, $]",
                        context: parseTree.entityDefinition().entityLine()));
                return null;
            }
            else
            {
                return entityName;
            }
        }

        private string ExtractType(LUFileParser.EntitySectionContext parseTree)
        {
            if (parseTree.entityDefinition().entityLine().entityType() != null)
            {
                return parseTree.entityDefinition().entityLine().entityType().GetText().Trim();
            }
            else
            {
                Errors.Add(
                    Diagnostic.BuildDiagnostic(
                        message: "Invalid entity line, did you miss entity type after $?",
                        context: parseTree.entityDefinition().entityLine()));
            }

            return null;
        }

        private List<string> ExtractSynonymsOrPhraseList(LUFileParser.EntitySectionContext parseTree)
        {
            var synonymsOrPhraseList = new List<string>();
            if (parseTree.entityDefinition().entityListBody() != null)
            {
                foreach (var errorItemStr in parseTree.entityDefinition().entityListBody().errorString())
                {
                    if (!string.IsNullOrEmpty(errorItemStr.GetText().Trim()))
                    {
                        Errors.Add(
                            Diagnostic.BuildDiagnostic(
                                message: "Invalid list entity line, did you miss '-' at line begin",
                                context: errorItemStr));
                    }
                }

                foreach (var normalItemStr in parseTree.entityDefinition().entityListBody().normalItemString())
                {
                    var itemStr = normalItemStr.GetText().Trim();
                    synonymsOrPhraseList.Add(itemStr.Substring(1).Trim());
                }
            }

            if (!string.IsNullOrEmpty(Type) && Type.IndexOf('=') > -1 && synonymsOrPhraseList.Count == 0)
            {
                var errorMsg = $"no synonyms list found for list entity definition: \"{parseTree.entityDefinition().entityLine().GetText()}\"";
                var error = Diagnostic.BuildDiagnostic(
                    message: errorMsg,
                    context: parseTree.entityDefinition().entityLine(),
                    severity: Diagnostic.WARN);
                Errors.Add(error);
            }

            return synonymsOrPhraseList;
        }
    }
}
