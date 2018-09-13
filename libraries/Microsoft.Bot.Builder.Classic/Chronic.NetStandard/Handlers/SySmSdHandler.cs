using System.Collections.Generic;
using System.Linq;

namespace Chronic.Handlers
{
    public class SySmSdHandler : SmSdSyHandler
    {
        public override Span Handle(IList<Token> tokens, Options options)
        {
            var newTokens = new List<Token>();
            newTokens.Add(tokens[1]);
            newTokens.Add(tokens[2]);
            newTokens.Add(tokens[0]);
            newTokens.AddRange(tokens.Skip(3));
            return base.Handle(newTokens, options);
        }
    }
}