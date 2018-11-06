// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    public static class TemplateResponses
    {
        // Templates
        public static string WelcomeUserTemplate { get; } = "[welcomeMessage]";

        public static string AskForLocationTemplate { get; } = "[AskForLocation]";

        public static string AskForDateTimeTemplate { get; } = "[AskForDateTime]";

        public static string AskForPartySizeTemplate { get; } = "[AskForPartySize]";

        public static string ConfirmBookingReadoutTemplate { get; } = "[ConfirmBookingReadout]";

        public static string BookingConfirmationReadoutTemplate { get; } = "[BookingConfirmationReadout]";
    }
}
