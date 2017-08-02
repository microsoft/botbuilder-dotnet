using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    public interface IBotLogger
    {
    }

    public class NullLogger : IBotLogger
    {
    }
}
