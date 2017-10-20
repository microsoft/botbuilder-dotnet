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

        public Route(Func<Task> function, double score, IList<string> routePath = null)
        {
            this.Action = function ?? throw new ArgumentNullException(nameof(function));
            if (score < 0 || score > 1.0)
                throw new ArgumentOutOfRangeException(nameof(score)); 

            this.Score = score;

            if (routePath != null)
                RoutePath = routePath;
        }

        public double Score { get; set; } = 1.0;

        public IList<string> RoutePath { get; private set; } = new List<String>();

        public Func<Task> Action { get; set; }
    }    
}
