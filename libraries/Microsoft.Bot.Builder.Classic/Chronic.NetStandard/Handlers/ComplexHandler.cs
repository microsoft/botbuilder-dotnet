using System;
using System.Collections.Generic;
using System.Linq;

namespace Chronic.Handlers
{
    public class ComplexHandler
    {
        readonly HandlerPattern[] _patterns;
        public IHandler BaseHandler { get; private set; }

        public ComplexHandler(IHandler handler, IEnumerable<HandlerPattern> patterns)
        {
            BaseHandler = handler;
            _patterns = patterns.ToArray();
        }

        public bool Match(IList<Token> tokens, HandlerRegistry registry)
        {
            var tokenIndex = 0;
            foreach (var pattern in _patterns)
            {
                var isOptional = pattern.IsOptional;
                var isRequired = !isOptional;
                var thereAreNoMoreTokens = tokenIndex == tokens.Count;

                if (pattern is TagPattern)
                {
                    var match = tokenIndex < tokens.Count &&
                        tokens[tokenIndex].IsTaggedAs((pattern as TagPattern).TagType);
                    if (match == false && isRequired)
                    {
                        return false;
                    }
                    if (match)
                    {
                        tokenIndex++;
                    }
                }
                else if (pattern is RepeatPattern)
                {
                    var repetition = pattern as RepeatPattern;
                    int advancement;
                    var match = repetition.Match(tokens.Skip(tokenIndex).ToList(), out advancement);
                    if (match == false && isRequired)
                    {
                        return false;
                    }
                    if (match)
                    {
                        tokenIndex += advancement;
                    }
                }
                else if (pattern is HandlerTypePattern)
                {
                    if (isOptional && thereAreNoMoreTokens)
                    {
                        return true;
                    }
                    var subHandlers = registry.GetHandlers((pattern as HandlerTypePattern).Type);
                    foreach (var handler in subHandlers)
                    {
                        if (handler.Match(tokens.Skip(tokenIndex).ToList(), registry))
                        {
                            return true;
                        }
                    }
                }
            }

            if (tokenIndex != tokens.Count)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} => {1}",
                String.Join("", _patterns.Select(pattern => pattern.ToString())),
                ((BaseHandler != null) ? BaseHandler.GetType().Name : "<null>"));
        }
    }
}