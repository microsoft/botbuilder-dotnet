using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class MyConversationIdFactory : ISkillConversationIdFactory
    {
        private readonly ConcurrentDictionary<(string, string), string> _forwardXref;
        private readonly ConcurrentDictionary<string, (string, string)> _backwardXref;

        public MyConversationIdFactory()
        {
            _forwardXref = new ConcurrentDictionary<(string, string), string>();
            _backwardXref = new ConcurrentDictionary<string, (string, string)>();
        }

        public string CreateSkillConversationId(string conversationId, string serviceUrl)
        {
            var result = _forwardXref.GetOrAdd((conversationId, serviceUrl), (key) => { return Guid.NewGuid().ToString(); });
            _backwardXref[result] = (conversationId, serviceUrl);
            return result;
        }

        public (string conversationId, string serviceUrl) GetConversationInfo(string encodedConversationId)
        {
            return _backwardXref[encodedConversationId];
        }
    }
}
