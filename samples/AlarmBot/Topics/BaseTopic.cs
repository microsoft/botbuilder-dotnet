// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot.Topics
{
    public class BaseTopic : ITopic
    {
        public string Name { get; set; } = "BaseTopic";

        public virtual Task<bool> ContinueTopic(IBotContext context)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> ResumeTopic(IBotContext context)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> StartTopic(IBotContext context)
        {
            return Task.FromResult(false);
        }
    }
}
