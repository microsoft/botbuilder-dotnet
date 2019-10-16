// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        private const string TwilioNumber = "+12345678";
        private readonly Uri _validationUrlString = new Uri("http://contoso.com");
        private readonly HMACSHA1 _hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthTokenString));

        [Fact]
        public void ActivityToTwilioShouldReturnMessageOptionsWithMediaUrl()
        {
            var activity = JsonConvert.DeserializeObject<Activity>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\Activities.json")));
            activity.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            var messageOption = TwilioHelper.ActivityToTwilio(activity, TwilioNumber);

            Assert.Equal(activity.Conversation.Id, messageOption.ApplicationSid);
            Assert.Equal(TwilioNumber, messageOption.From.ToString());
            Assert.Equal(activity.Text, messageOption.Body);
            Assert.Equal(new Uri(activity.Attachments[0].ContentUrl), messageOption.MediaUrl[0]);
        }

        [Fact]
        public void ActivityToTwilioShouldReturnEmptyMediaUrlWithNullActivityAttachments()
        {
            
            var activity = new Activity()
            {
                Conversation = new ConversationAccount()
                {
                    Id = "testId",
                },
                Text = "Testing Null Attachments",
                Attachments = null,
            };
            var messageOptions = TwilioHelper.ActivityToTwilio(activity, TwilioNumber);
            
            Assert.True(messageOptions.MediaUrl.Count == 0);
        }

        [Fact]
        public void ActivityToTwilioShouldReturnEmptyMediaUrlWithNullMediaUrls()
        {
            var activity = JsonConvert.DeserializeObject<Activity>(File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\files\Activities.json")));
            activity.Attachments = null;
            var messageOption = TwilioHelper.ActivityToTwilio(activity, TwilioNumber);

            Assert.Equal(activity.Conversation.Id, messageOption.ApplicationSid);
            Assert.Equal(TwilioNumber, messageOption.From.ToString());
            Assert.Equal(activity.Text, messageOption.Body);
            Assert.Empty(messageOption.MediaUrl);
        }

        [Fact]
        public void ActivityToTwilioShouldReturnNullWithNullActivity()
        {
            Assert.Null(TwilioHelper.ActivityToTwilio(null, TwilioNumber));
        }

        [Fact]
        public void ActivityToTwilioShouldReturnNullWithEmptyOrInvalidNumber()
        {
            Assert.Null(TwilioHelper.ActivityToTwilio(default(Activity), "not_a_number"));
            Assert.Null(TwilioHelper.ActivityToTwilio(default(Activity), string.Empty));
        }

        [Fact]
        public void QueryStringToDictionaryShouldReturnDictionaryWithValidQuery()
        {
            var builder = new StringBuilder(_validationUrlString.ToString());
            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\Files\NoMediaPayload.txt"));
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

            var hashArray = _hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var dictionary = TwilioHelper.QueryStringToDictionary();

            Assert.True(dictionary.ContainsKey("MessageSid"));
            Assert.True(dictionary.ContainsKey("From"));
            Assert.True(dictionary.ContainsKey("To"));
            Assert.True(dictionary.ContainsKey("Body"));
            /* ... */
            /* continue the rest of the properties in TwilioMessage */
        }

        [Fact]
        public void QueryStringToDictionaryShouldReturnEmptyDictionaryWithEmptyQuery()
        {
            var builder = new StringBuilder(_validationUrlString.ToString());
            var hashArray = _hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);
            var httpRequest = new Mock<HttpRequest>();
            
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);
            httpRequest.Object.Body = Stream.Null;

            var dictionary = TwilioHelper.QueryStringToDictionary();

            Assert.False(dictionary.ContainsKey("MessageSid"));
            Assert.False(dictionary.ContainsKey("From"));
            Assert.False(dictionary.ContainsKey("To"));
            Assert.False(dictionary.ContainsKey("Body"));
            /* ... */
            /* continue the rest of the properties in TwilioMessage */
        }

        [Fact]
        public async Task PayloadToActivityShouldReturnNullActivityAttachmentsWithNumMediaEqualToZero()
        {
            var builder = new StringBuilder(_validationUrlString.ToString());
            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\Files\NoMediaPayload.txt"));
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

            var hashArray = _hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var activity = await TwilioHelper.PayloadToActivity(httpRequest.Object);

            Assert.Null(activity.Attachments);
        }

        [Fact]
        public async Task PayloadToActivityShouldReturnActivityAttachmentsWithNumMediaGreaterThanZero()
        {
            var builder = new StringBuilder(_validationUrlString.ToString());
            var bodyString = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"\Files\MediaPayload.txt"));
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

            var hashArray = _hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;

            var activity = await TwilioHelper.PayloadToActivity(httpRequest.Object);

            Assert.NotNull(activity.Attachments);
        }

        [Fact]
        public async Task PayloadToActivityShouldNotThrowKeyNotFoundExceptionWithNumMediaGreaterThanAttachments()
        {
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

            var hashArray = _hmac.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            var hash = Convert.ToBase64String(hashArray);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns(hash);

            httpRequest.Object.Body = stream;
            
            var activity = await TwilioHelper.PayloadToActivity(httpRequest.Object);

            Assert.NotNull(activity.Attachments);
        }

        [Fact]
        public void PayloadToActivityShouldReturnNullWithNullBody()
        {
            Assert.Null(TwilioHelper.PayloadToActivity(null));
        }

        [Fact]
        public void ValidateRequestShouldFailWithNonMatchingSignature()
        {
            var httpRequest = new Mock<HttpRequest>();

            httpRequest.SetupAllProperties();
            httpRequest.SetupGet(req => req.Headers[It.IsAny<string>()]).Returns("wrong_signature");
            httpRequest.Object.Body = Stream.Null;

            Assert.Throws<AuthenticationException>(() =>
            {
                TwilioHelper.ValidateRequest(httpRequest.Object, null, _validationUrlString, AuthTokenString);
            });
        }
    }
}
