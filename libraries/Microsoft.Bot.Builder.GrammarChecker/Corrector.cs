using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.GrammarChecker.CorrectingInfos;

namespace Microsoft.Bot.Builder.GrammarChecker
{
    public class Corrector : ICorrector
    {
        private Dictionary<string, string> sg_pl_n;
        private Dictionary<string, string> pl_sg_n;
        private HashSet<string> singular_n;
        private HashSet<string> plural_n;
        private HashSet<string> singular_pn;
        private HashSet<string> plural_pn;
        private Dictionary<string, string> first_third_v;
        private Dictionary<string, string> third_first_v;
        private HashSet<string> first_v;
        private HashSet<string> third_v;
        private HashSet<string> elision_expection_w;
        private HashSet<string> englishNumberDict;
        private HashSet<string> englishOrdinalDict;
        private HashSet<string> englishNameDict;

        public Corrector()
        {
            // noun dictionary
            sg_pl_n = new Dictionary<string, string>();
            string path = GetExampleFilePath("singular_to_plural_noun.txt");
            ReadMapFile(path, sg_pl_n);

            pl_sg_n = sg_pl_n
                .ToLookup(kp => kp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

            singular_n = new HashSet<string>(sg_pl_n.Keys.ToList());
            plural_n = new HashSet<string>(sg_pl_n.Values.ToList());

            // pronoun dictionary
            singular_pn = new HashSet<string>();
            path = GetExampleFilePath("singular_pronoun.txt");
            ReadSetFile(path, singular_pn);

            plural_pn = new HashSet<string>();
            path = GetExampleFilePath("plural_pronoun.txt");
            ReadSetFile(path, plural_pn);

            // verb dictionary
            first_third_v = new Dictionary<string, string>();
            path = GetExampleFilePath("1st_to_3sg_verb.txt");
            ReadMapFile(path, first_third_v);

            third_first_v = first_third_v
                .ToLookup(kp => kp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

            first_v = new HashSet<string>(first_third_v.Keys.ToList());
            third_v = new HashSet<string>(first_third_v.Values.ToList());

            // elision exception
            elision_expection_w = new HashSet<string>();
            path = GetExampleFilePath("elision_exception.txt");
            ReadSetFile(path, elision_expection_w);

            // english number dictionary
            englishNumberDict = new HashSet<string>();
            path = GetExampleFilePath("number.txt");
            ReadSetFile(path, englishNumberDict);

            // english ordinal dictionary
            englishOrdinalDict = new HashSet<string>();
            path = GetExampleFilePath("ordinal.txt");
            ReadSetFile(path, englishOrdinalDict);

            // english name dictionary
            englishNameDict = new HashSet<string>();
            path = GetExampleFilePath("name.txt");
            ReadSetFile(path, englishNameDict);
        }

        public string CorrectElisionWord(string inputWord, string nextWord, CorrectingInfo correctingInfo)
        {
            var outputWord = inputWord;

            if (Regex.IsMatch(nextWord, "^(a|e|i|o|u)"))
            {
                if (elision_expection_w.Contains(nextWord.ToLower()))
                {
                    outputWord = "a";
                }
                else
                {
                    outputWord = "an";
                }
            }
            else
            {
                if (elision_expection_w.Contains(nextWord.ToLower()))
                {
                    outputWord = "an";
                }
                else
                {
                    outputWord = "a";
                }
            }

            return outputWord;
        }

        public string CorrectNounWord(string inputWord, CorrectingInfo correctingInfo)
        {
            var outputWord = inputWord;
            if (singular_n.Contains(inputWord.ToLower()) && correctingInfo.NumberInfo.Feature == Number.Singular)
            {
                outputWord = inputWord;
            }
            else if (plural_n.Contains(inputWord.ToLower()) && correctingInfo.NumberInfo.Feature == Number.Plural)
            {
                outputWord = inputWord;
            }
            else if (singular_n.Contains(inputWord.ToLower()) && correctingInfo.NumberInfo.Feature == Number.Plural)
            {
                sg_pl_n.TryGetValue(inputWord.ToLower(), out outputWord);
            }
            else if (plural_n.Contains(inputWord.ToLower()) && correctingInfo.NumberInfo.Feature == Number.Singular)
            {
                pl_sg_n.TryGetValue(inputWord.ToLower(), out outputWord);
            }

            return outputWord;
        }

        public string CorrectVerbWord(string inputWord, List<string> inputWords, CorrectingInfo correctingInfo)
        {
            var outputWord = inputWord;

            var ref_word = inputWords[correctingInfo.VerbInfo.ReferencePosition];
            if (singular_n.Contains(ref_word.ToLower())
                || singular_pn.Contains(ref_word.ToLower())
                || englishNameDict.Contains(ref_word)
                || correctingInfo.VerbInfo.SubjectNumber == Number.Singular)
            {
                correctingInfo.VerbInfo.Feature = SubjectVerb.Singular;
            }
            else if (plural_n.Contains(ref_word.ToLower())
                || plural_pn.Contains(ref_word.ToLower())
                || correctingInfo.VerbInfo.SubjectNumber == Number.Plural
                || correctingInfo.VerbInfo.SubjectNumberFromPosTagging == Number.Plural)
            {
                correctingInfo.VerbInfo.Feature = SubjectVerb.Plural;
            }
            else
            {
                return outputWord;
            }

            outputWord = GetTransVerbWord(inputWord, correctingInfo);

            return outputWord;
        }
        
        public bool IsNumber(string word, out Number number)
        {
            number = Number.None;
            if (word.Length == 0)
            {
                return false;
            }

            // parse int number like 2
            int outInt = 0;
            if (int.TryParse(word, out outInt))
            {
                if (outInt == 1)
                {
                    number = Number.Singular;
                }
                else
                {
                    number = Number.Plural;
                }

                return true;
            }

            // parse float number like 2.2
            float outFloat = 0.0f;
            if (float.TryParse(word, out outFloat))
            {
                if (outFloat == 1.0f)
                {
                    number = Number.Singular;
                }
                else
                {
                    number = Number.Plural;
                }

                return true;
            }

            // English number table to parse English ordinal number.e.g. three or four
            if (this.englishNumberDict.Contains(word.ToLower()))
            {
                if (word.ToLower().Equals("one"))
                {
                    number = Number.Singular;
                }
                else
                {
                    number = Number.Plural;
                }

                return true;
            }

            // English ordinal number table to parse English ordinal number. e.g. third or 3rd
            if (this.englishOrdinalDict.Contains(word.ToLower()))
            {
                number = Number.Singular;

                return true;
            }

            return false;
        }

        private string GetTransVerbWord(string inputWord, CorrectingInfo correctingInfo)
        {
            var outputWord = inputWord;

            if (first_v.Contains(inputWord.ToLower()) && correctingInfo.VerbInfo.Feature == SubjectVerb.Plural)
            {
                outputWord = inputWord;
            }
            else if (third_v.Contains(inputWord.ToLower()) && correctingInfo.VerbInfo.Feature == SubjectVerb.Singular)
            {
                outputWord = inputWord;
            }
            else if (first_v.Contains(inputWord.ToLower()) && correctingInfo.VerbInfo.Feature == SubjectVerb.Singular)
            {
                first_third_v.TryGetValue(inputWord.ToLower(), out outputWord);
            }
            else if (third_v.Contains(inputWord.ToLower()) && correctingInfo.VerbInfo.Feature == SubjectVerb.Plural)
            {
                third_first_v.TryGetValue(inputWord.ToLower(), out outputWord);
            }

            return outputWord;
        }

        private string GetExampleFilePath(string fileName, string locale = "en_us")
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            path = Path.Combine(path, locale, fileName);

            return path;
        }

        private void ReadMapFile(string path, Dictionary<string, string> word_map)
        {
            var line = string.Empty;
            using (var sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] keys = line.Replace("\n", string.Empty).Split(':');
                    if (!word_map.ContainsKey(keys[0]))
                    {
                        word_map.Add(keys[0], keys[1]);
                    }
                }
            }
        }

        private void ReadSetFile(string path, HashSet<string> word_set)
        {
            var line = string.Empty;
            using (var sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string key = line.Replace("\n", string.Empty);
                    word_set.Add(key);
                }
            }
        }
    }
}
