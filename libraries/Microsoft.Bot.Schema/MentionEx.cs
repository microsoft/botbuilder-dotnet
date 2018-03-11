using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Schema
{
    public partial class Mention
    {
        partial void CustomInit()
        {
            this.Type = "mention";
        }
    }
}
