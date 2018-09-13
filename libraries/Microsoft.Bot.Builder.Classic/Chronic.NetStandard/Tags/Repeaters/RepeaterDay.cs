using System;
using Chronic;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterDay : RepeaterUnitName
    {
        public static readonly int DAY_SECONDS = 24 * 60 * 60;

        public RepeaterDay()
            : base(UnitName.Day)
        {

        }

        private DateTime? _currentDayStart;

        protected override Span NextSpan(Pointer.Type pointer)
        {
            if (_currentDayStart == null)
            {
                _currentDayStart = Now.Value.Date;
            }

            var direction = (int)pointer;
            _currentDayStart = _currentDayStart.Value.AddDays(direction);

            return new Span(_currentDayStart.Value, _currentDayStart.Value.AddDays(1));
        }


        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            DateTime dayBegin;
            DateTime dayEnd;
            if (pointer == Pointer.Type.Future)
            {
                dayBegin = Time.New(Now.Value.Date, Now.Value.Hour);
                dayEnd = Now.Value.Date.AddDays(1);
            }
            else if (pointer == Pointer.Type.Past)
            {
                dayBegin = Now.Value.Date;
                dayEnd = Time.New(Now.Value.Date, Now.Value.Hour);
            }
            else if (pointer == Pointer.Type.None)
            {
                dayBegin = Now.Value.Date;
                dayEnd = Now.Value.Date.AddDays(1);
            }
            else
            {
                throw new ArgumentException("Unable to handle pointer " + pointer + ".", "pointer");
            }
            return new Span(dayBegin, dayEnd);
        }


        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            var direction = (int)pointer;
            return span.Add(direction * amount * RepeaterDay.DAY_SECONDS);
        }

        public override int GetWidth()
        {
            return RepeaterDay.DAY_SECONDS;
        }


        public override string ToString()
        {
            return base.ToString() + "-day";
        }
    }
}