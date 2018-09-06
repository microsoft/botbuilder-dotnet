namespace Chronic
{
    public class OrdinalDay : Ordinal
    {
        public OrdinalDay(int value) : base(value)
        {

        }

        public override string ToString()
        {
            return base.ToString() + "-day-" + Value;
        }

    }

    public class Ordinal : Tag<int>
    {
        public Ordinal(int value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return "ordinal";
        }
    }

    
}