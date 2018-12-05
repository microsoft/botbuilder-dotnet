// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.PublishValidation
{
    #region Enums

    public enum NotificationMessageTypes
    {
        Warning = 0,
        Error = 1
    }

    public enum BotServiceType
    {
        endpoint,
        luis,
        qna,
        dispatch
    }

    public enum Endpoints
    {
        production
    }

    #endregion

    public class NotificationMessage
    {
        public string Message { get; set; }
        public NotificationMessageTypes Type { get; set; }

        public NotificationMessage(string message, NotificationMessageTypes type)
        {
            Message = message;
            Type = type;
        }

        public override string ToString()
        {
            var notificationType = Type.ToString();
            return $"{ notificationType }: { Message }";
        }
    }
}
