using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder
{
    public class RegExpRecognizerSettings
    {
        /// <summary>
        /// Minimum score, on a scale from 0.0 to 1.0, that should be returned for a matched 
        /// expression.This defaults to a value of 0.0. 
        /// </summary>
        public double MinScore { get; set; } = 0.0;
    }
    public class RegExLocaleMap
    {
        private Dictionary<string, List<Regex>> _map = new Dictionary<string, List<Regex>>();

        public RegExLocaleMap()
        {
        }

        public RegExLocaleMap(List<Regex> items)
        {
            _map["*"] = items;
        }
        

        public List<Regex> GetLocale(string locale)
        {
            if (_map.ContainsKey(locale))
                return _map[locale];
            else
                return new List<Regex>();
        }

        public Dictionary<string, List<Regex>> Map
        {
            get { return _map; }
        }

    }

    public class RegExpRecognizerMiddleare : IntentRecognizerMiddleware
    {
        private RegExpRecognizerSettings _settings;
        private Dictionary<string, RegExLocaleMap> _intents = new Dictionary<string, RegExLocaleMap>();

        public RegExpRecognizerMiddleare() : this(new RegExpRecognizerSettings() { MinScore = 0.0 })
        {
        }

        public RegExpRecognizerMiddleare(RegExpRecognizerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException("settings");
            if (_settings.MinScore < 0 || _settings.MinScore > 1.0)
            {
                throw new ArgumentException($"RegExpRecognizerMiddleware: a minScore of {_settings.MinScore} is out of range.");
            }

            this.OnRecognize(async (context) =>
           {
               IList<Intent> intents = new List<Intent>();
               string utterance = CleanString(context.Request.Text);
               double minScore = _settings.MinScore;

               foreach (var name in _intents.Keys)
               {
                   var map = _intents[name];
                   List<Regex> expressions = GetExpressions(context, map);
                   Intent top = null;
                   foreach (Regex exp in expressions)
                   {
                       List<string> entityTypes = new List<string>();
                       Intent intent = Recognize(utterance, exp, entityTypes, minScore);
                       if (intent != null)
                       {
                           if (top == null)
                           {
                               top = intent;
                           }
                           else if (intent.Score > top.Score)
                           {
                               top = intent;
                           }
                       }

                       if (top != null)
                       {
                           top.Name = name;
                           intents.Add(top);
                       }
                   }
               }
               return intents;
           });
        }

        public RegExpRecognizerMiddleare AddIntent(string intentName, Regex regex)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");

            return AddIntent(intentName, new List<Regex> { regex });
        }
        public RegExpRecognizerMiddleare AddIntent(string intentName, List<Regex> regexList)
        {
            if (regexList == null)
                throw new ArgumentNullException("regexList");

            return AddIntent(intentName, new RegExLocaleMap(regexList)); 
        }

        public RegExpRecognizerMiddleare AddIntent(string intentName, RegExLocaleMap map)
        {
            if (string.IsNullOrWhiteSpace(intentName))
                throw new ArgumentNullException("intentName");

            if (_intents.ContainsKey(intentName))
                throw new ArgumentException($"RegExpRecognizer: an intent name '{intentName}' already exists.");

            _intents[intentName] = map;

            return this;
        }        
        private List<Regex> GetExpressions(IBotContext context, RegExLocaleMap map)
        {
            string locale = string.IsNullOrWhiteSpace(context.Request.Locale) ? "*" : context.Request.Locale;
            List<Regex> entry = map.GetLocale(locale);
            return entry;
        }
        private static string CleanString(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();
        }

        public static Intent Recognize(string text, Regex expression, List<string> entityTypes, double minScore)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");

            if (expression == null)
                throw new ArgumentNullException("expression");

            if (entityTypes == null)
                throw new ArgumentNullException("entity Types");

            if (minScore < 0 || minScore > 1.0)
                throw new ArgumentOutOfRangeException($"RegExpRecognizer: a minScore of '{minScore}' is out of range for expression '{expression.ToString()}'");

            var matched = expression.Match(text);
            if (matched.Success)
            {
                double coverage = (double) matched.Length / (double) text.Length;
                double score = minScore + ((1.0 - minScore) * coverage);

                //ToDo: Entity Matching

                return new Intent()
                {
                    Name = expression.ToString(),
                    Score = score
                };
            }

            return null;
        }
    }
}
