using System.Collections.Generic;
using System.Linq;

namespace Chronic.Handlers
{
    public class ORSRHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var outerSpan = new List<Token> {tokens[3]}.GetAnchor(options);
            return Utils.HandleORR(tokens.Take(2).ToList(), outerSpan, options);
        }
    }
}