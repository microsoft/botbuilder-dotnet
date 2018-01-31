// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Adapters
{
    public abstract class ActivityAdapterBase
    {
        public delegate Task OnReceiveDelegate(IActivity activity);

        public ActivityAdapterBase() { }

        public OnReceiveDelegate OnReceive { get; set; }               

        public abstract Task Post(IList<IActivity> activities);
    }
}
