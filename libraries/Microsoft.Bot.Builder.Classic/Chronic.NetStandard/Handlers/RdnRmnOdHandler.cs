using System;
using Chronic.Tags.Repeaters;

namespace Chronic.Handlers
{
    public class RdnRmnOdHandler : IHandler
    {
        public Span Handle(System.Collections.Generic.IList<Token> tokens,
                           Options options)
        {
            var month = tokens[1].GetTag<RepeaterMonthName>();
            var day = tokens[2].GetTag<OrdinalDay>().Value;
            var year = options.Clock().Year;

            if (Time.IsMonthOverflow(year, (int) month.Value, day))
            {
                return null;
            }
            try
            {
                var start = Time.New(year, (int) month.Value, day);
                var end = start.AddDays(1);
                return new Span(start, end);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}