using System;
using System.Globalization;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterDayName : Repeater<DayOfWeek>
    {
        public static readonly int DAY_SECONDS = 86400; // (24 * 60 * 60);
        DateTime? _start;

        public RepeaterDayName(DayOfWeek value)
            : base(value)
        {
        }



        public override string ToString()
        {
            return base.ToString() + "-dayname-" + Value;
        }

        public override int GetWidth()
        {
            return RepeaterDayName.DAY_SECONDS;
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            var now = Now.Value;
            var direction = (pointer == Pointer.Type.Future) ? 1 : -1;
            if (_start == null)
            {                
                _start = now.Date.AddDays(direction);
                var dayNum = (int)Value;

                while ((int)(_start.Value.DayOfWeek) != dayNum)
                {
                    _start = _start.Value.AddDays(direction);
                }
            }
            else
            {
                _start = _start.Value.AddDays(direction * 7);
            }
            return new Span(_start.Value, _start.Value.AddDays(1));
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            if (pointer == Pointer.Type.None)
            {
                pointer = Pointer.Type.Future;
            }
            return base.GetNextSpan(pointer);
        }

        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            throw new System.NotImplementedException();
        }
    }
}