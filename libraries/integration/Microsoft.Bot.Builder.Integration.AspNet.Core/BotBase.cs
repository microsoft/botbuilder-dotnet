// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{

    public abstract class BotBase
    {
        public BotBase(BotFrameworkOptions options)
        {
        }
    }
}
