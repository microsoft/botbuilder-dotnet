// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioHelperTests
    {
        private const string AuthTokenString = "authToken";
        private readonly Uri _validationUrlString = new Uri("http://contoso.com");

        [Fact]
        public void ActivityToTwilio_Should_Return_MessageOptions_With_MediaUrl()
        {
            var activity = JsonConvert.DeserializeObject<Activity>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\Activities.json")));
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

        [Fact]
        public void ActivityToTwilio_Should_Return_Empty_MediaUrl_With_Null_MediaUrls()
        {
            var activity = JsonConvert.DeserializeObject<Activity>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\Activities.json")));
            activity.Attachments = null;
            var messageOption = TwilioHelper.ActivityToTwilio(activity, "123456789");

            Assert.Equal(activity.Conversation.Id, messageOption.ApplicationSid);
            Assert.Equal("123456789", messageOption.From.ToString());
            Assert.Equal(activity.Text, messageOption.Body);
            Assert.Empty(messageOption.MediaUrl);
        }

        [Fact]
        public void ActivityToTwilio_Should_Return_Null_With_Null_Activity()
        {
            Assert.Null(TwilioHelper.ActivityToTwilio(null, "123456789"));
        }

        [Fact]
        public void ActivityToTwilio_Should_Return_Null_With_Empty_Or_Invalid_Number()
        {
            Assert.Null(TwilioHelper.ActivityToTwilio(default(Activity), "not_a_number"));
            Assert.Null(TwilioHelper.ActivityToTwilio(default(Activity), string.Empty));
        }

        [Fact]
        public void QueryStringToDictionary_Should_Return_Empty_Dictionary_With_Empty_Query()
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthTokenString));
            var builder = new StringBuilder(_validationUrlString.ToString());
            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            string hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);
            httpRequest.Object.Body = Stream.Null;

            var activity = TwilioHelper.RequestToActivity(httpRequest.Object, _validationUrlString, AuthTokenString);

            Assert.Null(activity.Id);
            Assert.Null(activity.Conversation.Id);
            Assert.Null(activity.From.Id);
            Assert.Null(activity.Recipient.Id);
            Assert.Null(activity.Text);
        }

        [Fact]
        public void QueryStringToDictionary_Should_Return_Dictionary_With_Valid_Query()
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthTokenString));
            var builder = new StringBuilder(_validationUrlString.ToString());

            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\NoMediaPayload.txt"));
            var byteArray = Encoding.ASCII.GetBytes(bodyString);
            var stream = new MemoryStream(byteArray);

            var values = new Dictionary<string, string>();

            var pairs = bodyString.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            var sortedKeys = new List<string>(values.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                builder.Append(key).Append(values[key] ?? string.Empty);
            }

            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var activity = TwilioHelper.RequestToActivity(httpRequest.Object, _validationUrlString, AuthTokenString);

            Assert.NotNull(activity.Id);
            Assert.NotNull(activity.Conversation.Id);
            Assert.NotNull(activity.From.Id);
            Assert.NotNull(activity.Recipient.Id);
            Assert.NotNull(activity.Text);
        }

        [Fact]
        public void RequestToActivity_Should_Return_Null_Activity_Attachments_With_NumMedia_EqualToZero()
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthTokenString));
            var builder = new StringBuilder(_validationUrlString.ToString());

            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\NoMediaPayload.txt"));
            var byteArray = Encoding.ASCII.GetBytes(bodyString);
            var stream = new MemoryStream(byteArray);

            var values = new Dictionary<string, string>();

            var pairs = bodyString.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            var sortedKeys = new List<string>(values.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                builder.Append(key).Append(values[key] ?? string.Empty);
            }

            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var activity = TwilioHelper.RequestToActivity(httpRequest.Object, _validationUrlString, AuthTokenString);

            Assert.Null(activity.Attachments);
        }

        [Fact]
        public void RequestToActivity_Should_Return_Activity_Attachments_With_NumMedia_GreaterThanZero()
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthTokenString));
            var builder = new StringBuilder(_validationUrlString.ToString());

            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\MediaPayload.txt"));
            var byteArray = Encoding.ASCII.GetBytes(bodyString);
            var stream = new MemoryStream(byteArray);

            var values = new Dictionary<string, string>();

            var pairs = bodyString.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            var sortedKeys = new List<string>(values.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                builder.Append(key).Append(values[key] ?? string.Empty);
            }

            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var activity = TwilioHelper.RequestToActivity(httpRequest.Object, _validationUrlString, AuthTokenString);

            Assert.NotNull(activity.Attachments);
        }

        [Fact]
        public void RequestToActivity_Should_NotThrow_KeyNotFoundException_With_NumMedia_GreaterThan_Attachments()
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthTokenString));
            var builder = new StringBuilder(_validationUrlString.ToString());

            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\MediaPayload.txt"));

            // Replace NumMedia with a number > the number of attachments
            bodyString = bodyString.Replace("NumMedia=1", "NumMedia=2");

            var byteArray = Encoding.ASCII.GetBytes(bodyString);
            var stream = new MemoryStream(byteArray);

            var values = new Dictionary<string, string>();

            var pairs = bodyString.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            var sortedKeys = new List<string>(values.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);

            foreach (var key in sortedKeys)
            {
                builder.Append(key).Append(values[key] ?? string.Empty);
            }

            var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var activity = TwilioHelper.RequestToActivity(httpRequest.Object, _validationUrlString, AuthTokenString);

            Assert.NotNull(activity.Attachments);
        }

        [Fact]
        public void RequestToActivity_Should_Return_Null_With_Null_HttpRequest()
        {
            Assert.Null(TwilioHelper.RequestToActivity(null, _validationUrlString, AuthTokenString));
        }

        [Fact]
        public void ValidateRequest_Should_Fail_With_NonMatching_Signature()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");
            httpRequest.Object.Body = Stream.Null;

            Assert.Throws<AuthenticationException>(() =>
            {
                TwilioHelper.RequestToActivity(httpRequest.Object, null, string.Empty);
            });
        }
    }
}
