using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;

namespace Chronic.Handlers
{
    public class RmnOdOnHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            IRepeater month = null;
            var day = 0;
            IList<Token> remainingTokens = null;
            if (tokens.Count > 3)
            {
                month = tokens[2].GetTag<RepeaterMonthName>();
                day = tokens[3].GetTag<OrdinalDay>().Value;
                remainingTokens = tokens.Take(2).ToList();
            }
            else
            {
                month = tokens[1].GetTag<RepeaterMonthName>();
                day = tokens[2].GetTag<OrdinalDay>().Value;
                remainingTokens = tokens.Take(1).ToList();
            }
            if (Time.IsMonthOverflow(options.Clock().Year, (int)month.RawValue, day))
            {
                return null;
            }
            return Utils.HandleMD(month, day, remainingTokens, options);

        }
    }
}