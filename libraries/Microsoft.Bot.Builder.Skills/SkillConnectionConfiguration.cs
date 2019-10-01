// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Skills
{
    public class SkillConnectionConfiguration
    {
        public SkillOptions SkillOptions { get; set; }

        public MicrosoftAppCredentials ServiceClientCredentials { get; set; }
    }
}
