using System.Collections.Generic;

namespace Chronic
{
    public class Scalar : Tag<int>
    {
        public Scalar(int value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return "scalar";
        }
    }

    public class ScalarDay : Scalar
    {
        public ScalarDay(int value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return base.ToString() + "-day-" + Value;
        }
    }

    public class ScalarMonth : Scalar
    {
        public ScalarMonth(int value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return base.ToString() + "-month-" + Value;
        }
    }

    public class ScalarYear : Scalar
    {
        public ScalarYear(int value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return base.ToString() + "-year-" + Value;
        }
    }
}