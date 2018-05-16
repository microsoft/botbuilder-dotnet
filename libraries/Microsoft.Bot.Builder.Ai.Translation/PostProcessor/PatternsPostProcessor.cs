using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    /// <summary>
    /// PatternsPostProcessor  is used to handle translation errors while translating numbers
    /// and to handle words that needs to be kept same as source language from provided template each line having a regex
    /// having first group matching the words that needs to be kept
    /// </summary>
    internal class PatternsPostProcessor : IPostProcessor
    {
        private readonly HashSet<string> _patterns;
        private string sourceMessage;
        private string targetMessage;
        private string alignment;


        /// <summary>
        /// Constructor that indexes input template for source language
        /// </summary>
        /// <param name="noTranslateTemplatePath">Path of no translate patterns</param> 
        public PatternsPostProcessor(List<string> patterns) : this()
        {
            foreach (string pattern in patterns)
            {
                string processedLine = pattern.Trim();
                if (!pattern.Contains('('))
                {
                    processedLine = '(' + pattern + ')';
                }
                _patterns.Add(processedLine);
            }
        }

        /// <summary>
        /// Constructor for postprocessor that fixes numbers only
        /// </summary>
        private PatternsPostProcessor()
        {
            _patterns = new HashSet<string>();
        }

        public string Alignment { get => alignment; set => alignment = value; }
        public string TargetMessage { get => targetMessage; set => targetMessage = value; }
        public string SourceMessage { get => sourceMessage; set => sourceMessage = value; }

        /// <summary>
        /// Adds a no translate phrase to the pattern list .
        /// </summary>
        /// <param name="noTranslatePhrase">String containing no translate phrase</param>
        public void AddNoTranslatePhrase(string noTranslatePhrase)
        {
            _patterns.Add("(" + noTranslatePhrase + ")");
        }

        /// <summary>
        /// Fixing translation
        /// used to handle numbers and no translate list
        /// </summary>
        /// <param name="sourceMessage">Source Message</param>
        /// <param name="alignment">String containing the Alignments</param>
        /// <param name="targetMessage">Target Message</param>
        /// <returns></returns>
        public void Process(out string processedResult)
        {
            bool containsNum = Regex.IsMatch(_sourceMessage, @"\d");

            if (_patterns.Count == 0 && !containsNum)
                processedResult = _targetMessage;
            if (string.IsNullOrWhiteSpace(_alignment))
                processedResult = _targetMessage;

            var toBeReplaced = from result in _patterns
                               where Regex.IsMatch(_sourceMessage, result, RegexOptions.Singleline | RegexOptions.IgnoreCase)
                               select result;
            string[] alignments = this._alignment.Trim().Split(' ');
            string[] srcWords = PostProcessingUtilities.SplitSentence(_sourceMessage, alignments);
            string[] trgWords = PostProcessingUtilities.SplitSentence(_targetMessage, alignments, false);
            Dictionary<int, int> alignMap = PostProcessingUtilities.WordAlignmentParse(alignments, srcWords, trgWords);
            if (toBeReplaced.Any())
            {
                foreach (string pattern in toBeReplaced)
                {
                    Match matchNoTranslate = Regex.Match(_sourceMessage, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    int noTranslateStartChrIndex = matchNoTranslate.Groups[1].Index;
                    int noTranslateMatchLength = matchNoTranslate.Groups[1].Length;
                    int wrdIndx = 0;
                    int chrIndx = 0;
                    int newChrLengthFromMatch = 0;
                    int srcIndex = -1;
                    int newNoTranslateArrayLength = 1;
                    foreach (string wrd in srcWords)
                    {

                        chrIndx += wrd.Length + 1;
                        wrdIndx++;
                        if (chrIndx == noTranslateStartChrIndex)
                        {
                            srcIndex = wrdIndx;
                        }
                        if (srcIndex != -1)
                        {
                            if (newChrLengthFromMatch + srcWords[wrdIndx].Length >= noTranslateMatchLength)
                                break;
                            newNoTranslateArrayLength += 1;
                            newChrLengthFromMatch += srcWords[wrdIndx].Length + 1;
                        }

                    }
                    if (srcIndex == -1)
                        continue;
                    string[] wrdNoTranslate = new string[newNoTranslateArrayLength];
                    Array.Copy(srcWords, srcIndex, wrdNoTranslate, 0, newNoTranslateArrayLength);
                    foreach (string srcWrd in wrdNoTranslate)
                    {
                        trgWords = PostProcessingUtilities.KeepSrcWrdInTranslation(alignMap, srcWords, trgWords, srcIndex);
                        srcIndex++;
                    }

                }
            }

            MatchCollection numericMatches = Regex.Matches(_sourceMessage, @"\d+", RegexOptions.Singleline);
            foreach (Match numericMatch in numericMatches)
            {
                int srcIndex = Array.FindIndex(srcWords, row => row == numericMatch.Groups[0].Value);
                trgWords = PostProcessingUtilities.KeepSrcWrdInTranslation(alignMap, srcWords, trgWords, srcIndex);
            }
            processedResult = PostProcessingUtilities.Join(" ", trgWords);
        }
    }
}
