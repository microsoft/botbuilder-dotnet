using System;
using System.Collections.Generic;
using System.Linq;
using Chronic;

namespace Chronic.Handlers
{
    public class SdSmSyHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var month = tokens[1].GetTag<ScalarMonth>().Value;
            var day = tokens[0].GetTag<ScalarDay>().Value;
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