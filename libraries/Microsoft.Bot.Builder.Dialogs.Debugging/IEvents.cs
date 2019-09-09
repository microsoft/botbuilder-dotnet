using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IEvents
    {
        Protocol.ExceptionBreakpointFilter[] Filters
        {
            get;
        }

        bool this[string filter]
        {
            get;
            set;
        }

        void Reset(IEnumerable<string> filters);
    }
}
