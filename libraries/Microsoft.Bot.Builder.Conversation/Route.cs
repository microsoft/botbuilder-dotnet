using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{
    /// <summary>
    /// Represents a Route returned from a Router matching on conditions
    /// </summary>
    public class Route
    {        
        public Route(Func<IBotContext, MatcherResult, Task>  function)
        {
            this.Action = function ?? throw new ArgumentNullException(nameof(function)); 
        }

        public Route(Func<IBotContext, MatcherResult, Task> function, double score, IList<string> routePath = null)
        {
            this.Action = function ?? throw new ArgumentNullException(nameof(function));
            if (score < 0 || score > 1.0)
                throw new ArgumentOutOfRangeException(nameof(score)); 

            this.Score = score;

            if (routePath != null)
                RoutePath = routePath;
        }

        /// <summary>
        /// (OPTIONAL) Score for this route
        /// </summary>
        public double Score { get; set; } = 1.0;

        /// <summary>
        /// Debug route path
        /// </summary>
        public IList<string> RoutePath { get; private set; } = new List<String>();

        /// <summary>
        /// Async Action to perform for this route
        /// </summary>
        public Func<IBotContext, MatcherResult, Task> Action { get; set; }

        /// <summary>
        /// Description of the reason for their being no action
        /// </summary>
        public string Reason { get; set; }
    }    
}
