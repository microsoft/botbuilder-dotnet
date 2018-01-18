using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Asynchronous external event
    /// </summary>
    public interface IEventActivity : IActivity
    {
        /// <summary>
        /// Name of the event
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Open-ended value 
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Reference to another conversation or activity
        /// </summary>
        ConversationReference RelatesTo { get; set; }
    }

    /// <summary>
    /// NOTE: Trigger activity has been renamed to Event activity
    /// </summary>
    [Obsolete]
    public interface ITriggerActivity : IEventActivity
    {
    }
}
