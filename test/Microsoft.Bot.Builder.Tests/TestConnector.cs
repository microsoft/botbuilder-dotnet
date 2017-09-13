using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public delegate void TestValidator(IList<IActivity> activities);

    public class ValidateOnPostConnector : Connector
    {
        private List<TestValidator> _validators = new List<TestValidator>();        

        public override async Task Post(IList<IActivity> activities, CancellationToken token)
        {
            Assert.IsTrue(_validators.Count > 0, "No Validators Present.");                
            foreach( var v in _validators)
            {
                v(activities);
            }            
        }

        public void ValidationsToRunOnPost(params TestValidator[] validators)
        {
            if (validators == null)
                throw new ArgumentNullException("validators");

            foreach (var v in validators)
                _validators.Add(v);
        }        
    }

    public class TestRunner
    {        
        public const string User = "testUser";
        public const string Bot = "testBot";        

        public static Activity MakeTestMessage()
        {
            return new Activity()
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = User },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = Bot },
                ServiceUrl = "InvalidServiceUrl",
                ChannelId = "Test",
                Attachments = Array.Empty<Attachment>(),
                Entities = Array.Empty<Entity>(),
            };
        }

        public async Task<TestRunner> Test(ValidateOnPostConnector c, string testMessage, CancellationToken token = default(CancellationToken))
        {            
            var message = MakeTestMessage();
            message.Text = testMessage;
            await c.Receive(message, token);

            return this;
        }
    }
}
