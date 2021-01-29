using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Transport
{
    /// <summary>
    /// Encapsulate the transport between Debug Client (e.g. Visual Studio Code)
    /// and Debug Adapter (i.e. running within the bot code).
    /// </summary>
    internal interface IDebugTransport
    {
        /// <summary>
        /// Gets or sets the callback for accepting a new connection.
        /// Single subscriber event, required to break circular dependency.
        /// </summary>
        /// <value>
        /// The accept callback.
        /// </value>
        Func<CancellationToken, Task> Accept { get; set; }

        /// <summary>
        /// Reads the next token from the transport.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing completion of the wait for the next token.</returns>
        Task<JToken> ReadAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends the next token to the transport.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing completion of the send.</returns>
        Task SendAsync(JToken token, CancellationToken cancellationToken);
    }
}
