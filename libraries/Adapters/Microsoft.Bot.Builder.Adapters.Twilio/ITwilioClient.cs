// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Twilio
{
    /// <summary>
    /// Represents a Twilio client.
    /// </summary>
    public interface ITwilioClient
    {
        /// <summary>
        /// Logs into the Twilio API service.
        /// </summary>
        /// <param name="username">The user name for the Twilio API.</param>
        /// <param name="password">The password for the Twilio API.</param>
        void LogIn(string username, string password);

        /// <summary>
        /// Returns the resource ID from a message.
        /// </summary>
        /// <param name="messageOptions">An object that represents the Twilio message options.</param>
        /// <returns>The ID from the Twilio message.</returns>
        Task<string> GetResourceIdentifier(object messageOptions);
    }
}
