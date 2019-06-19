using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Expressions
{
    internal class RegexLRUEntity
    {
        public RegexLRUEntity(long timestamp, Regex regex)
        {
            this.Timestamp = timestamp;
            this.Regex = regex;
        }

        /// <summary>
        /// Gets time when this item inserted.
        /// </summary>
        /// <value>
        /// Time when this item inserted.
        /// </value>
        public long Timestamp { get; }

        /// <summary>
        /// Gets regex entity of cache.
        /// </summary>
        /// <value>
        /// Regex entity of cache.
        /// </value>
        public Regex Regex { get; }
    }

    public class CommonRegex
    {
        private const int CacheCapacity = 15;
        private static Dictionary<string, RegexLRUEntity> lruCacheMap = new Dictionary<string, RegexLRUEntity>();
        private static readonly object LockObj = new object();

        public static Regex CreateRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern) || !IsCommonRegex(pattern))
            {
                throw new ArgumentException("A regular expression parsing error occurred.");
            }

            var regex = GetCacheValue(pattern);
            if (regex == null)
            {
                regex = new Regex(pattern);
                SetChacheValue(pattern, regex);
            }

            return regex as Regex;
        }

        private static bool IsCommonRegex(string pattern)
        {
            try
            {
                AntlrParse(pattern);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static IParseTree AntlrParse(string pattern)
        {
            var inputStream = new AntlrInputStream(pattern);
            var lexer = new CommonRegexLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new CommonRegexParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ErrorListener());
            parser.BuildParseTree = true;
            return parser.parse();
        }

        private static Regex GetCacheValue(string key)
        {
            if (key != null && lruCacheMap.TryGetValue(key, out var val))
            {
                // update timestamp
                SetChacheValue(key, val.Regex);
                return val.Regex;
            }
            else
            {
                return null;
            }
        }

        private static void SetChacheValue(string key, Regex value)
        {
            lock (LockObj)
            {
                if (key != null)
                {
                    if (lruCacheMap.ContainsKey(key))
                    {
                        // update timestamp
                        lruCacheMap[key] = new RegexLRUEntity(DateTime.Now.Ticks, value);
                    }
                    else
                    {
                        // delete least recently used objects
                        if (lruCacheMap.Count > CacheCapacity)
                        {
                            var deleteKeys = lruCacheMap
                                .OrderBy(item => item.Value.Timestamp)
                                .Take(lruCacheMap.Count - CacheCapacity)
                                .Select(u => u.Key).ToList();
                            deleteKeys.ForEach(u => lruCacheMap.Remove(u));
                        }
                        lruCacheMap.Add(key, new RegexLRUEntity(DateTime.Now.Ticks, value));
                    }
                }
            }
        }
    }

    internal class ErrorListener : BaseErrorListener
    {
        public static readonly ErrorListener Instance = new ErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e) => throw new Exception($"Regular expression is invalid.");
    }
}
