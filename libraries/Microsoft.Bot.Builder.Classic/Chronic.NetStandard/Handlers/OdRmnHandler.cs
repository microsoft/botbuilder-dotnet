using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;
using Chronic;

namespace Chronic.Handlers
{
    public class OdRmnHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var month = tokens[1].GetTag<RepeaterMonthName>();
            var day = tokens[0].GetTag<OrdinalDay>().Value;
            var now = options.Clock();
            
            if (Time.IsMonthOverflow(now.Year, (int)month.Value, day))
            {
                return null;
            }
            return Utils.HandleMD(month, day, tokens.Skip(2).ToList(), options);
        }
    }
}