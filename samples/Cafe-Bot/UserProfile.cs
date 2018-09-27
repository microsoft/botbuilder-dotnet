// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This is our application state. Just a regular serializable .NET class.
    /// </summary>
    public class UserProfile
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Location { get; set; }

        public string Time { get; set; }

        public int PartySize { get; set; }
    }
}
