// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Ai.Translation.PostProcessor
{
    public class PostProcessingUtilities
    {
        /// <summary>
        /// Helper to join words to sentence.
        /// </summary>
        /// <param name="delimiter">String delimiter used to join words.</param> 
        /// <param name="words">String Array of words to be joined.</param> 
        /// <returns>A <see cref="string"/> joined sentence.</returns>
        public static string Join(string delimiter, string[] words)
        {
            //null checking parameters
            if (delimiter == null)
            {
                throw new ArgumentNullException(nameof(delimiter));
            }

            if (words == null)
            {
                throw new ArgumentNullException(nameof(words));
            }

            //join the tokens using the specified delimiter
            //ex :  words = {"I", "didn", "'t", "see", "you"}
            //      delimiter = " "
            string sentence = string.Join(delimiter, words);
            //sentence = I didn 't see you

            //if found this punctuation pattern, overwrite it with only apostrophe, ie : remove the delimited around the apostrophe
            sentence = Regex.Replace(sentence, "[ ]?'[ ]?", "'");

            //sentence = I didn't see you 
            //the space delimiter is removed from the sentence
            return sentence.Trim();
        }

        /// <summary>
        /// Helper to split a sentence into words.
        /// </summary>
        /// <param name="sentence">String containing sentence to be splitted.</param>
        /// <param name="alignments">Alignment information from translator output.</param>
        /// <param name="isSourceSentence">Flag to indicate if the sentence sent is a source sentence or translated sentence</param>
        /// <returns>An array that contains the split sentence tokens.</returns>

        public static string[] SplitSentence(string sentence, string[] alignments = null, bool isSourceSentence = true)
        {
            if (sentence == null)
            {
                throw new ArgumentNullException(nameof(sentence));
            }

            string[] alignSplitWrds = new string[0];
            string[] words = sentence.Split(' ');

            //if alignment information exists, use them otherwise return a simple splitting using white space delimiter
            if (alignments != null && alignments.Length > 0)
            {
                List<string> outWords = new List<string>();

                //index value to use in the splitted alignment pairs
                //ex :  var alignment   =   "0:2-0:1 4:6-3:6" , after splitting using white space it will be :
                //                      =   {"0:2-0:1", "4:6-3:6"}, after splitting each pair using "-" each pair will be like :
                //                      =   {"0:2", "0:1"} the first part of the splitted array represents the source message alignment information 
                //which will be referenced using wordIndexInAlignment = 0 
                //while the second part of this string array represents the translated alignment information which will be referenced using wordIndexInAlignment = 1

                //TODO introducer a better data structure for alignment information for less confusing interpretation of string parsing
                int wordIndexInAlignment = 0;
                if (!isSourceSentence)
                {
                    wordIndexInAlignment = 1;
                    // reorder alignments in case of target translated  message to get ordered output words.
                    Array.Sort(alignments, (x, y) => Int32.Parse(x.Split('-')[wordIndexInAlignment].Split(':')[0]).CompareTo(Int32.Parse(y.Split('-')[wordIndexInAlignment].Split(':')[0])));
                }

                //remove all spaces and use alignment information to split the words instead
                string withoutSpaceSentence = sentence.Replace(" ", "");

                foreach (string alignData in alignments)
                {
                    //initialize alignSplitWrds with e,pty list outWords
                    alignSplitWrds = outWords.ToArray();
                    //get the index range for the current word
                    //ex : before splittig alignment entry = "0:2-0:1"
                    //after splittig alignment entry = {"0:2", "0:1"} and whether wordIndexInAlignment = 0 or 1
                    //the value of wordIndexes according to the example will be either "0:2" or "0:1"
                    string wordIndexes = alignData.Split('-')[wordIndexInAlignment];
                    //get the start index of the current word
                    int startIndex = Int32.Parse(wordIndexes.Split(':')[0]);
                    //calculate the length of the word using start index and ending index
                    int length = Int32.Parse(wordIndexes.Split(':')[1]) - startIndex + 1;
                    //get the word that matches the calculated boundaries
                    string word = sentence.Substring(startIndex, length);
                    //the following block of code is customly written to hotfix a problem with the translator api output, that sometimes it duplicates alignment information,
                    //which leads to a dublicated token, so the whole trick is to make sure before adding the token to splitted output array
                    //that the this token exists in the correct sequence in the original string
                    string[] newWrds = new string[outWords.Count + 1];
                    if (newWrds.Length > 1)
                        alignSplitWrds.CopyTo(newWrds, 0);
                    newWrds[outWords.Count] = word;
                    string subSentence = Join("", newWrds.ToArray());
                    if (withoutSpaceSentence.Contains(subSentence))
                        outWords.Add(word);
                }
                alignSplitWrds = outWords.ToArray();
            }
            char[] punctuationChars = new char[] { '.', ',', '?', '!' };
            if (Join("", alignSplitWrds).TrimEnd(punctuationChars) == Join("", words).TrimEnd(punctuationChars))
                return alignSplitWrds;
            return words;
        }

        /// <summary>
        /// Parsing alignment information onto a dictionary
        /// where key is word index in source
        /// and value is word index in translated text.
        /// </summary>
        /// <param name="alignments">Alignments array.</param>
        /// <param name="sourceTokens">Array containing source message tokens.</param>
        /// <param name="translatedTokens">Array containing translated message tokens.</param>
        /// <returns>A <see cref="Dictionary{Int32, UInt32}"/> represents the source to target tokens mapping.</returns>
        public static Dictionary<int, int> WordAlignmentParse(string[] alignments, string[] sourceTokens, string[] translatedTokens)
        {
            //parameter validation
            ValidateParametersWordAlignmentParse(alignments, sourceTokens, translatedTokens);
            Dictionary<int, int> alignMap = new Dictionary<int, int>();
            string sourceMessage = Join(" ", sourceTokens);
            string translatedMessage = Join(" ", translatedTokens);

            foreach (string alignData in alignments)
            {
                //split each alignment pair ie :  source and target into separate string
                //each alignment pair output from translator api follows the following pattern [source_start_character_index]:[source_ending_character_index]-[target_start_character_index]:[target_ending_character_index]
                //ex : the following alignment pair "0:2-0:1" means characters from index 0 to index 2 in source message maps to characters from index 0 to index 1 in translated message ,
                //that could map to something like trsnlating "mon" in French to "my" in English
                string[] wordIndexes = alignData.Split('-');
                //source token start index
                int srcStartIndex = Int32.Parse(wordIndexes[0].Split(':')[0]);
                //source token length
                int srcLength = Int32.Parse(wordIndexes[0].Split(':')[1]) - srcStartIndex + 1;
                //find the token/word that matches the calculated substring boundaries in source message
                string srcWrd = sourceMessage.Substring(srcStartIndex, srcLength);
                //get source word index
                int sourceWordIndex = Array.FindIndex(sourceTokens, row => row == srcWrd);

                //target/translated token start index
                int trgstartIndex = Int32.Parse(wordIndexes[1].Split(':')[0]);
                //target/translated token length
                int trgLength = Int32.Parse(wordIndexes[1].Split(':')[1]) - trgstartIndex + 1;
                //find the token/word that matches the calculated substring boundaries in translated message
                string trgWrd = translatedMessage.Substring(trgstartIndex, trgLength);
                //get target/translated word index
                int targetWordIndex = Array.FindIndex(translatedTokens, row => row == trgWrd);

                if (sourceWordIndex >= 0 && targetWordIndex >= 0)
                {
                    alignMap[sourceWordIndex] = targetWordIndex;
                }
            }
            return alignMap;
        }

        /// <summary>
        /// Validate arguments of <see cref="WordAlignmentParse(string[], string[], string[])"/>.
        /// </summary>
        /// <param name="alignments">Alignments array.</param>
        /// <param name="sourceTokens">Array containing source message tokens.</param>
        /// <param name="translatedTokens">Array containing translated message tokens.</param>
        private static void ValidateParametersWordAlignmentParse(string[] alignments, string[] sourceTokens, string[] translatedTokens)
        {
            //null check the alignments array
            if (alignments == null)
            {
                throw new ArgumentNullException(nameof(alignments));
            }

            //validate alignment format
            ValidateAlignmentsFormat(alignments);

            //null check the source tokens array
            if (sourceTokens == null)
            {
                throw new ArgumentNullException(nameof(sourceTokens));
            }

            //null check the translated tokens array
            if (translatedTokens == null)
            {
                throw new ArgumentNullException(nameof(translatedTokens));
            }
        }

        /// <summary>
        /// Use alignment information source sentence and translated sentence
        /// to keep a specific word from the source onto target translation.
        /// </summary>
        /// <param name="alignmentMap">Dictionary containing the alignments.</param>
        /// <param name="sourceTokens">Source message tokens.</param>
        /// <param name="translatedTokens">Translated message tokens.</param>
        /// <param name="sourceTokenIndex">Source word index.</param>
        /// <returns>An array that represents the translated tokens after processing.</returns>
        public static string[] KeepSourceWordInTranslation(Dictionary<int, int> alignmentMap, string[] sourceTokens, string[] translatedTokens, int sourceTokenIndex)
        {
            //parameter validation
            ValidateParametersKeepSourceWordInTranslation(alignmentMap, sourceTokens, translatedTokens, sourceTokenIndex);

            if (alignmentMap.ContainsKey(sourceTokenIndex))
            {
                translatedTokens[alignmentMap[sourceTokenIndex]] = sourceTokens[sourceTokenIndex];
            }
            return translatedTokens;
        }

        /// <summary>
        /// Validate arguments of <see cref="KeepSourceWordInTranslation(Dictionary{int, int}, string[], string[], int)"/>.
        /// </summary>
        /// <param name="alignmentMap">Dictionary containing the alignments.</param>
        /// <param name="sourceWords">Source message tokens.</param>
        /// <param name="translatedWords">Translated message tokens.</param>
        /// <param name="sourceWordIndex">Source word index.</param>
        private static void ValidateParametersKeepSourceWordInTranslation(Dictionary<int, int> alignmentMap, string[] sourceWords, string[] translatedWords, int sourceWordIndex)
        {
            if (alignmentMap == null)
            {
                throw new ArgumentNullException(nameof(alignmentMap));
            }

            //validate that all values of alignment map are +ve values
            if (alignmentMap.Count > 0)
            {
                foreach (KeyValuePair<int, int> alignmentElement in alignmentMap)
                {
                    if (alignmentElement.Value < 0)
                    {
                        throw new ArgumentException(MessagesProvider.NegativeValueAlignmentMapEntryErrorMessage);
                    }
                }
            }

            if (sourceWords == null)
            {
                throw new ArgumentNullException(nameof(sourceWords));
            }

            if (translatedWords == null)
            {
                throw new ArgumentNullException(nameof(translatedWords));
            }

            //validate that sourceWordIndex is +ve value
            if (sourceWordIndex < 0)
            {
                throw new ArgumentException(MessagesProvider.NegativeValueSourceWordIndexErrorMessage);
            }
        }

        /// <summary>
        /// Validate if alignment array is in the right format.
        /// </summary>
        /// <param name="alignments">Alignments array.</param>
        private static void ValidateAlignmentsFormat(string[] alignments)
        {
            if (alignments == null)
            {
                throw new ArgumentNullException(nameof(alignments));
            }

            if (alignments.Length > 0)
            {
                foreach (string alignmentElement in alignments)
                {
                    //check if the alignment element matches the alignment pattern ,
                    //for example : 0:2-0:5 , the previous is a valid pattern,
                    //another example : 0:-23-0:13 this is an incorrect pattern, 
                    //another example : test-test this is an incorrect pattern
                    if (!Regex.IsMatch(alignmentElement, "[0-9]*:[0-9]*-[0-9]*:[0-9]*"))
                    {
                        throw new FormatException(MessagesProvider.IncorrectAlignmentFormatErrorMessage);
                    }
                }
            }
        }
    }
}
