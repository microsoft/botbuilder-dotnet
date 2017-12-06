using AlarmBot.Models;
using AlarmBot.TopicViews;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmBot.Topics
{
    /// <summary>
    /// Topic for showing alarms
    /// </summary>
    public class ShowAlarmsTopic : BaseTopic
    {
        public ShowAlarmsTopic()
        {
            this.Name = "ShowAlarms";
        }

        /// <summary>
        /// Called when topic is activated (SINGLE TURN)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<bool> StartTopic(BotContext context)
        {
            await ShowAlarms(context);

            // end the topic immediately
            return false;
        }

        public static Task ShowAlarms(BotContext context)
        {
            var alarms = (List<Alarm>)context.State.User[UserProperties.ALARMS];
            if (alarms == null)
            {
                alarms = new List<Alarm>();
                context.State.User[UserProperties.ALARMS] = alarms;
            }

            context.ReplyWith(ShowAlarmsTopicView.SHOWALARMS, alarms);
            return Task.CompletedTask;
        }
    }
}
