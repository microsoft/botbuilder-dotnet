using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public static class LGExtension
    {
        public static IList<LGTemplate> MarkSource(this IList<LGTemplate> templates, string source)
        {
            return templates.Select(u =>
            {
                u.Source = source;
                return u;
            }).ToList();
        }
    }
}
