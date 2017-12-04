using AlarmBot.Models;
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
        public const string TopicName = "ShowAlarmsTopic";

        // Template Ids
        public const string SHOWALARMS = "ShowAlarmsTopic.ShowAlarms";

        /// <summary>
        /// Language dictionary of template functions
        /// </summary>
        public static TemplateDictionary ReplyTemplates = new TemplateDictionary
        {
            ["default"] = new TemplateIdMap
                {
                    { SHOWALARMS, (context, data) =>
                        {
                            IEnumerable<Alarm> alarms = (IEnumerable<Alarm>)data;
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("# Current Alarms\n");
                            if (alarms.Any())
                            {
                                foreach(var alarm in alarms)
                                {
                                    sb.AppendLine($"* Title: {alarm.Title} Time: {alarm.Time}");
                                }
                            }
                            else
                                sb.AppendLine("*There are no alarms defined.*");
                            return sb.ToString();
                        }
                    }
                }
        };

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

            context.ReplyWith(SHOWALARMS, alarms);
            return Task.CompletedTask;
        }
    }
}
