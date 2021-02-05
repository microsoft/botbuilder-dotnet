// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A card representing a request to sign in.
    /// </summary>
    public partial class SigninCard
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
        public SigninCard(string text = default(string), IList<CardAction> buttons = default(IList<CardAction>))
        {
            Text = text;
            Buttons = buttons;
        }

        /// <summary>
        /// Gets or sets text for sign-in request.
        /// </summary>
        /// <value>The text for the sign-in request.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets action to use to perform signin.
        /// </summary>
        /// <value>The action(s) to use to perform sign-in.</value>
        [JsonProperty(PropertyName = "buttons")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<CardAction> Buttons { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

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
