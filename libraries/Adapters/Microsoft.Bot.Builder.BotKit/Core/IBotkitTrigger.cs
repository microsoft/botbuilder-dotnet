// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.BotKit.Core
{
    public interface IBotkitTrigger
    {
        string Type { get; set; }

        // TO-DO: pattern: string | RegExp | { (message: BotkitMessage): Promise<boolean> };

        // string Pattern { get; set; }
        // Regex RegEx { get; set; }
        // Tuple<IBotkitMessage, Task<bool>> Message { get; set; }
        IBotkitHandler Handler { get; set; }
    }
}
