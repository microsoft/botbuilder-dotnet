using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    internal interface IRangePromptOptions<T>
        where T : struct, IComparable<T>
    {
        T MinValue { get; set; }

        T MaxValue { get; set; }

        ITemplate<Activity> TooSmallResponse { get; set; }

        ITemplate<Activity> TooLargeResponse { get; set; }
    }
}
