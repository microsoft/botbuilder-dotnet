using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// A bot was installed or removed from a channel
    /// </summary>
    public interface IInstallationUpdateActivity : IActivity
    {
        /// <summary>
        /// add|remove
        /// </summary>
        string Action { get; set; }
    }
}
