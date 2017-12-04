using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace AlarmBot.Topics
{
    public class BaseTopic : ITopic
    {
        public virtual Task<bool> ContinueTopic(BotContext context)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> ResumeTopic(BotContext context)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> StartTopic(BotContext context)
        {
            return Task.FromResult(false);
        }
    }
}
