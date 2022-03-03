// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A card representing a request to sign in.
    /// </summary>
    public class SigninCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SigninCard"/> class.
        /// </summary>
        public SigninCard()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigninCard"/> class.
        /// </summary>
        /// <param name="text">Text for signin request.</param>
        /// <param name="buttons">Action to use to perform signin.</param>
        public SigninCard(string text = default, IList<CardAction> buttons = default)
        {
            Text = text;
            Buttons = buttons ?? new List<CardAction>();
        }

        /// <summary>
        /// Gets or sets text for sign-in request.
        /// </summary>
        /// <value>The text for the sign-in request.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets action to use to perform signin.
        /// </summary>
        /// <value>The action(s) to use to perform sign-in.</value>
        [JsonProperty(PropertyName = "buttons")]
        public IList<CardAction> Buttons { get; private set; } = new List<CardAction>();

        /// <summary>
        /// Creates a <see cref="SigninCard"/>.
        /// </summary>
        /// <param name="text"> The <see cref="Text"/>text.</param>
        /// <param name="buttonLabel"> The signin button label.</param>
        /// <param name="url"> The sigin url.</param>
        /// <returns> The created sigin card.</returns>
        public static SigninCard Create(string text, string buttonLabel, string url)
        {
            return new SigninCard
            {
                Text = text,
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                       Title = buttonLabel,
                       Type = ActionTypes.Signin,
                       Value = url,
                    },
                },
            };
        }
    }
}
