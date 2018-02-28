// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace Microsoft.Bot
{
    public interface IBot
    {
        Task OnReceiveActivity(IBotContext botContext);
    }
}