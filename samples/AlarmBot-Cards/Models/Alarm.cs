using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot.Models
{
    /// <summary>
    /// Alarm class the AlarmBot creates
    /// </summary>
    public class Alarm
    {
        public string Title { get; set; }
        public DateTimeOffset? Time { get; set; }
    }

}
