// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.Bot.Builder.Alexa.Integration
{
    public class AlexaBotPaths
    {
        public AlexaBotPaths()
        {
            this.BasePath = "/api";
            this.SkillRequestsPath = "skillrequests";
        }

        public string BasePath { get; set; }
        public string SkillRequestsPath { get; set; }
    }
}