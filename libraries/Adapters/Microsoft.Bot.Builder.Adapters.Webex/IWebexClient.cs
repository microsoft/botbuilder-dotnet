// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public interface IWebexClient
    {
        void CreateClient(string accessToken);

        Task<string> CreateMessageAsync(string toPersonOrEmail, string text);
    }
}
