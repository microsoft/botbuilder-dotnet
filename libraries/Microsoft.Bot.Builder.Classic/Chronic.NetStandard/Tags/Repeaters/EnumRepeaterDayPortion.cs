using System;

namespace Chronic.Tags.Repeaters
{
    public class EnumRepeaterDayPortion : RepeaterDayPortion<DayPortion>
    {
        static Range AM_RANGE = new Range(0, 12 * 60 * 60); // 12am-12pm
        static Range PM_RANGE = new Range(12 * 60 * 60, 24 * 60 * 60 - 1); // 12pm-12am
        static Range MORNING_RANGE = new Range(6 * 60 * 60, 12 * 60 * 60); // 6am-12pm
        static Range AFTERNOON_RANGE = new Range(13 * 60 * 60, 17 * 60 * 60); // 1pm-5pm
        static Range EVENING_RANGE = new Range(17 * 60 * 60, 20 * 60 * 60); // 5pm-8pm
        static Range NIGHT_RANGE = new Range(20 * 60 * 60, 24 * 60 * 60); // 8pm-12pm
        Range _range;

        public EnumRepeaterDayPortion(DayPortion value)
            : base(value)
        {
            if (value == DayPortion.AM)
            {
                _range = EnumRepeaterDayPortion.AM_RANGE;
            }
            else if (value == DayPortion.PM)
            {
                _range = EnumRepeaterDayPortion.PM_RANGE;
            }
            else if (value == DayPortion.MORNING)
            {
                _range = EnumRepeaterDayPortion.MORNING_RANGE;
            }
            else if (value == DayPortion.AFTERNOON)
            {
                _range = EnumRepeaterDayPortion.AFTERNOON_RANGE;
            }
            else if (value == DayPortion.EVENING)
            {
                _range = EnumRepeaterDayPortion.EVENING_RANGE;
            }
            else if (value == DayPortion.NIGHT)
            {
                _range = EnumRepeaterDayPortion.NIGHT_RANGE;
            }
            else
            {
                throw new ArgumentException("Unknown day portion value " + value,
                                            "value");
            }
        }

        protected override Range GetRange(DayPortion value)
        {

            return _range;
        }

        protected override int GetWidth(Range range)
        {
            return (int)range.Width;
        }
    }
}