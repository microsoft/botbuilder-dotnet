using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.PublishValidation
{
    public enum NotificationMessageTypes
    {
        Warning = 0,
        Error = 1
    }

    public class NotificationMessage
    {
        public string message { get; set; }
        public int type { get; set; }

        public NotificationMessage(string message, int type)
        {
            this.message = message;
            this.type = type;
        }

        public override string ToString()
        {
            string notificationType = ((NotificationMessageTypes)this.type).ToString();
            return $"({ notificationType }): { this.message }";
        }
    }
}
