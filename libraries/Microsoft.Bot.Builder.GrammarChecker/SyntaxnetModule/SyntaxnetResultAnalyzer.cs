namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModule
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

    public class SyntaxnetResultAnalyzer
    {
        public static List<SyntaxnetWord> Analysis(string strInput)
        {
            var syntaxnetWords = new List<SyntaxnetWord>();
            if (strInput.Length == 0)
            {
                return syntaxnetWords;
            }

            var words = new List<string>(strInput.Split('\n'));
            if (words.Count == 0)
            {
                return syntaxnetWords;
            }

            foreach (string word in words)
            {
                var synWord = new SyntaxnetWord();
                var features = new List<string>(word.Split('\t'));

                if (features.Count != 10)
                {
                    continue;
                }

                int wordIdx = -1;
                if (int.TryParse(features[0], out wordIdx))
                {
                    synWord.WordIndex = wordIdx - 1;
                }
                else
                {
                    throw new Exception("Parse syntaxnet word index error");
                }

                int wordHead = -1;
                if (int.TryParse(features[6], out wordHead))
                {
                    synWord.WordHead = wordHead - 1;
                }
                else
                {
                    throw new Exception("Parse syntaxnet word head error");
                }

                var posTagStr = features[3];
                POSTag posTag;
                if (Enum.TryParse(posTagStr, true, out posTag))
                {
                    synWord.WordPOS = posTag;
                }

                var depTagStr = features[7];
                DependencyTag depTag;
                if (Enum.TryParse(depTagStr, true, out depTag))
                {
                    synWord.WordDependency = depTag;
                }

                synWord.Word = features[1];
                syntaxnetWords.Add(synWord);
            }

            return syntaxnetWords;
        }
    }
}
