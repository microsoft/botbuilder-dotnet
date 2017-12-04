using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot.Models
{
    /// <summary>
    /// shape of conversation state for the alarmbot
    /// </summary>
    public interface IAlarmBotConversationState
    {
        // active topic
        ITopic ActiveTopic { get; set; }
    }


}
