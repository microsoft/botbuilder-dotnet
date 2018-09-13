namespace Chronic
{
    public class Range
    {
        public long StartSecond { get; private set; }
        public long EndSecond { get; private set; }
        
        public long Width
        {
            get { return EndSecond - StartSecond; }
        }

        public Range(long start, long end)
        {
            StartSecond = start;
            EndSecond = end;
        }

        public bool Contains(long point) 
        {
            return StartSecond <= point && EndSecond >= point;
        }
    }
}