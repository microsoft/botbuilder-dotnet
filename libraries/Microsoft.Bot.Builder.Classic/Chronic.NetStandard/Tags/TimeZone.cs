namespace Chronic.Tags
{
    public class TimeZone : Tag<string>
    {
        public TimeZone(string value) : base(value)
        {
        }

        public override string ToString()
        {
            return "timezone";
        }
    }
}
