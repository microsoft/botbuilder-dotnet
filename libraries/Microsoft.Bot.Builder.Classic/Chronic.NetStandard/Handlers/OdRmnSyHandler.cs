using System;
using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;

namespace Chronic.Handlers
{
    public class OdRmnSyHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var day = tokens[0].GetTag<OrdinalDay>().Value;
            var month = (int) tokens[1].GetTag<RepeaterMonthName>().Value;
            var year = tokens[2].GetTag<ScalarYear>().Value;
            var timeTokens = tokens.Skip(3).ToList();
            if (Time.IsMonthOverflow(year, month, day))
            {
                return null;
            }

            try
            {
                var dayStart = Time.New(year, month, day);
                return Utils.DayOrTime(dayStart, timeTokens, options);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}