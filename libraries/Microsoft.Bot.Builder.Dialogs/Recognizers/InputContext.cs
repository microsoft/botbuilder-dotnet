// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Input context for the next turn.
    /// </summary>
    public class InputContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputContext"/> class.
        /// </summary>
        /// <param name="expected">Expected intents and entities.</param>
        /// <param name="possible">Possible intents and entities.</param>
        public InputContext(RecognizerDescription expected = null, RecognizerDescription possible = null)
        {
            Expected = expected ?? new RecognizerDescription();
            Possible = possible ?? new RecognizerDescription();
        } 
        
        /// <summary>
        /// Gets the description of the expected intents and entities for the next turn.
        /// </summary>
        /// <value><see cref="RecognizerDescription"/> of the expected intents and entities.</value>
        public RecognizerDescription Expected { get; }

        /// <summary>
        /// Gets the description of the possible intents and entities for the next turn.
        /// </summary>
        /// <value><see cref="RecognizerDescription"/> of the possible intents and entities.</value>
        public RecognizerDescription Possible { get; }
    }
}
