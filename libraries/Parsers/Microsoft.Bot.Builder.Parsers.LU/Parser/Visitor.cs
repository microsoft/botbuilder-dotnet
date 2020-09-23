#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable SA1402 // File may only contain a single type
using System.Collections.Generic;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Parsers.LU.Parser
{
    public static class Visitor
    {
        public static UtteranceAndEntitiesMap VisitNormalIntentStringContext(LUFileParser.NormalIntentStringContext context)
        {
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
                        utterance = RecursivelyResolveTokenizedUtterance(tokenizedUterance, entities, errorMessages, utterance.TrimStart());
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

        private static string RecursivelyResolveTokenizedUtterance(List<object> tokenizedUterance, List<EntityElement> entities, List<string> errorMessages, string srcUtterance)
        {
            char[] invalidCharsInIntentOrEntityName = { '<', '>', '*', '%', '&', ':', '\\', '$' };
            foreach (var item in tokenizedUterance)
            {
                if (item is AuxEntity)
                {
                    var auxEntity = (AuxEntity)item;
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
                        srcUtterance += patternStr;
                        entities.Add(
                            new EntityElement
                            {
                                Type = TypeEnum.PatternAnyEntities,
                                Entity = auxEntity.EntityName.Trim(),
                                Role = auxEntity.Role.Trim()
                            });
                    }
                    else
                    {
                        // we have a new entity
                        var newEntity = new EntityElement
                        {
                            Type = TypeEnum.Entities,
                            Entity = auxEntity.EntityName.Trim(),
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
                            srcUtterance = RecursivelyResolveTokenizedUtterance(auxEntity.EntityValue, entities, errorMessages, srcUtterance).TrimStart();
                            newEntity.EndPos = srcUtterance.Length - 1;
                            entities.Add(newEntity);
                        }
                    }
                }
                else
                {
                    char charItem = (char)item;
                    srcUtterance += charItem;
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

            foreach (char character in expression)
            {
                switch (character)
                {
                    case '{':
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
                    case '}':
                        currentEntity = currentEntity.Parent;
                        currentList = currentEntity != null ? currentEntity.EntityValue : splitString;
                        entityNameCapture = false;
                        entityValueCapture = false;
                        entityRoleCapture = false;
                        break;
                    case '=':
                        currentEntity.EntityValue = new List<object>();
                        currentList = currentEntity.EntityValue;
                        entityNameCapture = false;
                        entityValueCapture = true;
                        entityRoleCapture = false;
                        break;
                    case ':':
                        if (currentEntity != null && !string.IsNullOrEmpty(currentEntity.EntityName) && entityNameCapture)
                        {
                            entityNameCapture = false;
                            entityValueCapture = false;
                            entityRoleCapture = true;
                        }
                        else
                        {
                            currentList.Add(character);
                        }

                        break;
                    default:
                        if (entityNameCapture)
                        {
                            currentEntity.EntityName += character;
                        }
                        else if (entityValueCapture)
                        {
                            if (character == ' ')
                            {
                                // we do not want leading spaces
                                if (currentList.Count != 0)
                                {
                                    currentList.Add(character);
                                }
                            }
                            else
                            {
                                currentList.Add(character);
                            }
                        }
                        else if (entityRoleCapture)
                        {
                            currentEntity.Role += character;
                        }
                        else
                        {
                            currentList.Add(character);
                        }

                        break;
                }
            }

            return splitString;
        }
    }

    public class AuxEntity
    {
        public List<object> EntityValue = null;

        public AuxEntity Parent = null;

        public string EntityName { get; set; }

        public string Role { get; set; }
    }
}
