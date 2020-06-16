// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// CardAction extension.
    /// </summary>
    public partial class CardAction
    {
        /// <summary>
        /// Implicit conversion of string to CardAction to simplify creation of
        /// CardActions with string values.
        /// </summary>
        /// <param name="input">input.</param>
        public static implicit operator CardAction(string input)
        {
            return new CardAction(title: input, value: input);
        }

        /// <summary>
        /// Creates a <see cref="CardAction"/> from the given input.
        /// </summary>
        /// <param name="input">Represents the title and value for the <see cref="CardAction"/>.</param>
        /// <returns>A new <see cref="CardAction"/> instance.</returns>
        public static CardAction FromString(string input)
        {
            return new CardAction(title: input, value: input);
        }
    }
}
