// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TemplateManager.Tests
{
    public class TemplateFixture : IDisposable
    {
        public TemplateFixture()
        {
            Templates1 = new LanguageTemplateDictionary
            {
                ["default"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"default: {data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)default: {data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"default: Yo {data.name}" },
                },
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"en: {data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)en: {data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"en: Yo {data.name}" },
                },
                ["fr"] = new TemplateIdMap
                {
                    { "stringTemplate", (context, data) => $"fr: {data.name}" },
                    { "activityTemplate", (context, data) => { return new Activity() { Type = ActivityTypes.Message, Text = $"(Activity)fr: {data.name}" }; } },
                    { "stringTemplate2", (context, data) => $"fr: Yo {data.name}" },
                },
            };
            Templates2 = new LanguageTemplateDictionary
            {
                ["en"] = new TemplateIdMap
                {
                    { "stringTemplate2", (context, data) => $"en: StringTemplate2 override {data.name}" },
                },
            };
        }

        public LanguageTemplateDictionary Templates1 { get; private set; }

        public LanguageTemplateDictionary Templates2 { get; private set; }

        public void Dispose()
        {
            Templates1.Clear();
            Templates2.Clear();
        }
    }
}
