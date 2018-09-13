using System;
using System.Collections.Generic;

namespace Chronic.Handlers
{
    public class SmSyHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var month = (int)tokens[0].GetTag<ScalarMonth>().Value;
            var year = tokens[1].GetTag<ScalarYear>().Value;

            try
            {
                var start = Time.New(year, month);
                return new Span(start, start.AddMonths(1));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}