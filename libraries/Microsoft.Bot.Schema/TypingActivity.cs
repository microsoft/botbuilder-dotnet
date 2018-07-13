// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The From address is typing
    /// </summary>
    public class TypingActivity : Activity
    {
        public static readonly TypingActivity Default = new TypingActivity();

        public TypingActivity() : base(ActivityTypes.Typing)
        {
        }
    }
}
