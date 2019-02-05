using System;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    internal interface IRangePromptOptions<T>
        where T : struct, IComparable<T>
    {
        T MinValue { get; set; }

        T MaxValue { get; set; }

        ActivityTemplate TooSmallResponse { get; set; }

        ActivityTemplate TooLargeResponse { get; set; }
    }
}
