using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    public static class FallbackMessages
    {
        // Templates
        public static string AskToBookTable { get; } = "Would you like to book a table?";

        public static string UnableToHelp { get; } = "well then maybe I'm not the most suitable bot to help you as I can only book restaurants : ) ";

        public static string NotAvailableInThisPlace { get; } = "Sorry we only have a store based in Cairo, soon we will extend to other cities :) ";
    }
}
