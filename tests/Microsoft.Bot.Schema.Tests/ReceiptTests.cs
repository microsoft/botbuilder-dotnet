// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class ReceiptTests
    {
        [Fact]
        public void ReceiptCardInits()
        {
            var title = "receiptTitle";
            var facts = new List<Fact>() { new Fact("key", "value") };
            var items = new List<ReceiptItem>() { new ReceiptItem("title", "subtitle") };
            var tap = new CardAction("type", "title", "http://example.com", "text", "displayText");
            var total = "100.00";
            var tax = "10.00";
            var vat = "0.00";
            var buttons = new List<CardAction>();

            var receiptCard = new ReceiptCard(title, facts, items, tap, total, tax, vat, buttons);

            Assert.NotNull(receiptCard);
            Assert.IsType<ReceiptCard>(receiptCard);
            Assert.Equal(title, receiptCard.Title);
            Assert.Equal(facts, receiptCard.Facts);
            Assert.Equal(items, receiptCard.Items);
            Assert.Equal(tap, receiptCard.Tap);
            Assert.Equal(total, receiptCard.Total);
            Assert.Equal(tax, receiptCard.Tax);
            Assert.Equal(vat, receiptCard.Vat);
            Assert.Equal(buttons, receiptCard.Buttons);
        }
        
        [Fact]
        public void ReceiptCardInitsWithNoArgs()
        {
            var receiptCard = new ReceiptCard();

            Assert.NotNull(receiptCard);
            Assert.IsType<ReceiptCard>(receiptCard);
        }

        [Fact]
        public void ReceiptItemInits()
        {
            var title = "title";
            var subtitle = "subtitle";
            var text = "text";
            var image = new CardImage("http://example.com", "example image");
            var price = "$20.00";
            var quantity = "5";
            var tap = new CardAction();

            var receiptItems = new ReceiptItem(title, subtitle, text, image, price, quantity, tap);

            Assert.NotNull(receiptItems);
            Assert.IsType<ReceiptItem>(receiptItems);
            Assert.Equal(title, receiptItems.Title);
            Assert.Equal(subtitle, receiptItems.Subtitle);
            Assert.Equal(text, receiptItems.Text);
            Assert.Equal(image, receiptItems.Image);
            Assert.Equal(price, receiptItems.Price);
            Assert.Equal(quantity, receiptItems.Quantity);
            Assert.Equal(tap, receiptItems.Tap);
        }
        
        [Fact]
        public void ReceiptItemInitsWithNoArgs()
        {
            var receiptItems = new ReceiptItem();

            Assert.NotNull(receiptItems);
            Assert.IsType<ReceiptItem>(receiptItems);
        }
    }
}
