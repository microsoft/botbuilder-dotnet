using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class  AnswerSpanRequest
    {
        public boolean enable { get; set; }

        public int topAnswersWithSpan { get; set; }
    }
}
