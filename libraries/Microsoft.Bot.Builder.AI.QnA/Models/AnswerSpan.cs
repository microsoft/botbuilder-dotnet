// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class AnswerSpan
    {
        public string text { get; set; }

        public float score { get; set; }

        public int startIndex { get; set; }

        public int endIndex { get; set; }
    }
}
