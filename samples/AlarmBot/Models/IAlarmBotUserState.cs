using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot.Models
{
    /// <summary>
    /// Shape of alarmbot data for user state
    /// </summary>
    public interface IAlarmBotUserState
    { 
        /// <summary>
        ///  saved alarms
        /// </summary>
        List<Alarm> Alarms { get; set; }
    }
}
