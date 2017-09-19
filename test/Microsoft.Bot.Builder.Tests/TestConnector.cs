using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    public delegate void TestValidator(IList<Activity> activities);

    public class TestConnector : Connector
    {        
        private List<TestValidator> _validators = new List<TestValidator>();
        int _nextId = 0;

        public TestConnector()
        {
            ConversationReference r = new ConversationReference
            {
                ChannelId = "test",
                ServiceUrl = "https://test.com"
            };

            r.User = new ChannelAccount("user1", "User1");
            r.Bot = new ChannelAccount("bot", "Bot");
            r.Conversation = new ConversationAccount(false, "convo1", "Conversation1");            

            Reference = r;
        }

        public TestConnector(ConversationReference reference)
        {
            Reference = reference;
        }

        public ConversationReference Reference { get; set; }        

        public Activity MakeTestActivity()
        {
            Activity a = new Activity
            {
                Type = ActivityTypes.Message,
                From = Reference.User,
                Recipient = Reference.Bot,
                Conversation = Reference.Conversation,
                ServiceUrl = Reference.ServiceUrl,
                Id = (_nextId++).ToString()
            };

            //Attachments = Array.Empty<Attachment>(),
            //Entities = Array.Empty<Entity>(),

            return a;
        }

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

        public async Task<TestRunner> Test(TestConnector c, string testMessage)
        {
            var message = c.MakeTestActivity();             
            message.Text = testMessage;
            await c.Receive(message, new CancellationToken());

            return this;

        }
        public async Task<TestRunner> Test(TestConnector c, string testMessage, TestValidator testFunction)
        {
            c.Clear();
            c.ValidationsToRunOnPost(testFunction);

            return await Test(c, testMessage);
        }
    }
}
