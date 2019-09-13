namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogEvent
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event will be bubbled to the parent `DialogContext` 
        /// if not handled by the current dialog.
        /// </summary>
        /// <value>
        /// Whether the event can be bubbled to the parent `DialogContext`.
        /// </value>
        public bool Bubble { get; set; }

        /// <summary>
        /// Gets or sets name of the event being raised.
        /// </summary>
        /// <value>
        /// Name of the event being raised.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets optional value associated with the event.
        /// </summary>
        /// <value>
        /// Optional value associated with the event.
        /// </value>
        public object Value { get; set; }
    }
}
