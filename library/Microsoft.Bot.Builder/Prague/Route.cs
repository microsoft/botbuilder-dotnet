using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{
    public class Route
    {        
        public Route(Func<Task> function)
        {
            this.Action = function ?? throw new ArgumentNullException(nameof(function)); 
        }

        public Route(Func<Task> function, double score)
        {
            this.Action = function ?? throw new ArgumentNullException(nameof(function));
            if (score < 0 || score > 1.0)
                throw new ArgumentOutOfRangeException(nameof(score)); 

            this.Score = score;
        }

        public double Score { get; set; } = 1.0;
        public bool Thrown { get; set; } = false;

        public Func<Task> Action { get; private set; }
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
