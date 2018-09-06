using System.Collections.Generic;
using System.Linq;

namespace Chronic.Handlers
{
    public class GRGRHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var outerSpan = tokens.Skip(2).Take(2).GetAnchor(options);
            return Utils.HandleGRR(tokens.Take(2).ToList(), outerSpan);
        }
    }
}