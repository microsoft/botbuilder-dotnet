// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    /// <summary>
    /// Special visitor to extract composite entities.
    /// This class is static.
    /// </summary>
    public static class Visitor
    {
        /// <summary>
        /// Special visitor to extract composite entities.
        /// This class is static.
        /// </summary>
        /// <param name="context">The intent context.</param>
        /// <returns>The utterance to entities map.</returns>
        public static UtteranceAndEntitiesMap VisitNormalIntentStringContext(LUFileParser.NormalIntentStringContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var utterance = string.Empty;
            var entities = new List<EntityElement>();
            var errorMessages = new List<string>();

            // TODO: Check that this interface is actually implemented in the iterable
            foreach (ITerminalNode innerNode in context.children)
            {
                switch (innerNode.Symbol.Type)
                {
                    case LUFileParser.DASH:
                        break;
                    case LUFileParser.EXPRESSION:
                        var tokenizedUterance = TokenizeUtterance(innerNode.GetText().Trim());
                        var utteranceBuilder = new StringBuilder(utterance.TrimStart());
                        utterance = RecursivelyResolveTokenizedUtterance(tokenizedUterance, entities, errorMessages, utteranceBuilder).ToString();
                        break;
                    default:
                        utterance += innerNode.GetText();
                        break;
                }
            }

            return new UtteranceAndEntitiesMap
            {
                Utterance = utterance.Trim(),
                Entities = entities,
                ErrorMsgs = errorMessages
            };
        }

        private static StringBuilder RecursivelyResolveTokenizedUtterance(List<object> tokenizedUterance, List<EntityElement> entities, List<string> errorMessages, StringBuilder srcUtterance)
        {
            char[] invalidCharsInIntentOrEntityName = { '<', '>', '*', '%', '&', ':', '\\', '$' };
            foreach (var item in tokenizedUterance)
            {
                if (item is AuxEntity auxEntity)
                {
                    var entityName = auxEntity.EntityName.Trim();
                    if (!string.IsNullOrEmpty(entityName) && entityName.IndexOfAny(invalidCharsInIntentOrEntityName) >= 0)
                    {
                        errorMessages.Add($"Invalid utterance line, entity name {entityName} cannot contain any of the following characters: [<, >, *, %, &, :, \\, $]");
                        continue;
                    }

                    if (auxEntity.EntityValue == null)
                    {
                        // we have a pattern.any entity
                        var patternStr = !string.IsNullOrEmpty(auxEntity.Role) ? $"{{{auxEntity.EntityName}:{auxEntity.Role}}}" : $"{{{auxEntity.EntityName}}}";
                        srcUtterance.Append(patternStr);
                        entities.Add(
                            new EntityElement
                            {
                                Type = TypeEnum.PatternAnyEntities,
                                Entity = entityName,
                                Role = auxEntity.Role.Trim()
                            });
                    }
                    else
                    {
                        // we have a new entity
                        var newEntity = new EntityElement
                        {
                            Type = TypeEnum.Entities,
                            Entity = entityName,
                            Role = auxEntity.Role.Trim(),
                            StartPos = srcUtterance.Length,
                            EndPos = null
                        };
                        if (auxEntity.EntityValue == null)
                        {
                            errorMessages.Add($"Composite entity {auxEntity.Parent.EntityName} includes pattern.any entity {auxEntity.EntityName}.\r\n\tComposites cannot include pattern.any entity as a child.");
                        }
                        else
                        {
                            srcUtterance = new StringBuilder(RecursivelyResolveTokenizedUtterance(auxEntity.EntityValue, entities, errorMessages, srcUtterance).ToString().TrimStart());
                            newEntity.EndPos = srcUtterance.Length - 1;
                            entities.Add(newEntity);
                        }
                    }
                }
                else
                {
                    string unicodeCharItem = (string)item;
                    srcUtterance.Append(unicodeCharItem);
                }
            }

            return srcUtterance;
        }

        private static List<object> TokenizeUtterance(string expression)
        {
            var splitString = new List<object>();
            var currentList = splitString;
            AuxEntity currentEntity = null;
            var entityNameCapture = false;
            var entityValueCapture = false;
            var entityRoleCapture = false;

            TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(expression);
            while (charEnum.MoveNext())
            {
                var unicodeCharacter = charEnum.GetTextElement();
                switch (unicodeCharacter)
                {
                    case "{":
                        var newEntity = new AuxEntity
                        {
                            EntityName = string.Empty,
                            Role = string.Empty,
                            EntityValue = null,
                            Parent = currentEntity
                        };
                        currentList.Add(newEntity);
                        currentEntity = newEntity;
                        entityNameCapture = true;
                        entityValueCapture = false;
                        entityRoleCapture = false;
                        break;
                    case "}":
                        currentEntity = currentEntity.Parent;
                        currentList = currentEntity != null ? currentEntity.EntityValue : splitString;
                        entityNameCapture = false;
                        entityValueCapture = false;
                        entityRoleCapture = false;
                        break;
                    case "=":
                        currentEntity.EntityValue = new List<object>();
                        currentList = currentEntity.EntityValue;
                        entityNameCapture = false;
                        entityValueCapture = true;
                        entityRoleCapture = false;
                        break;
                    case ":":
                        if (currentEntity != null && !string.IsNullOrEmpty(currentEntity.EntityName) && entityNameCapture)
                        {
                            entityNameCapture = false;
                            entityValueCapture = false;
                            entityRoleCapture = true;
                        }
                        else
                        {
                            currentList.Add(unicodeCharacter);
                        }

                        break;
                    default:
                        if (entityNameCapture)
                        {
                            currentEntity.EntityName += unicodeCharacter;
                        }
                        else if (entityValueCapture)
                        {
                            if (unicodeCharacter == " ")
                            {
                                // we do not want leading spaces
                                if (currentList.Count != 0)
                                {
                                    currentList.Add(unicodeCharacter);
                                }
                            }
                            else
                            {
                                currentList.Add(unicodeCharacter);
                            }
                        }
                        else if (entityRoleCapture)
                        {
                            currentEntity.Role += unicodeCharacter;
                        }
                        else
                        {
                            currentList.Add(unicodeCharacter);
                        }

                        break;
                }
            }

            return splitString;
        }

        private class AuxEntity
        {
            public List<object> EntityValue { get; set; }

            public AuxEntity Parent { get; set; }

            public string EntityName { get; set; }

            public string Role { get; set; }
        }
    }
}
