namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

    public class SyntaxnetModule : ISyntaxModule
    {
        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "InitializeSyntaxnet", CharSet = CharSet.Unicode)]
        private static extern bool InitializeSyntaxnet(IntPtr pSyntax, [MarshalAs(UnmanagedType.LPStr)] string strModelDirectory);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "CreateSyntaxnet")]
        private static extern IntPtr CreateSyntaxnet();

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "DeleteSyntaxnet")]
        private static extern void DeleteSyntax(IntPtr pSyntax);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "DoPosTagging", CharSet = CharSet.Unicode)]
        private static extern IntPtr DoPosTagging(
            IntPtr pSyntax,
            [MarshalAs(UnmanagedType.LPStr)] string strInputSentence);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "DoDependencyParsing", CharSet = CharSet.Unicode)]
        private static extern IntPtr DoDependencyParsing(
            IntPtr pSyntax,
            [MarshalAs(UnmanagedType.LPStr)] string strInputSentence);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "FreeSyntaxResult")]
        private static extern void FreeSyntaxResult(IntPtr pResult);

        private IntPtr pSyntax;

        public SyntaxnetModule()
        {
            this.pSyntax = IntPtr.Zero;
        }

        ~SyntaxnetModule()
        {
            DeleteSyntaxnet();
        }

        public bool InitSyntaxModule(string path = "")
        {
            this.pSyntax = CreateSyntaxnet();
            string modelDirectory = Path.Combine(path, "SyntaxnetModule/ModelData").Replace(@"\", "/");

            return InitializeSyntaxnet(this.pSyntax, modelDirectory);
        }

        public void Dispose()
        {
            DeleteSyntaxnet();
        }

        public bool PosTagging(string sentence, out List<object> tags)
        {
            tags = new List<object>();
            if (this.pSyntax == IntPtr.Zero)
            {
                return false;
            }
            
            var responsePtr = DoPosTagging(this.pSyntax, sentence);
            if (responsePtr != IntPtr.Zero)
            {
                var posStr = Marshal.PtrToStringAnsi(responsePtr);
                tags = posStr.Split('\n').ToList<object>();
                FreeSyntaxResult(responsePtr);
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool DependencyParsing(List<object> tags, out List<DependencyFeature> depFeatures)
        {
            depFeatures = new List<DependencyFeature>();
            if (this.pSyntax == IntPtr.Zero)
            {
                return false;
            }

            var responsePtr = DoDependencyParsing(this.pSyntax, string.Join("\n", tags.ToArray()));
            if (responsePtr != IntPtr.Zero)
            {
                var strDepFeatures = Marshal.PtrToStringAnsi(responsePtr);
                var syntaxnetWords = SyntaxnetResultAnalyzer.Analysis(strDepFeatures);
                for (int idx = 0; idx < syntaxnetWords.Count; idx++)
                {
                    var feature = new DependencyFeature();
                    feature.WordIndex = idx;
                    if (feature.WordIndex >= 0 && feature.WordIndex < syntaxnetWords.Count)
                    {
                        // for verb, will find subject word index
                        // for noun, will find number modifier index
                        if (syntaxnetWords[feature.WordIndex].WordPOS.Equals(POSTag.VERB))
                        {
                            feature.SubjectIndex = FindVerbSubjectIndex(syntaxnetWords, feature.WordIndex);
                        }
                        else if (syntaxnetWords[feature.WordIndex].WordPOS.Equals(POSTag.NOUN))
                        {
                            feature.NumericModifierIndex = FindNumberModifier(syntaxnetWords, feature.WordIndex);
                        }

                        feature.PosTag = syntaxnetWords[feature.WordIndex].WordPOS;
                        depFeatures.Add(feature);
                    }
                    else
                    {
                        throw new Exception("Word index is out of range of syntaxnet words array");
                    }
                }

                FreeSyntaxResult(responsePtr);
            }
            else
            {
                return false;
            }

            return true;
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

            if (syntaxWords[wordIndex].WordPOS.Equals(POSTag.NUM))
            {
                isValid = true;
            }

            if (syntaxWords[wordIndex].WordPOS.Equals(POSTag.NOUN)
                || syntaxWords[wordIndex].WordPOS.Equals(POSTag.PRON))
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

        private void DeleteSyntaxnet()
        {
            if (this.pSyntax != IntPtr.Zero)
            {
                DeleteSyntax(this.pSyntax);
                this.pSyntax = IntPtr.Zero;
            }
        }
    }
}
