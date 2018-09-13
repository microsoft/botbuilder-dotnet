using System;
using System.Collections.Generic;

namespace Chronic.Handlers
{
    public class Utils
    {
        public static Span HandleORR(IList<Token> tokens, Span outerSpan,
                                     Options options)
        {
            var repeater = tokens[1].GetTag<IRepeater>();
            repeater.Now = outerSpan.Start.Value.AddSeconds(-1);
            var ordinal = tokens[0].GetTag<Ordinal>().Value;
            Span span = null;
            for (var i = 0; i < ordinal; i++)
            {
                span = repeater.GetNextSpan(Pointer.Type.Future);
                if (span.Start > outerSpan.End)
                {
                    span = null;
                    break;
                }
            }
            return span;
        }

        public static Span HandleGRR(IList<Token> tokens, Span outerSpan)
        {
            var grabber = tokens[0].GetTag<Grabber>().Value;
            var repeater = tokens[1].GetTag<IRepeater>();
            Span span = null;
            if (grabber == Grabber.Type.Last)
            {
                repeater.Now = outerSpan.End.Value;
                span = repeater.GetNextSpan(Pointer.Type.Past);
            }

            return span;
        }

        public static Span DayOrTime(DateTime dayStart, IList<Token> timeTokens,
                                     Options options)
        {
            var outerSpan = new Span(dayStart, dayStart.AddDays(1));
            if (timeTokens.Count > 0)
            {
                options.Clock = () => outerSpan.Start.Value;
                var time = timeTokens
                    .DealiasAndDisambiguateTimes(options)
                    .GetAnchor(options);
                return time;
            }
            return outerSpan;
        }

        public static Span HandleMD(IRepeater month, int day,
                                    IList<Token> timeTokens, Options options)
        {
            month.Now = options.Clock();
            var span = month.GetCurrentSpan(options.Context);
            var date = span.Start.Value;
            var dayStart = Time.New(date.Year, date.Month, day);
            return Utils.DayOrTime(dayStart, timeTokens, options);
        }



    }
}