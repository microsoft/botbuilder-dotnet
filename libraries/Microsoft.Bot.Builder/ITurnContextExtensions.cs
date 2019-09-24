// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TraceExtensions
{
    /// <summary>
    /// Contains methods for working with <see cref="ITurnContext"/> objects.
    /// </summary>
    public static class ITurnContextExtensions
    {
        /// <summary>
        /// Sends a trace activity to the <see cref="BotAdapter"/> for logging purposes.
        /// </summary>
        /// <param name="turnContext">The context for the current turn.</param>
        /// <param name="name">The value to assign to the activity's <see cref="Activity.Name"/> property.</param>
        /// <param name="value">The value to assign to the activity's <see cref="Activity.Value"/> property.</param>
        /// <param name="valueType">The value to assign to the activity's <see cref="Activity.ValueType"/> property.</param>
        /// <param name="label">The value to assign to the activity's <see cref="Activity.Label"/> property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the adapter is being hosted in the Emulator, the task result contains
        /// a <see cref="ResourceResponse"/> object with the original trace activity's ID; otherwise,
        /// it contains a <see cref="ResourceResponse"/> object containing the ID that the receiving
        /// channel assigned to the activity.</remarks>
        public static Task<ResourceResponse> TraceActivityAsync(this ITurnContext turnContext, string name, object value = null, string valueType = null, [CallerMemberName] string label = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return turnContext.SendActivityAsync(turnContext.Activity.CreateTrace(name, value, valueType, label), cancellationToken);
        }
    }
}
