using System;
using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterSecond : RepeaterUnit
    {
        const int SECOND_SECONDS = 1; // (60 * 60);

        DateTime? _start;

        public RepeaterSecond()
            : base(UnitName.Second)
        {
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            var now = Now.Value;
            int direction = (pointer == Pointer.Type.Future) ? 1 : -1;
            if (_start == null)
            {
                _start = now.AddSeconds(direction * 1);
            }
            else
            {
                _start = _start.Value.AddSeconds(direction * 1);
            }

            return new Span(_start.Value, _start.Value.AddSeconds(1));
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            var now = Now.Value;
            return new Span(now, now.AddSeconds(1));
        }

        public override Span GetOffset(Span span, int amount,
                                       Pointer.Type pointer)
        {
            int direction = (pointer == Pointer.Type.Future) ? 1 : -1;
            // WARN: Does not use Calendar
            return span.Add(direction * amount * RepeaterSecond.SECOND_SECONDS);
        }

        public override int GetWidth()
        {
            return RepeaterSecond.SECOND_SECONDS;
        }

        public override string ToString()
        {
            return base.ToString() + "-second";
        }
    }
}