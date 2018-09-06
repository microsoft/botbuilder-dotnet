using System;
using System.Collections.Generic;
using Chronic.Tags.Repeaters;
using Chronic;

namespace Chronic.Handlers
{
    public class RmnSyHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var month = (int)tokens[0].GetTag<RepeaterMonthName>().Value;
            var year = (int)tokens[1].GetTag<ScalarYear>().Value;

            int next_month_year = 0;
            int next_month_month = 0;
            if (month == 12)
            {
                next_month_year = year + 1;
                next_month_month = 1;
            }
            else
            {
                next_month_year = year;
                next_month_month = month + 1;
            }

            try
            {
                var endTime = Time.New(next_month_year, next_month_month);
                return new Span(Time.New(year, month), endTime);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}