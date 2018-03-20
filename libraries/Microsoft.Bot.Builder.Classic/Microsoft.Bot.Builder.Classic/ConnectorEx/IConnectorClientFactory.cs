// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Microsoft.Bot.Builder.Classic;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Dialogs.Internals
{
    /// <summary>
    /// Factory for IConnectorClient.
    /// </summary>
    public interface IConnectorClientFactory
    {
        /// <summary>
        /// Make the IConnectorClient implementation.
        /// </summary>
        /// <returns>The IConnectorClient implementation.</returns>
        IConnectorClient MakeConnectorClient();

    }

    public sealed class ConnectorClientFactory : IConnectorClientFactory
    {
        private readonly Uri serviceUri;
        private readonly IAddress address;
        private readonly MicrosoftAppCredentials credentials;
        public ConnectorClientFactory(IAddress address, MicrosoftAppCredentials credentials)
        {
            SetField.NotNull(out this.address, nameof(address), address);
            SetField.NotNull(out this.credentials, nameof(credentials), credentials);

            this.serviceUri = new Uri(address.ServiceUrl);
        }

        public static bool IsEmulator(IAddress address)
        {
            return address.ChannelId.Equals(ChannelIds.Emulator, StringComparison.OrdinalIgnoreCase);
        }

        IConnectorClient IConnectorClientFactory.MakeConnectorClient()
        {
            return new ConnectorClient(this.serviceUri, this.credentials);
        }
    }
}
