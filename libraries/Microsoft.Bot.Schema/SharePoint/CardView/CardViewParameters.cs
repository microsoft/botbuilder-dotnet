// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Adaptive Card Extension Card View Parameters.
    /// </summary>
    public class CardViewParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardViewParameters"/> class.
        /// </summary>
        protected CardViewParameters() 
        {
        }

        /// <summary>
        /// Gets or sets card view type.
        /// </summary>
        /// <value>Card view type.</value>
        [JsonProperty(PropertyName = "cardViewType")]
        public string CardViewType { get; set; }

        /// <summary>
        /// Gets or sets image displayed on the card.
        /// </summary>
        /// <value>Image displayed on the card.</value>
        [JsonProperty(PropertyName = "image")]
        public CardImage Image { get; set; }

        /// <summary>
        /// Gets or sets card view title area (card bar) components.
        /// </summary>
        /// <value>Card bar area components.</value>
        [JsonProperty(PropertyName = "cardBar")]
#pragma warning disable CA2227
        public IList<CardBarComponent> CardBar { get; set; }
#pragma warning restore CA2227

        /// <summary>
        /// Gets or sets card view header area components.
        /// </summary>
        /// <value>Card header area components.</value>
        [JsonProperty(PropertyName = "header")]
        public IEnumerable<BaseCardComponent> Header { get; set; }

        /// <summary>
        /// Gets or sets card view body area components.
        /// </summary>
        /// <value>Card body area components.</value>
        [JsonProperty(PropertyName = "body")]
        public IEnumerable<BaseCardComponent> Body { get; set; }

        /// <summary>
        /// Gets or sets card footer area components.
        /// </summary>
        /// <value>Card footer area components.</value>
        [JsonProperty(PropertyName = "footer")]
        public IEnumerable<BaseCardComponent> Footer { get; set; }

        /// <summary>
        /// Helper method to create a Basic Text Card View.
        /// </summary>
        /// <param name="cardBar">Card bar component.</param>
        /// <param name="header">Text component to display as header.</param>
        /// <param name="footer">Up to two buttons or text input to display as footer.</param>
        /// <returns>Card view configuration.</returns>
        /// <remarks>The Basic Text card view displays the following:
        /// - Card bar
        /// - One primary text field
        /// - Zero or one button in the Medium card size, up to two buttons in Large card size; or text input.
        /// </remarks>
        public static CardViewParameters PrimaryTextCardViewParameters(
            CardBarComponent cardBar,
            CardTextComponent header,
            IList<BaseCardComponent> footer)
        {
            // Validate parameters
            if (cardBar == null)
            {
                throw new ArgumentNullException(nameof(cardBar));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            ValidateGenericCardViewFooterConfiguration(footer);

            return new CardViewParameters()
            {
                CardViewType = "text",
                CardBar = new List<CardBarComponent> { cardBar },
                Header = new List<CardTextComponent> { header },
                Footer = footer
            };
        }

        /// <summary>
        /// Helper method to create a Primary Text Card View.
        /// </summary>
        /// <param name="cardBar">Card bar component.</param>
        /// <param name="header">Text component to display as header.</param>
        /// <param name="body">Text component to display as body.</param>
        /// <param name="footer">Up to two buttons or text input to display as footer.</param>
        /// <returns>Card view configuration.</returns>
        /// <remarks>The Primary Text card view displays the following:
        /// - Card bar
        /// - One primary text field
        /// - One description text field
        /// - Zero or one button in the Medium card size, up to two buttons in Large card size; or text input.
        /// </remarks>
        public static CardViewParameters PrimaryTextCardViewParameters(
            CardBarComponent cardBar,
            CardTextComponent header,
            CardTextComponent body,
            IList<BaseCardComponent> footer)
        {
            // Validate parameters
            if (cardBar == null)
            {
                throw new ArgumentNullException(nameof(cardBar));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            ValidateGenericCardViewFooterConfiguration(footer);

            return new CardViewParameters()
            {
                CardViewType = "text",
                CardBar = new List<CardBarComponent> { cardBar },
                Header = new List<CardTextComponent> { header },
                Body = new List<CardTextComponent> { body },
                Footer = footer
            };
        }

        /// <summary>
        /// Helper method to create an Image Card View.
        /// </summary>
        /// <param name="cardBar">Card bar component.</param>
        /// <param name="header">Text component to display as header.</param>
        /// <param name="footer">Up to two buttons or text input to display as footer.</param>
        /// <param name="image">Image to display.</param>
        /// <returns>Card view configuration.</returns>
        /// <remarks>The Image Card view displays the following:
        /// - Card bar
        /// - One primary text field
        /// - One image
        /// - Zero buttons in the Medium card size, up to two buttons in Large card size; or text input.
        /// </remarks>
        public static CardViewParameters ImageCardViewParameters(
            CardBarComponent cardBar,
            CardTextComponent header,
            IList<BaseCardComponent> footer,
            CardImage image)
        {
            // Validate parameters
            if (cardBar == null)
            {
                throw new ArgumentNullException(nameof(cardBar));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            ValidateGenericCardViewFooterConfiguration(footer);

            return new CardViewParameters()
            {
                CardViewType = "text",
                CardBar = new List<CardBarComponent> { cardBar },
                Header = new List<CardTextComponent> { header },
                Image = image,
                Footer = footer
            };
        }

        /// <summary>
        /// Helper method to create a Text Input Card View.
        /// </summary>
        /// <param name="cardBar">Card bar component.</param>
        /// <param name="header">Text component to display as header.</param>
        /// <param name="body">Text input component to display as body.</param>
        /// <param name="footer">Up to two buttons to display as footer.</param>
        /// <param name="image">Optional image to display.</param>
        /// <returns>Card view configuration.</returns>
        /// /// <remarks>The Text Input Card view displays the following:
        /// - Card bar
        /// - One primary text field
        /// - Zero or one image
        /// - Zero text input in Medium card size if image is presented, one text input in Medium card size if no image is presented, one text input in Large card size
        /// - Zero buttons in the Medium card size if image is presented, one button in Medium card size if no image is presented, up to two buttons in Large card size; or text input.
        /// </remarks>
        public static CardViewParameters TextInputCardViewParameters(
            CardBarComponent cardBar,
            CardTextComponent header,
            CardTextInputComponent body,
            IList<CardButtonComponent> footer,
            CardImage image)
        {
            // Validate parameters
            if (cardBar == null)
            {
                throw new ArgumentNullException(nameof(cardBar));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (footer.Count > 2)
            {
                throw new ArgumentException("Card view footer must contain up to two buttons.", nameof(footer));
            }

            return new CardViewParameters()
            {
                CardViewType = "textInput",
                CardBar = new List<CardBarComponent> { cardBar },
                Header = new List<CardTextComponent> { header },
                Body = new List<CardTextInputComponent> { body },
                Image = image,
                Footer = footer
            };
        }

        /// <summary>
        /// Helper method to create a Search Card View.
        /// </summary>
        /// <param name="cardBar">Card bar component.</param>
        /// <param name="header">Text component to display as header.</param>
        /// <param name="body">Search box to display as body.</param>
        /// <param name="footer">Search footer component to display as footer.</param>
        /// <returns>Card view configuration.</returns>
        /// /// <remarks>The Search Card view displays the following:
        /// - Card bar
        /// - One primary text field
        /// - One search box
        /// - One search box footer.
        /// </remarks>
        public static CardViewParameters SearchCardViewParameters(
            CardBarComponent cardBar,
            CardTextComponent header,
            CardSearchBoxComponent body,
            CardSearchFooterComponent footer)
        {
            // Validate parameters
            if (cardBar == null)
            {
                throw new ArgumentNullException(nameof(cardBar));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (footer == null)
            {
                throw new ArgumentNullException(nameof(footer));
            }

            return new CardViewParameters()
            {
                CardViewType = "search",
                CardBar = new List<CardBarComponent> { cardBar },
                Header = new List<CardTextComponent> { header },
                Body = new List<CardSearchBoxComponent> { body },
                Footer = new List<CardSearchFooterComponent> { footer }
            };
        }

        /// <summary>
        /// Helper method to create a Sign In Card View.
        /// </summary>
        /// <param name="cardBar">Card bar component.</param>
        /// <param name="header">Text component to display as header.</param>
        /// <param name="body">Text component to display as body.</param>
        /// <param name="footer">Sign in button.</param>
        /// <returns>Card view configuration.</returns>
        /// <remarks>Only Title property of the Sign in button is used.</remarks>
        public static CardViewParameters SignInCardViewParameters(
            CardBarComponent cardBar,
            CardTextComponent header,
            CardTextComponent body,
            CardButtonComponent footer)
        {
            // Validate parameters
            if (cardBar == null)
            {
                throw new ArgumentNullException(nameof(cardBar));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (footer == null)
            {
                throw new ArgumentNullException(nameof(footer));
            }

            return new CardViewParameters()
            {
                CardViewType = "signIn",
                CardBar = new List<CardBarComponent> { cardBar },
                Header = new List<CardTextComponent> { header },
                Body = new List<CardTextComponent> { body },
                Footer = new List<CardButtonComponent> { footer }
            };
        }

        private static void ValidateGenericCardViewFooterConfiguration(IList<BaseCardComponent> footer)
        {
            if (footer == null)
            {
                // footer can be empty
                return;
            }

            int componentsCount = footer.Count;

            bool hasError;

            if (componentsCount == 0)
            {
                return;
            }
            else if (componentsCount > 2)
            {
                // we don't support more than 2 components in the footer.
                hasError = true;
            }
            else if (componentsCount == 2)
            {
                // both components should be buttons.
                hasError = !(footer[0] is CardButtonComponent) || !(footer[1] is CardButtonComponent); 
            }
            else
            {
                // single component should be either a button or a text input
                hasError = !(footer[0] is CardButtonComponent) || !(footer[0] is CardTextInputComponent);
            }

            if (hasError)
            {
                throw new ArgumentException("Card view footer must contain up to two buttons or text input", nameof(footer));
            }
        }
    }
}
