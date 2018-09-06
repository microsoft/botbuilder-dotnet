using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;
using Chronic;

namespace Chronic.Handlers
{
    public class RmnSdOnHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            RepeaterMonthName month = null;
            int day = 0;
            IList<Token> remainingTokens = null;
            if (tokens.Count > 3)
            {
                month = tokens[2].GetTag<RepeaterMonthName>();
                day = tokens[3].GetTag<ScalarDay>().Value;
                remainingTokens = tokens.Take(2).ToList();
            }
            else
            {
                month = tokens[1].GetTag<RepeaterMonthName>();
                day = tokens[2].GetTag<ScalarDay>().Value;
                remainingTokens = tokens.Take(1).ToList();                
            }

            var now = options.Clock();
            if (Time.IsMonthOverflow(now.Year, (int)month.Value, day))
            {
                return null;
            }
            return Utils.HandleMD(month, day, remainingTokens, options);
        }
    }
}