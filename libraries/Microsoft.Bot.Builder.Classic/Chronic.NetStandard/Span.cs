using System;

namespace Chronic
{
    public class Span
    {
        public DateTime? Start;

        public DateTime? End;

        public Span(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public int Width
        {
            get
            {
                return
                    (int)Math.Truncate((End.Value - Start.Value).TotalSeconds);
            }
        }

        public Span Add(long seconds)
        {
            return new Span(Start.Value.AddSeconds(seconds),
                            End.Value.AddSeconds(seconds));
        }

        public Span Subtract(long seconds)
        {
            return Add(-seconds);
        }

        public bool Contains(DateTime? value)
        {
            return Start <= value && value <= End;
        }

        public DateTime ToTime()
        {
            if (Width > 1)
            {
                return Start.Value.AddSeconds((double)Width / 2);
            }
            else
            {
                return Start.Value;
            }

        }

        public override string ToString()
        {
            return String.Format("({0} - {1})", Start, End);
        }
    }


}
