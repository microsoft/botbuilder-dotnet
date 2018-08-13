// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptRecognizerResult<T>
    {
        public PromptRecognizerResult()
        {
            Succeeded = false;
        }

        public bool Succeeded { get; set; }

        public T Value { get; set; }
    }
}
