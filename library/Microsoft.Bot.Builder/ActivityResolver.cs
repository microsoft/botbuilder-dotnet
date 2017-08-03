using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public class ActivityResolver
    {
        private IActivity activity;

        public void Register(IActivity activity)
        {
            SetField.NotNull(out this.activity, nameof(activity), activity);
        }

        public IActivity Resolve()
        {
            return this.activity;
        }
    }
}
