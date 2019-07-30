// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.BotKit.Core
{
    public interface IBotkitPlugin
    {
        string Name { get; set; }

        Task Init(Botkit botkit);
    }
}
