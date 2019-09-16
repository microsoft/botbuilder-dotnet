using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModel;

namespace Microsoft.Bot.Builder.GrammarChecker.Tests
{
    public class SyntaxnetWord
    {
        public SyntaxnetWord()
        {
            Word = string.Empty;
            WordHead = -1;
            WordIndex = -1;
        }

        public int WordIndex { get; set; }

        public int WordHead { get; set; }

        public string Word { get; set; }

        public BasicPosTag WordPOS { get; set; }

        public NounPosTag NounPosTag { get; set; }

        public DependencyTag WordDependency { get; set; }
    }

    public class MockDependencyParser : IDependencyParser
    {
        public List<DependencyFeature> DependencyParsing(List<PosFeature> posFeatures)
        {
            var depFeatures = new List<DependencyFeature>();
            var strDepFeatures = MockData.SyntaxDict[string.Join(' ', posFeatures.Select(o => o.WordText).ToList())];
            var syntaxnetWords = Analysis(strDepFeatures);
            for (int idx = 0; idx < syntaxnetWords.Count; idx++)
            {
                var feature = new DependencyFeature();
                feature.WordIndex = idx;
                if (feature.WordIndex >= 0 && feature.WordIndex < syntaxnetWords.Count)
                {
                    // for verb, will find subject word index
                    // for noun, will find number modifier index
                    if (syntaxnetWords[feature.WordIndex].WordPOS.Equals(BasicPosTag.VERB))
                    {
                        feature.SubjectIndex = FindVerbSubjectIndex(syntaxnetWords, feature.WordIndex);
                    }
                    else if (syntaxnetWords[feature.WordIndex].WordPOS.Equals(BasicPosTag.NOUN))
                    {
                        feature.NumericModifierIndex = FindNumberModifier(syntaxnetWords, feature.WordIndex);
                    }

                    feature.PosFeature.BasicPosTag = syntaxnetWords[feature.WordIndex].WordPOS;
                    feature.PosFeature.NounPosTag = syntaxnetWords[feature.WordIndex].NounPosTag;

                    depFeatures.Add(feature);
                }
                else
                {
                    throw new Exception("Word index is out of range of syntaxnet words array");
                }
            }

            return depFeatures;
        }

        private List<SyntaxnetWord> Analysis(string strInput)
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
                BasicPosTag posTag;
                if (Enum.TryParse(posTagStr, true, out posTag))
                {
                    synWord.WordPOS = posTag;
                }

                var subPosTagStr = features[4];
                NounPosTag nounTag;
                if (Enum.TryParse(subPosTagStr, true, out nounTag))
                {
                    synWord.NounPosTag = nounTag;
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

        private int FindVerbSubjectIndex(
            List<SyntaxnetWord> syntaxWords,
            int wordIndex)
        {
            int subjectIndex = -1, backupSubjectIndex = -1;
            var wordDepTag = syntaxWords[wordIndex].WordDependency;
            if ((subjectIndex = FindSubject(syntaxWords, wordIndex, out backupSubjectIndex)) == -1)
            {
                int maxLoop = syntaxWords.Count;
                while (maxLoop > 0 && (wordDepTag.Equals(DependencyTag.conj)
                       || wordDepTag.Equals(DependencyTag.cop)
                       || wordDepTag.Equals(DependencyTag.aux)
                       || wordDepTag.Equals(DependencyTag.auxpass)))
                {
                    wordIndex = syntaxWords[wordIndex].WordHead;
                    if (wordIndex == -1)
                    {
                        break;
                    }
                    else
                    {
                        wordDepTag = syntaxWords[wordIndex].WordDependency;
                    }

                    maxLoop--;
                }

                // here means the dead cycle happened
                if (maxLoop == 0)
                {
                    wordIndex = -1;
                }

                if (wordIndex >= 0)
                {
                    int backupSubjectTmp = -1;
                    subjectIndex = FindSubject(syntaxWords, wordIndex, out backupSubjectTmp);
                    if (backupSubjectIndex == -1)
                    {
                        backupSubjectIndex = backupSubjectTmp;
                    }
                }
            }

            if (subjectIndex == -1 && backupSubjectIndex != -1)
            {
                return backupSubjectIndex;
            }

            return subjectIndex;
        }

        private int FindSubject(
            List<SyntaxnetWord> syntaxWords,
            int wordIndex,
            out int backupSubjectIndex)
        {
            int subjectIndex = -1;
            backupSubjectIndex = -1;
            if (wordIndex < 0 || wordIndex >= syntaxWords.Count)
            {
                return subjectIndex;
            }

            bool findSubject = false;
            if (wordIndex < syntaxWords.Count && syntaxWords[wordIndex].WordDependency.Equals(DependencyTag.rcmod))
            {
                subjectIndex = syntaxWords[wordIndex].WordHead;

                // point to a preposition phrase
                if (subjectIndex < syntaxWords.Count && syntaxWords[subjectIndex].WordDependency.Equals(DependencyTag.pobj))
                {
                    if (!IsValidNounSubject(syntaxWords, subjectIndex))
                    {
                        // if the preposition object is not a valid subject, then skip the preposition
                        subjectIndex = FindPrepositionModifier(syntaxWords, subjectIndex);
                        if (IsValidNounSubject(syntaxWords, subjectIndex))
                        {
                            findSubject = true;
                        }
                    }
                    else
                    {
                        findSubject = true;
                    }
                }
                else
                {
                    if (IsValidNounSubject(syntaxWords, subjectIndex))
                    {
                        findSubject = true;
                    }
                }
            }

            if (!findSubject)
            {
                foreach (var synWord in syntaxWords)
                {
                    if (synWord.WordDependency.Equals(DependencyTag.nsubj)
                        && wordIndex.Equals(synWord.WordHead))
                    {
                        subjectIndex = synWord.WordIndex;
                        if (IsValidNounSubject(syntaxWords, subjectIndex))
                        {
                            findSubject = true;
                            break;
                        }
                    }
                }
            }

            if (findSubject)
            {
                return subjectIndex;
            }

            backupSubjectIndex = subjectIndex;
            return -1;
        }

        private int FindPrepositionModifier(List<SyntaxnetWord> syntaxWords, int wordIndex)
        {
            int prepositionModifierIndex = -1;
            if (wordIndex < 0 || wordIndex >= syntaxWords.Count)
            {
                return prepositionModifierIndex;
            }

            int prepositionSubject = syntaxWords[wordIndex].WordHead;
            if (syntaxWords[prepositionSubject].WordDependency.Equals(DependencyTag.prep))
            {
                prepositionModifierIndex = syntaxWords[prepositionSubject].WordHead;
            }

            return prepositionModifierIndex;
        }

        private bool IsValidNounSubject(List<SyntaxnetWord> syntaxWords, int wordIndex)
        {
            if (wordIndex == -1)
            {
                return false;
            }

            bool isValid = false;

            if (syntaxWords[wordIndex].WordPOS.Equals(BasicPosTag.NUM))
            {
                isValid = true;
            }

            if (syntaxWords[wordIndex].WordPOS.Equals(BasicPosTag.NOUN)
                || syntaxWords[wordIndex].WordPOS.Equals(BasicPosTag.PRON))
            {
                isValid = true;
            }

            return isValid;
        }

        private int FindNumberModifier(List<SyntaxnetWord> syntaxWords, int wordIndex)
        {
            int numModifierIndex = -1;
            if (wordIndex < 0 || wordIndex >= syntaxWords.Count)
            {
                return numModifierIndex;
            }

            for (var numberIndex = 0; numberIndex < syntaxWords.Count; numberIndex++)
            {
                if ((syntaxWords[numberIndex].WordDependency.Equals(DependencyTag.num)
                    || syntaxWords[numberIndex].WordDependency.Equals(DependencyTag.amod))
                    && syntaxWords[numberIndex].WordHead.Equals(wordIndex))
                {
                    numModifierIndex = numberIndex;
                    break;
                }
            }

            return numModifierIndex;
        }
    }
}
