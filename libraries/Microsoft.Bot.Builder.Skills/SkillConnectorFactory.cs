using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Skills.Internal;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Skills
{
    public static class SkillConnectorFactory
    {
        public static SkillConnector Create(SkillOptions skillOptions, MicrosoftAppCredentials serviceClientCredentials, IBotTelemetryClient botTelemetryClient)
        {
            return new SkillWebSocketsConnector(skillOptions, serviceClientCredentials, botTelemetryClient);
        }
    }
}
