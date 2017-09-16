using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public delegate void TestValidator(IList<Activity> activities);

    public class ValidateOnPostConnector : Connector
    {
        private List<TestValidator> _validators = new List<TestValidator>();        

        public override async Task Post(IList<Activity> activities, CancellationToken token)
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

        public void Clear()
        {
            _validators = new List<TestValidator>();
        }
    }

    public class TestRunner
    {        
        public const string User = "testUser";
        public const string Bot = "testBot";

        //public delegate void TestDelegate(IActivity activity);

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

        public async Task<TestRunner> Test(ValidateOnPostConnector c, string testMessage)
        {
            var message = MakeTestMessage();
            message.Text = testMessage;
            await c.Receive(message, new CancellationToken());

            return this;

        }
        public async Task<TestRunner> Test(ValidateOnPostConnector c, string testMessage, TestValidator testFunction)
        {
            c.Clear();
            c.ValidationsToRunOnPost(testFunction);

            return await Test(c, testMessage);
        }
    }
}
