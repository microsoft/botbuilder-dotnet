// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Alexa.Integration.AspNet.Core;

namespace Microsoft.Bot.Builder.Alexa.Integration
{
    public class AlexaOptions
    {
        public AlexaOptions()
        {
            ValidateIncomingAlexaRequests = true;
            ShouldEndSessionByDefault = false;
        }

        public bool ValidateIncomingAlexaRequests { get; set; }

        public bool ShouldEndSessionByDefault { get; set; }
    }
}
