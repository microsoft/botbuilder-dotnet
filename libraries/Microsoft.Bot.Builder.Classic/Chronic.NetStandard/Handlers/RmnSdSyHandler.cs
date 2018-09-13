using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Chronic.Tags.Repeaters;
using Chronic;

namespace Chronic.Handlers
{
    public class RmnSdSyHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var month = (int)tokens[0].GetTag<RepeaterMonthName>().Value;
            var day = tokens[1].GetTag<ScalarDay>().Value;
            var year = tokens[2].GetTag<ScalarYear>().Value;

            Span span = null;
            try
            {
                var timeTokens = tokens.Skip(3).ToList();
                var dayStart = Time.New(year, month, day);
                span = Utils.DayOrTime(dayStart, timeTokens, options);
            }
            catch (ArgumentException e)
            {
                span = null;
            }
            return span;
        }
    }
}