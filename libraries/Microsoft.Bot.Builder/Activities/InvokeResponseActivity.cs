using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public class InvokeResponseActivity : ActivityWithValue
    {
        public InvokeResponseActivity()
            : base(ActivityTypesEx.InvokeResponse)
        {
        }
    }
}
