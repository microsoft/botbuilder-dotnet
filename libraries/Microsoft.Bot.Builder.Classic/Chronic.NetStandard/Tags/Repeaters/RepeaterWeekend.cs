using System;
using System.Globalization;
using Chronic;
using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterWeekend : RepeaterUnit
    {
        public static readonly int WEEKEND_SECONDS = 172800; // (2 * 24 * 60 * 60);
        private DateTime? _start;

        public RepeaterWeekend()
            : base(UnitName.Weekend)
        {

        }

        public override int GetWidth()
        {
            return RepeaterWeekend.WEEKEND_SECONDS;
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            if (_start == null)
            {
                if (pointer == Pointer.Type.Future)
                {
                    var saturdayRepeater = new RepeaterDayName(DayOfWeek.Saturday);
                    saturdayRepeater.Now = Now;
                    var nextSaturdaySpan = saturdayRepeater.GetNextSpan(Pointer.Type.Future);
                    _start = nextSaturdaySpan.Start.Value;
                }
                else if (pointer == Pointer.Type.Past)
                {
                    var saturdayRepeater = new RepeaterDayName(DayOfWeek.Saturday);
                    saturdayRepeater.Now = Now.Value.AddSeconds(RepeaterDay.DAY_SECONDS);
                    var lastSaturdaySpan = saturdayRepeater.GetNextSpan(Pointer.Type.Past);
                    _start = lastSaturdaySpan.Start.Value;
                }
            }
            else
            {
                var direction = (pointer == Pointer.Type.Future) ? 1 : -1;
                _start = _start.Value.AddSeconds(direction * RepeaterWeek.WEEK_SECONDS);
            }
            return new Span(
                _start.Value,
                _start.Value.AddSeconds(RepeaterWeekend.WEEKEND_SECONDS));
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {

            Span thisSpan;
            if (pointer == Pointer.Type.Future || pointer == Pointer.Type.None)
            {
                var saturdayRepeater = new RepeaterDayName(DayOfWeek.Saturday);
                saturdayRepeater.Now = Now;
                var thisSaturdaySpan = saturdayRepeater.GetNextSpan(Pointer.Type.Future);
                thisSpan = new Span(
                    thisSaturdaySpan.Start.Value,
                    thisSaturdaySpan.Start.Value.AddSeconds(WEEKEND_SECONDS));
            }
            else if (pointer == Pointer.Type.Past)
            {
                var saturdayRepeater = new RepeaterDayName(DayOfWeek.Saturday);
                saturdayRepeater.Now = Now;
                var lastSaturdaySpan = saturdayRepeater.GetNextSpan(Pointer.Type.Past);
                thisSpan = new Span(
                    lastSaturdaySpan.Start.Value,
                    lastSaturdaySpan.Start.Value.AddSeconds(RepeaterWeekend.WEEKEND_SECONDS));
            }
            else
            {
                throw new ArgumentException("Unable to handle pointer " + pointer + ".", "pointer");
            }
            return thisSpan;

        }

        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            var direction = (pointer == Pointer.Type.Future) ? 1 : -1;
            var weekend = new RepeaterWeekend();
            weekend.Now = span.Start;
            var start = weekend
                .GetNextSpan(pointer)
                .Start.Value
                .AddSeconds((amount - 1) * direction * RepeaterWeek.WEEK_SECONDS);

            return new Span(start, start.AddSeconds(span.Width));

        }
    }
}