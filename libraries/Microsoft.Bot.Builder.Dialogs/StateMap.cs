using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class StateMap : Dictionary<string, object>
    {
        public StateMap()
            : base()
        {
        }

        public StateMap(StateMap other)
            : base()
        {
            foreach (var item in other)
            {
                this.Add(item.Key, item.Value);
            }
        }
    }
}
