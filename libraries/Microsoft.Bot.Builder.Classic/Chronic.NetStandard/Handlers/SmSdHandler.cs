using System.Collections.Generic;
using Chronic;

namespace Chronic.Handlers
{
    public class SmSdHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var month = (int)tokens[0].GetTag<ScalarMonth>().Value;
            var day = (int)tokens[1].GetTag<ScalarDay>().Value;
            var now = options.Clock();
            var start = Time.New(now.Year, month, day);
            var end = start.AddMonths(1);
            return new Span(start, end);
        }
    }
}