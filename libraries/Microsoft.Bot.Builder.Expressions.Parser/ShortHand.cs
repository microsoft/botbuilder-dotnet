namespace Microsoft.Bot.Builder.Expressions.Parser
{
    /// <summary>
    /// Bot Memory short hand.
    /// </summary>
    public class ShortHand
    {
        public ShortHand(string prefix, string functionName, ConvertShorthandName shorthandConvert)
        {
            Prefix = prefix;
            FunctionName = functionName;
            ConvertName = shorthandConvert;
        }

        /// <summary>
        /// Convert function delegate, from shorthand name to real memory name.
        /// </summary>
        /// <param name="disappealName">shorthand name.</param>
        /// <returns>real memory name.</returns>
        public delegate string ConvertShorthandName(string disappealName);

        /// <summary>
        /// Gets or sets shorthand prefix mark, like "@".
        /// </summary>
        /// <value>
        /// Shorthand prefix mark, like "@".
        /// </value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the short hand corresponding function name.
        /// </summary>
        /// <value>
        /// The short hand corresponding function name.
        /// </value>
        public string FunctionName { get; set; }

        /// <summary>
        /// Gets or sets convert function, from shorthand name to real memory name.
        /// </summary>
        /// <value>
        /// Convert function, from shorthand name to real memory name.
        /// </value>
        public ConvertShorthandName ConvertName { get; set; }
    }
}
