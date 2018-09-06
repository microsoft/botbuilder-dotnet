using System;
using Chronic;
using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterHour : RepeaterUnit
    {
        public RepeaterHour()
            : base(UnitName.Hour)
        {

        }

        public static readonly int HOUR_SECONDS = 3600; // (60 * 60);

        private DateTime? _start;

        public override string ToString()
        {
            return base.ToString() + "-hour";
        }

        public override int GetWidth()
        {
            return RepeaterHour.HOUR_SECONDS;
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            var now = Now.Value;
            if (_start == null)
            {
                if (pointer == Pointer.Type.Future)
                {
                    _start = Time.New(now, now.Hour).AddHours(1);
                }
                else if (pointer == Pointer.Type.Past)
                {
                    _start = Time.New(now, now.Hour).AddHours(-1);
                }
                else
                {
                    throw new ArgumentException("Unable to handle pointer " + pointer + ".");
                }
            }
            else
            {
                var direction = (pointer == Pointer.Type.Future) ? 1 : -1;
                _start.Value.AddHours(direction * 1);
            }
            return new Span(_start.Value, _start.Value.AddHours(1));
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            var now = Now.Value;
            DateTime hourStart;
            DateTime hourEnd;
            if (pointer == Pointer.Type.Future)
            {

                hourStart = Time.New(now, now.Hour, now.Minute).AddMinutes(1);
                hourEnd = Time.New(now, now.Hour).AddHours(1);
            }
            else if (pointer == Pointer.Type.Past)
            {
                hourStart = Time.New(now, now.Hour);
                hourEnd = Time.New(now, now.Hour, now.Minute);
            }
            else if (pointer == Pointer.Type.None)
            {
                hourStart = Time.New(now, now.Hour);
                hourEnd = hourStart.AddHours(1);
            }
            else
            {
                throw new ArgumentException("Unable to handle pointer " + pointer + ".");
            }
            return new Span(hourStart, hourEnd);
        }

        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            var direction = (pointer == Pointer.Type.Future) ? 1 : -1;
            return span.Add(direction * amount * RepeaterHour.HOUR_SECONDS);
        }
    }
}