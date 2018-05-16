using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    internal class PostProcessingUtilities
    {
        /// <summary>
        /// Helper to Join words to sentence
        /// </summary>
        /// <param name="delimiter">String delimiter used  to join words.</param> 
        /// <param name="words">String Array of words to be joined.</param> 
        /// <returns>string joined sentence</returns>
        public static string Join(string delimiter, string[] words)
        {
            string sentence = string.Join(delimiter, words);
            sentence = Regex.Replace(sentence, "[ ]?'[ ]?", "'");
            return sentence.Trim();
        }

        /// <summary>
        /// Helper to split sentence to words 
        /// </summary>
        /// <param name="sentence">String containing sentence to be splitted.</param> 
        /// <returns>string array of words.</returns>
        public static string[] SplitSentence(string sentence, string[] alignments = null, bool isSrcSentence = true)
        {
            string[] wrds = sentence.Split(' ');
            string[] alignSplitWrds = new string[0];
            if (alignments != null && alignments.Length > 0)
            {
                List<string> outWrds = new List<string>();
                int wrdIndxInAlignment = 1;

                if (isSrcSentence)
                    wrdIndxInAlignment = 0;
                else
                {
                    // reorder alignments in case of target translated  message to get ordered output words.
                    Array.Sort(alignments, (x, y) => Int32.Parse(x.Split('-')[wrdIndxInAlignment].Split(':')[0]).CompareTo(Int32.Parse(y.Split('-')[wrdIndxInAlignment].Split(':')[0])));
                }
                string withoutSpaceSentence = sentence.Replace(" ", "");

                foreach (string alignData in alignments)
                {
                    alignSplitWrds = outWrds.ToArray();
                    string wordIndexes = alignData.Split('-')[wrdIndxInAlignment];
                    int startIndex = Int32.Parse(wordIndexes.Split(':')[0]);
                    int length = Int32.Parse(wordIndexes.Split(':')[1]) - startIndex + 1;
                    string wrd = sentence.Substring(startIndex, length);
                    string[] newWrds = new string[outWrds.Count + 1];
                    if (newWrds.Length > 1)
                        alignSplitWrds.CopyTo(newWrds, 0);
                    newWrds[outWrds.Count] = wrd;
                    string subSentence = Join("", newWrds.ToArray());
                    if (withoutSpaceSentence.Contains(subSentence))
                        outWrds.Add(wrd);
                }
                alignSplitWrds = outWrds.ToArray();
            }
            char[] punctuationChars = new char[] { '.', ',', '?', '!' };
            if (Join("", alignSplitWrds).TrimEnd(punctuationChars) == Join("", wrds).TrimEnd(punctuationChars))
                return alignSplitWrds;
            return wrds;
        }

        /// <summary>
        ///parsing alignment information onto a dictionary
        /// dictionary key is word index in source
        /// value is word index in translated text
        /// </summary>
        /// <param name="alignment">String containing phrase alignments</param>
        /// <param name="sourceMessage">String containing source message</param>
        /// /<param name="trgMessage">String containing translated message</param>
        /// <returns></returns>
        public static Dictionary<int, int> WordAlignmentParse(string[] alignments, string[] srcWords, string[] trgWords)
        {
            Dictionary<int, int> alignMap = new Dictionary<int, int>();
            string sourceMessage = Join(" ", srcWords);
            string trgMessage = Join(" ", trgWords);
            foreach (string alignData in alignments)
            {
                string[] wordIndexes = alignData.Split('-');
                int srcStartIndex = Int32.Parse(wordIndexes[0].Split(':')[0]);
                int srcLength = Int32.Parse(wordIndexes[0].Split(':')[1]) - srcStartIndex + 1;
                if ((srcLength + srcStartIndex) > sourceMessage.Length)
                    continue;
                string srcWrd = sourceMessage.Substring(srcStartIndex, srcLength);
                int sourceWordIndex = Array.FindIndex(srcWords, row => row == srcWrd);

                int trgstartIndex = Int32.Parse(wordIndexes[1].Split(':')[0]);
                int trgLength = Int32.Parse(wordIndexes[1].Split(':')[1]) - trgstartIndex + 1;
                if ((trgLength + trgstartIndex) > trgMessage.Length)
                    continue;
                string trgWrd = trgMessage.Substring(trgstartIndex, trgLength);
                int targetWordIndex = Array.FindIndex(trgWords, row => row == trgWrd);

                if (sourceWordIndex >= 0 && targetWordIndex >= 0)
                    alignMap[sourceWordIndex] = targetWordIndex;
            }
            return alignMap;
        }

        /// <summary>
        /// use alignment information source sentence and target sentence
        /// to keep a specific word from the source onto target translation
        /// </summary>
        /// <param name="alignment">Dictionary containing the alignments</param>
        /// <param name="source">Source Language</param>
        /// <param name="target">Target Language</param>
        /// <param name="srcWrd">Source Word</param>
        /// <returns></returns>
        public static string[] KeepSrcWrdInTranslation(Dictionary<int, int> alignment, string[] sourceWords, string[] targetWords, int srcWrdIndx)
        {
            if (alignment.ContainsKey(srcWrdIndx))
            {
                targetWords[alignment[srcWrdIndx]] = sourceWords[srcWrdIndx];
            }
            return targetWords;
        }

    }
}
