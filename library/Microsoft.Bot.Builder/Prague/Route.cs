using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public class Route
    {        
        public Route(Func<Task> action)
        {
            this.Action = action ?? throw new ArgumentNullException("action");
        }

        public Route(Func<Task> action, double score)
        {
            this.Action = action ?? throw new ArgumentNullException("action");
            if (score < 0 || score > 1.0)
                throw new ArgumentOutOfRangeException("score");

            this.Score = score;
        }

        public double Score { get; set; } = 1.0;
        public bool Thrown { get; set; } = false;

        public Func<Task> Action { get; internal set; }
    }

    public sealed class MinRoute : Route
    {
        public MinRoute() : base(
            () => throw new InvalidOperationException("This shold never be called."))
        {
            this.Score = 0;
        }
    }   
}
