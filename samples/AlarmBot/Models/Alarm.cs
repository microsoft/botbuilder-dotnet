// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

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
