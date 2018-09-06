namespace Chronic.Tags.Repeaters
{
    public class IntegerRepeaterDayPortion : RepeaterDayPortion<int>
    {
        readonly Range _range;
        const int SecondsInHour = 60*60;

        public IntegerRepeaterDayPortion(int value)
            : base(value)
        {
            _range = new Range(value * SecondsInHour, (value + 12) * SecondsInHour);
        }

        protected override Range GetRange(int value)
        {            
            return _range;
        }

        protected override int GetWidth(Range range)
        {
            return 12 * SecondsInHour;
        }
    }
}