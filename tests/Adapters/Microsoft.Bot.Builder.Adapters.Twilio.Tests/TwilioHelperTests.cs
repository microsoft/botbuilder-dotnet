// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioHelperTests
    {
        [Fact]
        public void ActivityToTwilio_Should_Return_MessageOptions_With_MediaUrl()
        {
            var activity = JsonConvert.DeserializeObject<Activity>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\files\Activities.json"));
            activity.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            var messageOption = TwilioHelper.ActivityToTwilio(activity, "123456789");

            Assert.Equal(activity.Conversation.Id, messageOption.ApplicationSid);
            Assert.Equal("123456789", messageOption.From.ToString());
            Assert.Equal(activity.Text, messageOption.Body);
            Assert.Equal(new Uri(activity.Attachments[0].ContentUrl), messageOption.MediaUrl[0]);
        }

        [Fact]
        public void ActivityToTwilio_Should_Return_EmptyMediaUrl_With_Null_ActivityAttachments()
        {
            var twilioNumber = "+12345678";
            var activity = new Activity()
            {
                Conversation = new ConversationAccount()
                {
                    Id = "testId",
                },
                Text = "Testing Null Attachments",
                Attachments = null,
            };

            var messageOptions = TwilioHelper.ActivityToTwilio(activity, twilioNumber);
            Assert.True(messageOptions.MediaUrl.Count == 0);
        }
    }
}
