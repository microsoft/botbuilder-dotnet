// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Represents a point in a bot's logic, to help with bot debugging.
    /// </summary>
    /// <remarks>
    /// The trace activity typically is logged by transcript history components to become part of a
    /// transcript history. In remote debugging scenarios the trace activity can be sent to the client
    /// so that the activity can be inspected as part of the debug flow.
    ///
    /// Trace activities are normally not shown to the user, and are internal to transcript logging
    /// and developer debugging.
    ///
    /// See also InspectionMiddleware.
    /// </remarks>
    public interface ITraceActivity : IActivity
    {
        /// <summary>
        /// Gets or sets the name of the trace operation.
        /// </summary>
        /// <value>
        /// The name of the trace operation.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets an optional label which can provide contextual information about the trace.
        /// </summary>
        /// <value>
        /// A descriptive label for the trace.
        /// </value>
        string Label { get; set; }

        /// <summary>
        /// Gets or sets an optional identifier for the format of the <see cref="Value"/> property.
        /// </summary>
        /// <value>
        /// An optional identifier for the format of the value object.
        /// </value>
        /// <remarks>This property is optional, if the <see cref="Name"/> property adequately identifies
        /// the format of the <see cref="Value"/> property.</remarks>
        string ValueType { get; set; }

        /// <summary>
        /// Gets or sets the content for this trace, as defined by the <see cref="ValueType"/> or
        /// <see cref="Name"/> property.
        /// </summary>
        /// <value>
        /// The content for this trace.
        /// </value>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ConversationReference"/>, and optionally a specific activity
        /// within that conversation, that this trace is related to.
        /// </summary>
        /// <value>
        /// The conversation reference to which this trace activity relates.
        /// </value>
        ConversationReference RelatesTo { get; set; }
    }
}
