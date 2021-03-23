using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Bot component attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class BotComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotComponentAttribute"/> class.
        /// </summary>
        /// <param name="type"><see cref="Type"/> for the bot component entry point. Must inherit <see cref="ComponentRegistration"/>.</param>
        public BotComponentAttribute(Type type) 
        {
            ComponentType = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the bot component entry point.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> for the bot component entry point.
        /// </value>
        public Type ComponentType { get; private set; }
    }
}
