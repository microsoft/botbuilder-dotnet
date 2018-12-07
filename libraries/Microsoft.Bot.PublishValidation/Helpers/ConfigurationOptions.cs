// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.PublishValidation
{
    public class ConfigurationOptions
    {
        public string ProjectPath { get; set; }

        public string Secret { get; set; }

        public bool ForbidSpacesInProjectName { get; set; }

        public bool RequireBotFile { get; set; }

        public IEnumerable<string> RequiredEndpoints { get; set; }

        public IEnumerable<string> ForbiddenEndpoints { get; set; }

        public bool RequireLuisKey { get; set; }

        public bool RequireQnAMakerKey { get; set; }
    }
}
