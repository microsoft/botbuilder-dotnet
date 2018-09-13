using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chronic
{
    public class Tick
    {
        public int Time { get; set; }
        public bool IsAmbiguous { get; set; }

        public Tick(int time, bool isAmbiguous)
        {
            Time = time;
            IsAmbiguous = isAmbiguous;
        }

        public Tick Times(int multiplier)
        {
            return new Tick(Time * multiplier, IsAmbiguous);
        }

        public int ToInt32()
        {
            return Time;
        }

        public float ToFloat()
        {
            return (float)Time;
        }

        public override string ToString()
        {
            return Time + (IsAmbiguous ? "?" : "");
        }
    }

}
