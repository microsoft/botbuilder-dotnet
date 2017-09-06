using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Note: This class is only needed for DI. It'll be removed in the next pass as part of cleanup. 
    /// </summary>
    public class ActivityResolver
    {
        private IActivity _activity;

        public void Register(IActivity activity)
        {
            _activity = activity ?? throw new ArgumentNullException("activity");
        }

        public IActivity Resolve()
        {
            return this._activity;
        }
    }
}
