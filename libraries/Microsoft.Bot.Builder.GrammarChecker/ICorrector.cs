using System.Collections.Generic;
using Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public interface ICorrector
    {
        string CorrectElisionWord(string inputWord, string nextWord, CorrectingInfo correctingInfo);

        string CorrectNounWord(string inputWord, CorrectingInfo correctingInfo);

        string CorrectVerbWord(string inputWord, List<string> inputWords, CorrectingInfo correctingInfo);

        bool IsNumber(string word, out Number number);
    }
}
