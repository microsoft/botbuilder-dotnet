// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    public class ModelResult<T>
    {
        public string Text { get; set; }

        public int Start { get; set; }

        public int End { get; set; }

        public string TypeName { get; set; }

        public T Resolution { get; set; }
    }
}
