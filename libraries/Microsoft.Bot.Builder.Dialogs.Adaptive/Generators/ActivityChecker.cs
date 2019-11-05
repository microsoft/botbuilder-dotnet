using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdaptiveCards;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    public static class ActivityChecker
    {
        /// <summary>
        /// check the lg string result before generate an Activity.
        /// </summary>
        /// <param name="lgStringResult">string result from languageGenerator.</param>
        /// <returns>Diagnostic list.</returns>
        public static List<Diagnostic> Check(string lgStringResult)
        {
            JObject lgStructuredResult;
            try
            {
                lgStructuredResult = JObject.Parse(lgStringResult);
            }
            catch
            {
                return new List<Diagnostic> { BuildDiagnostic("lg output is not a json object, and will fallback to string format.", false) };
            }

            return CheckStructuredResult(lgStructuredResult);
        }

        private static List<Diagnostic> CheckStructuredResult(JObject lgJObj)
        {
            var result = new List<Diagnostic>();
            var type = GetStructureType(lgJObj);
            if (string.IsNullOrWhiteSpace(type))
            {
                result.Add(BuildDiagnostic("there is no 'type' or '$type' in the lg output json."));
            }
            else if (ActivityFactory.GenericCardTypeMapping.ContainsKey(type))
            {
                result.AddRange(CheckAttachment(lgJObj));
            }
            else if (type == nameof(Activity).ToLower())
            {
                result.AddRange(CheckActivity(lgJObj));
            }
            else
            {
                result.Add(BuildDiagnostic($"type '{type}' is not support currently."));
            }

            if (result.Count > 0)
            {
                return result;
            }

            return SchemaCheck(lgJObj);
        }

        private static List<Diagnostic> SchemaCheck(JObject lgJObj)
        {
            var result = new List<Diagnostic>();
            var assembly = Assembly.GetExecutingAssembly();

            using (var sr = new StreamReader(assembly.GetManifestResourceStream("Microsoft.Bot.Builder.Dialogs.Adaptive.Generators.StructuredLG.schema")))
            {
                var schemaContent = sr.ReadToEnd();
                var schema = JSchema.Parse(schemaContent);
                ChangePropertiesToLowerCase(lgJObj);

                var valid = lgJObj.IsValid(schema, out IList<ValidationError> errors);

                if (!valid)
                {
                    foreach (var error in errors)
                    {
                        result.Add(BuildDiagnostic($"schema error : {error.Message}"));
                    }
                }

                return result;
            }
        }

        private static void ChangePropertiesToLowerCase(JObject jsonObject)
        {
            foreach (var property in jsonObject.Properties().ToList())
            {
                property.Replace(new JProperty(property.Name.ToLower(), property.Value));
            }
        }

        private static List<Diagnostic> CheckActivity(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            var activityType = lgJObj["type"]?.ToString();
            if (!string.IsNullOrEmpty(activityType))
            {
                // Currently Event and Message type are supported.
                if (activityType != ActivityTypes.Event && activityType != ActivityTypes.Message)
                {
                    result.Add(BuildDiagnostic($"'{activityType}' is not support currently. It will fallback to message activity.", false));
                }
            }

            if (activityType == ActivityTypes.Event)
            {
                result.AddRange(CheckEventActivity(lgJObj));
            }
            else
            {
                result.AddRange(CheckMessageActivity(lgJObj));
            }

            return result;
        }

        private static List<Diagnostic> CheckEventActivity(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLower())
                {
                    case "$type":
                    case "type":
                    case "name":
                    case "value":
                        break;

                    default:
                        result.Add(BuildDiagnostic($"'{property}' is not support in Event Activity.", false));
                        break;
                }
            }

            return result;
        }

        private static List<Diagnostic> CheckMessageActivity(JObject lgJObj)
        {
            var result = new List<Diagnostic>();
            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim();
                var value = item.Value;

                switch (property.ToLower())
                {
                    case "$type":
                    case "type":
                    case "text":
                    case "speak":
                    case "inputhint":
                    case "attachmentlayout":
                        break;

                    case "attachments":
                        result.AddRange(CheckAttachments(value));
                        break;

                    case "suggestedactions":
                        result.AddRange(CheckSuggestions(value));
                        break;

                    case "attachment":
                        result.Add(BuildDiagnostic($"'{property}' is not support, do you mean 'attachments'?", false));
                        break;
                    case "suggestedaction":
                        result.Add(BuildDiagnostic($"'{property}' is not support, do you mean 'suggestedactions'?", false));
                        break;
                    default:
                        result.Add(BuildDiagnostic($"'{property}' is not support in message activity.", false));
                        break;
                }
            }

            return result;
        }

        private static List<Diagnostic> CheckSuggestions(JToken value)
        {
            var result = new List<Diagnostic>();
            var actions = NormalizedToList(value);
            foreach (var action in actions)
            {
                if (action is JValue jValue && jValue.Type == JTokenType.String)
                {
                    return result;
                }
                else if (action is JObject actionJObj)
                {
                    result.AddRange(CheckCardAction(actionJObj));
                }
            }

            return result;
        }

        private static List<Diagnostic> CheckButtons(JToken value)
        {
            var result = new List<Diagnostic>();
            var actions = NormalizedToList(value);

            foreach (var action in actions)
            {
                if (action is JValue jValue && jValue.Type == JTokenType.String)
                {
                    return result;
                }
                else if (action is JObject actionJObj)
                {
                    result.AddRange(CheckCardAction(actionJObj));
                }
            }

            return result;
        }

        private static List<Diagnostic> CheckCardAction(JObject cardActionJObj)
        {
            var result = new List<Diagnostic>();
            var type = GetStructureType(cardActionJObj);
            if (type != nameof(CardAction).ToLower())
            {
                result.Add(BuildDiagnostic($"'{type}' is not card action type.", false));
            }
            else
            {
                foreach (var item in cardActionJObj)
                {
                    var property = item.Key.Trim();
                    var value = item.Value.ToString().ToLower();

                    switch (property.ToLower())
                    {
                        case "$type":
                        case "title":
                        case "value":
                        case "displaytext":
                        case "text":
                        case "image":
                            break;

                        case "type":
                            if (value != ActionTypes.ImBack.ToLower()
                                && value != ActionTypes.Call.ToLower()
                                && value != ActionTypes.DownloadFile.ToLower()
                                && value != ActionTypes.MessageBack.ToLower()
                                && value != ActionTypes.openApp.ToLower()
                                && value != ActionTypes.OpenUrl.ToLower()
                                && value != ActionTypes.Payment.ToLower()
                                && value != ActionTypes.PlayAudio.ToLower()
                                && value != ActionTypes.PlayVideo.ToLower()
                                && value != ActionTypes.PostBack.ToLower()
                                && value != ActionTypes.ShowImage.ToLower()
                                && value != ActionTypes.Signin.ToLower())
                            {
                                result.Add(BuildDiagnostic($"'{value}' is not a valid action type"));
                            }

                            break;
                        default:
                            result.Add(BuildDiagnostic($"'{property}' is not support for card action.", false));
                            break;
                    }
                }
            }

            return result;
        }

        private static string GetStructureType(JObject jObj)
        {
            if (jObj == null)
            {
                return string.Empty;
            }

            var type = jObj["$type"]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(type))
            {
                // Adaptive card type
                type = jObj["type"]?.ToString()?.Trim();
            }

            return type?.ToLower() ?? string.Empty;
        }

        private static List<Diagnostic> CheckAttachments(JToken value)
        {
            var result = new List<Diagnostic>();

            var attachmentsJsonList = NormalizedToList(value);

            foreach (var attachmentsJson in attachmentsJsonList)
            {
                if (attachmentsJson is JObject attachmentsJsonJObj)
                {
                    result.AddRange(CheckAttachment(attachmentsJsonJObj));
                }
            }

            return result;
        }

        private static List<Diagnostic> CheckAttachment(JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            var type = GetStructureType(lgJObj);

            if (ActivityFactory.GenericCardTypeMapping.ContainsKey(type))
            {
                result.AddRange(CheckCardAtttachment(ActivityFactory.GenericCardTypeMapping[type], lgJObj));
            }
            else if (type == nameof(AdaptiveCard).ToLower())
            {
                try
                {
                    var parseResult = AdaptiveCard.FromJson(lgJObj.ToString());
                    if (parseResult.Warnings.Count > 0)
                    {
                        foreach (var warning in parseResult.Warnings)
                        {
                            result.Add(BuildDiagnostic(warning.Message, false));
                        }
                    }
                }
                catch (Exception e)
                {
                    result.Add(BuildDiagnostic(e.Message));
                }
            }
            else
            {
                result.Add(BuildDiagnostic($"'{type}' is not an attachment type.", false));
            }

            return result;
        }

        private static List<Diagnostic> CheckCardAtttachment(string type, JObject lgJObj)
        {
            var result = new List<Diagnostic>();

            foreach (var item in lgJObj)
            {
                var property = item.Key.Trim().ToLower();
                var value = item.Value;

                switch (property)
                {
                    case "$type":
                    case "title":
                    case "subtitle":
                    case "text":
                    case "aspect":
                    case "value":
                    case "connectionname":
                    case "image":
                    case "images":
                    case "media":
                        break;
                    case "buttons":
                        result.AddRange(CheckButtons(value));
                        break;

                    case "autostart":
                    case "shareable":
                    case "autoloop":
                        if (value.ToString().ToLower() != "true" && value.ToString().ToLower() != "false")
                        {
                            result.Add(BuildDiagnostic($"'{value.ToString()}' is not a boolean value."));
                        }

                        break;
                    default:
                        result.Add(BuildDiagnostic($"'{property}' is not support for card.", false));
                        break;
                }
            }

            return result;
        }

        private static List<JToken> NormalizedToList(JToken item)
        {
            return item == null ?
                new List<JToken>() :
                item is JArray array ? array.ToList() : new List<JToken>() { item };
        }

        private static Diagnostic BuildDiagnostic(string message, bool isError = true)
        {
            message = message ?? string.Empty;
            var emptyRange = new Range(new Position(0, 0), new Position(0, 0));

            return isError ? new Diagnostic(emptyRange, message, DiagnosticSeverity.Error)
                : new Diagnostic(emptyRange, message, DiagnosticSeverity.Warning);
        }
    }
}
