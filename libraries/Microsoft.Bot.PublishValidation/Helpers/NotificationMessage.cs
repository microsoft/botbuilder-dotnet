// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.PublishValidation
{
    public enum NotificationMessageTypes
    {
        Warning = 0,
        Error = 1
    }

    public enum BotServiceType
    {
        Endpoint,
        Luis,
        Qna,
        Dispatch
    }

    public enum Endpoints
    {
        Production
    }

    public class NotificationMessage
    {
        public NotificationMessage(string message, NotificationMessageTypes type)
        {
            Message = message;
            Type = type;
        }

        public string Message { get; set; }

        public NotificationMessageTypes Type { get; set; }

        public override string ToString()
        {
            var notificationType = Type.ToString();
            return $"{notificationType}: {Message}";
        }
    }
}
