using System.Collections.Generic;
using System.Linq;

namespace Chronic.Handlers
{
    public class MultiSRHandler : IHandler
    {
        public virtual Span Handle(IList<Token> tokens, Options options)
        {
            var now = options.Clock();
            var span = new Span(now, now.AddSeconds(1));

            var grabberTokens = tokens
                .SkipWhile(token => token.IsNotTaggedAs<Grabber>())
                .ToList();
            if (grabberTokens.Any())
            {
                span = grabberTokens.GetAnchor(options);
            }

            var scalarRepeaters = tokens
                .TakeWhile(token => token.IsNotTaggedAs<Pointer>())
                .Where(token => token.IsNotTaggedAs<SeparatorComma>())
                .ToList();

            var pointer = tokens.First(token => token.IsTaggedAs<Pointer>());

            for (var index = 0; index < scalarRepeaters.Count - 1; index++)
            {
                var scalar = scalarRepeaters[index];
                var repeater = scalarRepeaters[++index];
                span = Handle(new List<Token>{ scalar, repeater, pointer}, span, options);
            }

            return span;
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