using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public class GrammarChecker : IGrammarChecker
    {
        private IPosTagger posTagger;
        private IDependencyParser dependencyParser;
        private ICorrector corrector;

        public GrammarChecker(IPosTagger posTagger, IDependencyParser dependencyParser, ICorrector corrector)
        {
            this.posTagger = posTagger;
            this.dependencyParser = dependencyParser;
            this.corrector = corrector;
        }
        
        public string CheckText(string text)
        {
            List<char> separators;
            var sentences = SplitTextToSentences(text, out separators);
            var checkedSentences = new List<string>();
            sentences.ForEach(sentence => checkedSentences.Add(CheckSentence(sentence)));

            return MergeSentencesToText(checkedSentences, separators);
        }

        private List<string> SplitTextToSentences(string text, out List<char> separators)
        {
            separators = new List<char>();
            var sentenceSeparators = new char[] { '!', '.', '?', ',' };
            for (var idx = 0; idx < text.Length; idx++)
            {
                if (sentenceSeparators.Contains(text[idx]))
                {
                    separators.Add(text[idx]);
                }
            }

            var sentences = text.Split(sentenceSeparators);

            return sentences.ToList();
        }

        private string CheckSentence(string sentence)
        {
            if (sentence.Trim().Length == 0)
            {
                return sentence;
            }

            List<string> wsSeparators;
            var words = SplitSentenceToWords(sentence, out wsSeparators);
            var tagResults = PosTagging(sentence.Trim());
            var dependencyResults = DependencyParsing(tagResults);
            var outputWords = CorrectWords(words, dependencyResults);
            var strResult = MergeWordsToSentence(outputWords, wsSeparators);

            return strResult;
        }

        private string MergeSentencesToText(List<string> sentences, List<char> separators)
        {
            var text = string.Empty;
            for (int idx = 0; idx < sentences.Count - 1; idx++)
            {
                text += sentences[idx] + separators[idx];
            }

            text += sentences[sentences.Count - 1];

            return text;
        }

        private List<PosFeature> PosTagging(string sentence)
        {
            var posFeatures = posTagger.PosTagging(sentence);

            return posFeatures;
        }

        private List<DependencyFeature> DependencyParsing(List<PosFeature> posFeatures)
        {
            var depFeatures = dependencyParser.DependencyParsing(posFeatures);

            return depFeatures;
        }

        private List<string> CorrectWords(List<string> words, List<DependencyFeature> depFeatures)
        {
            foreach (var depFeature in depFeatures)
            {
                var correctingInfo = new CorrectingInfo();
                if (depFeature.WordIndex >= 0 && depFeature.WordIndex < words.Count)
                {
                    if (IsElision(words[depFeature.WordIndex]))
                    {
                        correctingInfo.WordIndex = depFeature.WordIndex;

                        words[correctingInfo.WordIndex] = this.corrector.CorrectElisionWord(
                            words[correctingInfo.WordIndex], 
                            words[correctingInfo.WordIndex + 1], 
                            correctingInfo);
                    }
                    else if (depFeature.PosFeature.BasicPosTag.Equals(BasicPosTag.VERB) && depFeature.SubjectIndex != -1)
                    {
                        correctingInfo.WordIndex = depFeature.WordIndex;
                        correctingInfo.VerbInfo.ReferencePosition = depFeature.SubjectIndex;

                        var number = Number.None;
                        if (this.corrector.IsNumber(words[depFeature.SubjectIndex], out number))
                        {
                            correctingInfo.VerbInfo.SubjectNumber = number;
                        }

                        if (depFeatures[depFeature.SubjectIndex].PosFeature.NounPosTag == NounPosTag.NN
                            || depFeatures[depFeature.SubjectIndex].PosFeature.NounPosTag == NounPosTag.NNP)
                        {
                            correctingInfo.VerbInfo.SubjectNumberFromPosTagging = Number.Singular;
                        }

                        if (depFeatures[depFeature.SubjectIndex].PosFeature.NounPosTag == NounPosTag.NNS
                            || depFeatures[depFeature.SubjectIndex].PosFeature.NounPosTag == NounPosTag.NNPS)
                        {
                            correctingInfo.VerbInfo.SubjectNumberFromPosTagging = Number.Plural;
                        }

                        words[correctingInfo.WordIndex] = this.corrector.CorrectVerbWord(words[correctingInfo.WordIndex], words, correctingInfo);
                    }
                    else if (depFeature.PosFeature.BasicPosTag.Equals(BasicPosTag.NOUN) && depFeature.NumericModifierIndex != -1)
                    {
                        var number = Number.None;
                        if (this.corrector.IsNumber(words[depFeature.NumericModifierIndex], out number))
                        {
                            correctingInfo.WordIndex = depFeature.WordIndex;
                            correctingInfo.NumberInfo.Feature = number;

                            words[correctingInfo.WordIndex] = this.corrector.CorrectNounWord(words[correctingInfo.WordIndex], correctingInfo);
                        }
                    }
                }
            }

            return words;
        }

        private List<string> SplitSentenceToWords(string sentence, out List<string> wsSeparators)
        {
            wsSeparators = new List<string>();
            var wsStartIndex = 0;
            var inWS = true;
            for (var idx = 0; idx < sentence.Length; idx++)
            {
                if (sentence[idx] == ' ')
                {
                    if (!inWS)
                    {
                        wsStartIndex = idx;
                        inWS = true;
                    }
                }
                else if (inWS)
                {
                    var wsStr = sentence.Substring(wsStartIndex, idx - wsStartIndex);
                    inWS = false;
                    wsSeparators.Add(wsStr);
                }
            }

            if (inWS)
            {
                var wsStr = sentence.Substring(wsStartIndex, sentence.Length - wsStartIndex);
                wsSeparators.Add(wsStr);
            }
            else
            {
                wsSeparators.Add(string.Empty);
            }

            var words = sentence.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return words.ToList();
        }

        private string MergeWordsToSentence(List<string> words, List<string> separators)
        {
            var sentence = separators[0];
            for (var idx = 0; idx < words.Count; idx++)
            {
                sentence += words[idx] + separators[idx + 1];
            }

            return sentence;
        }

        private bool IsElision(string strWord)
        {
            if (strWord.Trim().Equals(Elision.a.ToString(), StringComparison.InvariantCultureIgnoreCase)
                || strWord.Trim().Equals(Elision.an.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
