using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("State Management")]
    public class ConversationStateTests
    {
        private class TestConversationState : ConversationState
        {
            public TestConversationState(IStorage storage)
                : base(storage)
            {
            }

            public TestConversationState(IStorage storage, int maxKeyLength)
                : base(storage, maxKeyLength) 
            {                
            }

            // This makes the GetStorageKey method public, so it's easier to test. 
            public string GetKey(ITurnContext turnContext) =>GetStorageKey(turnContext);
        }


        [TestMethod]        
        public void KeyLengthValidation()
        {
            var dictionary = new Dictionary<string, JObject>();
            var state = new ConversationState(new MemoryStorage(dictionary));
            Assert.AreEqual(254, state.MaxKeyLength, "Incorrect default key length");

            var state100 = new ConversationState(new MemoryStorage(dictionary), 100);
            Assert.AreEqual(100, state100.MaxKeyLength, "Key length must be 100");

            var state500 = new ConversationState(new MemoryStorage(dictionary), 500);
            Assert.AreEqual(500, state500.MaxKeyLength, "Key length must be 500");
        }

        [TestMethod]        
        public void KeyInRange()
        {
            var dictionary = new Dictionary<string, JObject>();
            var state = new TestConversationState(new MemoryStorage(dictionary));

            var context = TestUtilities.CreateEmptyContext();
            var key1 = state.GetKey(context);

            Assert.AreEqual("EmptyContext/conversations/test", key1, "Incorrect key");

            // Set the conversation Id to a long string
            context.Activity.Conversation.Id = new string('A', 500);
            var key2 = state.GetKey(context);

            // The resulting key should be a hash of the ConversationId. 
            var hashedConversationId = context.Activity.Conversation.Id.GetHashCode().ToString("x");
            var correctKey = $"{context.Activity.ChannelId}/conversations/{hashedConversationId}";
            Assert.AreEqual(correctKey, key2, "Incorrect key");
        }
    }
}
