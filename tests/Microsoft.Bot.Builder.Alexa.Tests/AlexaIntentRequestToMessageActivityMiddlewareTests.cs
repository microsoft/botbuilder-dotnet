using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Alexa.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Alexa.Tests
{
    [TestClass]
    public class AlexaIntentRequestToMessageActivityMiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Default_Single_Phrase_Slot_To_Message_Activity_Text()
        {
            var requestJsonPath = Path.Combine(@"..\..\..\TestData\", "request_single_phrase_slot.json");
            var requestJson = new StreamReader(requestJsonPath).ReadToEnd();
            var request = JsonConvert.DeserializeObject<AlexaRequestBody>(requestJson);

            var activity = new Activity(
                type: "IntentRequest",
                id: "amzn1.echo-api.request.19b85614-9f9b-4bd8-bf37-604b7ff65a77",
                channelData: request);
            
            TestAdapter adapter = new TestAdapter(AlexaConversationReference)
                .Use(new AlexaIntentRequestToMessageActivityMiddleware());
            
            await new TestFlow(adapter, (context) =>
                {
                    context.SendActivity(context.Activity.AsMessageActivity().Text);
                    return Task.CompletedTask;
                })
                .Send(activity)
                .AssertReply("What time is my train tomorrow?")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Intent_And_Slot_Values_To_Message_Activity_Text()
        {
            var requestJsonPath = Path.Combine(@"..\..\..\TestData\", "request_multiple_slots.json");
            var requestJson = new StreamReader(requestJsonPath).ReadToEnd();
            var request = JsonConvert.DeserializeObject<AlexaRequestBody>(requestJson);

            var activity = new Activity(
                type: "IntentRequest",
                id: "amzn1.echo-api.request.19b85614-9f9b-4bd8-bf37-604b7ff65a77",
                channelData: request);

            TestAdapter adapter = new TestAdapter(AlexaConversationReference)
                .Use(new AlexaIntentRequestToMessageActivityMiddleware(RequestTransformPatterns.MessageActivityTextFromIntentAndAllSlotValues));

            await new TestFlow(adapter, (context) =>
                {
                    context.SendActivity(context.Activity.AsMessageActivity().Text);
                    return Task.CompletedTask;
                })
                .Send(activity)
                .AssertReply("Intent='GetUserIntent' phrase='What time is my train tomorrow?' slot2='Test second slot value'")
                .StartTest();
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Custom_Transform_To_Message_Activity_Text()
        {
            var requestJsonPath = Path.Combine(@"..\..\..\TestData\", "request_single_phrase_slot.json");
            var requestJson = new StreamReader(requestJsonPath).ReadToEnd();
            var request = JsonConvert.DeserializeObject<AlexaRequestBody>(requestJson);

            var activity = new Activity(
                type: "IntentRequest",
                id: "amzn1.echo-api.request.19b85614-9f9b-4bd8-bf37-604b7ff65a77",
                channelData: request);

            TestAdapter adapter = new TestAdapter(AlexaConversationReference)
                .Use(new AlexaIntentRequestToMessageActivityMiddleware(
                    (context, intentRequest) => 
                    $"Custom Transform: {intentRequest.Intent.Name} -> {intentRequest.Intent.Slots["phrase"].Value}"));

            await new TestFlow(adapter, (context) =>
                {
                    context.SendActivity(context.Activity.AsMessageActivity().Text);
                    return Task.CompletedTask;
                })
                .Send(activity)
                .AssertReply("Custom Transform: GetUserIntent -> What time is my train tomorrow?")
                .StartTest();
        }

        public static readonly ConversationReference AlexaConversationReference = new ConversationReference
        {
            ChannelId = "alexa",
            ServiceUrl = "amazon.com",
            User = new ChannelAccount("user1", "User1"),
            Bot = new ChannelAccount("bot", "Bot"),
            Conversation = new ConversationAccount(false, "convo1", "Conversation1")
        };

        public static readonly MediaTypeFormatter[] AlexaMessageMediaTypeFormatters = {
            new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                }
            }
        };
    }
}
