using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    internal static class TestTelemetryProperties
    {
        internal static Dictionary<string, string> GetCodeIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "codeIntent" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"codeIntent\":{\"score\":1.0,\"pattern\":\"(?<code>[a-z][0-9])\"}}" },
                {
                    "Entities",
                    "{\r\n  \"code\": [\r\n    \"a1\",\r\n    \"b2\"\r\n  ],\r\n  \"$instance\": {\r\n    \"code\": [\r\n      {\r\n        \"startIndex\": 7,\r\n        \"endIndex\": 9,\r\n        \"score\": 1.0,\r\n        \"text\": \"a1\",\r\n        \"type\": \"code\",\r\n        \"resolution\": null\r\n      },\r\n      {\r\n        \"startIndex\": 10,\r\n        \"endIndex\": 12,\r\n        \"score\": 1.0,\r\n        \"text\": \"b2\",\r\n        \"type\": \"code\",\r\n        \"resolution\": null\r\n      }\r\n    ]\r\n  }\r\n}"
                },
                { "AdditionalProperties", null },
            };
        }

        internal static Dictionary<string, string> GetColorIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "colorIntent" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"colorIntent\":{\"score\":1.0,\"pattern\":\"(?i)(color|colour)\"}}" },
                {
                    "Entities",
                    "{\r\n  \"color\": [\r\n    \"red\",\r\n    \"orange\"\r\n  ],\r\n  \"$instance\": {\r\n    \"color\": [\r\n      {\r\n        \"startIndex\": 19,\r\n        \"endIndex\": 23,\r\n        \"score\": 1.0,\r\n        \"text\": \"red\",\r\n        \"type\": \"color\",\r\n        \"resolution\": null\r\n      },\r\n      {\r\n        \"startIndex\": 27,\r\n        \"endIndex\": 34,\r\n        \"score\": 1.0,\r\n        \"text\": \"orange\",\r\n        \"type\": \"color\",\r\n        \"resolution\": null\r\n      }\r\n    ]\r\n  }\r\n}"
                },
                { "AdditionalProperties", null },
            };
        }

        internal static Dictionary<string, string> GetGreetingIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "Greeting" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"Greeting\":{\"score\":1.0,\"pattern\":\"(?i)howdy\"}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", null }
            };
        }

        internal static Dictionary<string, string> GetChooseIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "ChooseIntent" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"ChooseIntent\":{\"score\":1.0}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"candidates\":[{\"id\":\"y\",\"intent\":\"y\",\"score\":1.0,\"result\":{\"text\":\"criss-cross applesauce\",\"alteredText\":null,\"intents\":{\"y\":{\"score\":1.0,\"pattern\":\"criss-cross applesauce\"}},\"entities\":{},\"id\":\"y\"}},{\"id\":\"z\",\"intent\":\"z\",\"score\":1.0,\"result\":{\"text\":\"criss-cross applesauce\",\"alteredText\":null,\"intents\":{\"z\":{\"score\":1.0,\"pattern\":\"criss-cross applesauce\"}},\"entities\":{},\"id\":\"z\"}}]}" },
            };
        }

        internal static Dictionary<string, string> GetXIntentProperties()
        {
            return new Dictionary<string, string>()
            {
                { "AlteredText", null },
                { "TopIntent", "x" },
                { "TopIntentScore", "Microsoft.Bot.Builder.IntentScore" },
                { "Intents", "{\"x\":{\"score\":1.0,\"pattern\":\"x\"}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"id\":\"x\"}" }
            };
        }
    }
}
