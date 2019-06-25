namespace Microsoft.Bot.Builder.Expressions.Parser
{
    /// <summary>
    /// Bot Memory shorthand entity.
    /// </summary>
    public class Shorthand
    {
        public Shorthand(string prefix, string functionName, NameExtracter extractName)
        {
            Prefix = prefix;
            BuildinFunction = functionName;
            ExtractName = extractName;
        }

        /// <summary>
        /// Convert function delegate, from shorthand name to real memory name.
        /// </summary>
        /// <param name="shorthandName">shorthand name.</param>
        /// <returns>real memory name.</returns>
        public delegate string NameExtracter(string shorthandName);

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
        public string BuildinFunction { get; set; }

        /// <summary>
        /// Gets or sets convert function, from shorthand name to real memory name.
        /// </summary>
        /// <value>
        /// Convert function, from shorthand name to real memory name.
        /// </value>
        public NameExtracter ExtractName { get; set; }
    }
}
