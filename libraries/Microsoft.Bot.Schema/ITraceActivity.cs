namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// An activity by which a bot can log internal information into a logged conversation transcript.
    /// </summary>
    public interface ITraceActivity : IActivity
    {
        /// <summary>
        /// Gets or Sets Name of the trace activity.
        /// </summary>
        /// <value>
        /// Name of the trace activity.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or Sets Descriptive label for the trace.
        /// </summary>
        /// <value>
        /// Descriptive label for the trace.
        /// </value>
        string Label { get; set; }

        /// <summary>
        /// Gets or Sets Unique string which identifies the format of the value object.
        /// </summary>
        /// <value>
        /// Unique string which identifies the format of the value object.
        /// </value>
        string ValueType { get; set; }

        /// <summary>
        /// Gets or Sets Open-ended value.
        /// </summary>
        /// <value>
        /// Open-ended value.
        /// </value>
        object Value { get; set; }

        /// <summary>
        /// Gets or sets of Sets Open-ended value.
        /// </summary>
        /// <value>
        /// Open-ended value.
        /// </value>
        ConversationReference RelatesTo { get; set; }
    }
}
