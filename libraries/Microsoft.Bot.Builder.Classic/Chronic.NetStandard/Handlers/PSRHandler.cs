using System.Collections.Generic;

namespace Chronic.Handlers
{
    public class PSRHandler : SRPHandler
    {
        public override Span Handle(IList<Token> tokens, Options options)
        {
            var tokensToHandle = new List<Token> { tokens[1], tokens[2], tokens[0] };
            return base.Handle(tokensToHandle, options);
        }
    }
}