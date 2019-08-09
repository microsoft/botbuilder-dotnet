// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    public interface ITwilioClient
    {
        void LogIn(string username, string password);

        Task<string> GetResourceIdentifier(object messageOptions);
    }
}
