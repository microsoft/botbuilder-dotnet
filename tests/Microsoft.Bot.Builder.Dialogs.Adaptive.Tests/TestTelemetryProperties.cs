// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    /// <summary>
    /// Helper class used to provide the expected telemetry properties for the intent specified in <see cref="RecognizerTelemetryUtils"/>.
    /// </summary>
    internal static class TestTelemetryProperties
    {
        internal static Dictionary<string, string> GetCodeIntentProperties()
        {
            return new Dictionary<string, string>
            {
                { "TopIntent", "codeIntent" },
                { "TopIntentScore", "1.0" },
                { "Intents", "{\"codeIntent\":{\"score\":1.0,\"pattern\":\"(?<code>[a-z][0-9])\"}}" },
                { "Entities", "{\r\n  \"code\": [\r\n    \"a1\",\r\n    \"b2\"\r\n  ],\r\n  \"$instance\": {\r\n    \"code\": [\r\n      {\r\n        \"startIndex\": 7,\r\n        \"endIndex\": 9,\r\n        \"score\": 1.0,\r\n        \"text\": \"a1\",\r\n        \"type\": \"code\",\r\n        \"resolution\": null\r\n      },\r\n      {\r\n        \"startIndex\": 10,\r\n        \"endIndex\": 12,\r\n        \"score\": 1.0,\r\n        \"text\": \"b2\",\r\n        \"type\": \"code\",\r\n        \"resolution\": null\r\n      }\r\n    ]\r\n  }\r\n}" },
                { "AdditionalProperties", null },
            };
        }

        internal static Dictionary<string, string> GetColorIntentProperties()
        {
            return new Dictionary<string, string>
            {
                { "TopIntent", "colorIntent" },
                { "TopIntentScore", "1.0" },
                { "Intents", "{\"colorIntent\":{\"score\":1.0,\"pattern\":\"(?i)(color|colour)\"}}" },
                { "Entities", "{\r\n  \"color\": [\r\n    \"red\",\r\n    \"orange\"\r\n  ],\r\n  \"$instance\": {\r\n    \"color\": [\r\n      {\r\n        \"startIndex\": 19,\r\n        \"endIndex\": 23,\r\n        \"score\": 1.0,\r\n        \"text\": \"red\",\r\n        \"type\": \"color\",\r\n        \"resolution\": null\r\n      },\r\n      {\r\n        \"startIndex\": 27,\r\n        \"endIndex\": 34,\r\n        \"score\": 1.0,\r\n        \"text\": \"orange\",\r\n        \"type\": \"color\",\r\n        \"resolution\": null\r\n      }\r\n    ]\r\n  }\r\n}" },
                { "AdditionalProperties", null },
            };
        }

        internal static Dictionary<string, string> GetGreetingIntentProperties()
        {
            return new Dictionary<string, string>
            {
                { "TopIntent", "Greeting" },
                { "TopIntentScore", "1.0" },
                { "Intents", "{\"Greeting\":{\"score\":1.0,\"pattern\":\"(?i)howdy\"}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", null }
            };
        }

        internal static Dictionary<string, string> GetChooseIntentProperties()
        {
            return new Dictionary<string, string>
            {
                { "TopIntent", "ChooseIntent" },
                { "TopIntentScore", "1.0" },
                { "Intents", "{\"ChooseIntent\":{\"score\":1.0}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"candidates\":[{\"id\":\"y\",\"intent\":\"y\",\"score\":1.0,\"result\":{\"text\":\"criss-cross applesauce\",\"alteredText\":null,\"intents\":{\"y\":{\"score\":1.0,\"pattern\":\"criss-cross applesauce\"}},\"entities\":{},\"id\":\"y\"}},{\"id\":\"z\",\"intent\":\"z\",\"score\":1.0,\"result\":{\"text\":\"criss-cross applesauce\",\"alteredText\":null,\"intents\":{\"z\":{\"score\":1.0,\"pattern\":\"criss-cross applesauce\"}},\"entities\":{},\"id\":\"z\"}}]}" },
            };
        }

        internal static Dictionary<string, string> GetXIntentProperties()
        {
            return new Dictionary<string, string>
            {
                { "TopIntent", "x" },
                { "TopIntentScore", "1.0" },
                { "Intents", "{\"x\":{\"score\":1.0,\"pattern\":\"x\"}}" },
                { "Entities", "{}" },
                { "AdditionalProperties", "{\"id\":\"x\"}" }
            };
        }
    }
}
