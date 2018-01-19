using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Defines the importance of an Activity
    /// </summary>
    public static class ActivityImportance
    {
        /// <summary>
        /// Low importance.
        /// </summary>
        public const string Low = "low";

        /// <summary>
        /// Normal importance.
        /// </summary>
        public const string Normal = "normal";

        /// <summary>
        /// High importance.
        /// </summary>
        public const string High = "high";
    }
}
