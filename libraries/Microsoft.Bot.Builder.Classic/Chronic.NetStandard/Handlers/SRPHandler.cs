using System.Collections.Generic;

namespace Chronic.Handlers
{
    public class SRPHandler : IHandler
    {
        public virtual Span Handle(IList<Token> tokens, Options options)
        {
            var now = options.Clock();
            var span = new Span(now, now.AddSeconds(1));
            return Handle(tokens, span, options);
        }

        public Span Handle(IList<Token> tokens, Span span, Options options)
        {
            var distance = tokens[0].GetTag<Scalar>().Value;
            var repeater = tokens[1].GetTag<IRepeater>();
            var pointer = tokens[2].GetTag<Pointer>().Value;
            return repeater.GetOffset(span, distance, pointer);
        }
    }
}