using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public static class TemplateResponses
    {
        // Templates
        public static string ConfusionTemplate { get; } = "[confusion]";

        public static string NameQuestionTemplate { get; } = "[nameQuestion]";

        public static string CityQuestionTemplate { get; } = "[cityQuestion]";

        public static string ShowAppreciationTemplate { get; } = "[showAppreciation]";

        public static string WelcomeUserTemplate { get; } = "[welcomeUser]";

        public static string OfferHelpTemplate { get; } = "[offerHelp]";
    }
}
