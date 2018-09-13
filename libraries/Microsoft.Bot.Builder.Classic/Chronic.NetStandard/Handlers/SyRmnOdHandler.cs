using System;
using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;
using Chronic;

namespace Chronic.Handlers
{
    public class SyRmnOdHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var year = tokens[0].GetTag<ScalarYear>().Value;
            var month = tokens[1].GetTag<RepeaterMonthName>();
            var day = tokens[2].GetTag<OrdinalDay>().Value;
            var time_tokens = tokens.Skip(3).ToList();
            if (Time.IsMonthOverflow(year, (int)month.Value, day))
            {
                return null;
            }
            try
            {
                var dayStart = Time.New(year, (int)month.Value, day);
                return Utils.DayOrTime(dayStart, time_tokens, options);

            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}