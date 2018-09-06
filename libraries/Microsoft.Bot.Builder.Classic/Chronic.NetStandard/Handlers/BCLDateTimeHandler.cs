using System;
using System.Collections.Generic;

namespace Chronic.Handlers
{
    /// <summary>
    /// System.DateTime.Parse based handler. To be used as a fallback mechanism.
    /// </summary>
    public class BCLDateTimeHandler : IHandler
    {
        public Span Handle(IList<Token> tokens, Options options)
        {
            var time = DateTime.Parse(options.OriginalPhrase);
            return new Span(time, time.AddSeconds(1));
        }
    }
}