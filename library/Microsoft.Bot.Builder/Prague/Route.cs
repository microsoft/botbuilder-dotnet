using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public class Route
    {
        public delegate void RouteAction();

        public Route(RouteAction action)
        {
            this.Action = action ?? throw new ArgumentNullException("action");
        }

        public Route(RouteAction action, double score)
        {
            this.Action = action ?? throw new ArgumentNullException("action");
            if (score < 0 || score > 1.0)
                throw new ArgumentOutOfRangeException("score");

            this.Score = score;
        }

        public double Score { get; set; } = 1.0;
        public bool Thrown { get; set; } = false;

        public RouteAction Action { get; internal set; }
    }

    public sealed class MinRoute : Route
    {
        public MinRoute() : base(
            () => throw new InvalidOperationException("This shold never be called."))
        {
            this.Score = 0;
        }
    }


    //public class Match
    //{
    //    public double Score { get; set; } = 1.0;

    //}   

   
}
