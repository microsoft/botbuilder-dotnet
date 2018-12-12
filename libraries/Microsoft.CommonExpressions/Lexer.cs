using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Expressions
{
    public static class Lexer
    {
        public static readonly string[] Patterns = new[]
        {
            @"[+-]?\d+(?:\.\d+)?",
            "'[^']*'",
            @"[a-z][a-z0-9]*",
        }.Concat(
            OperatorTable.All.Select(e => e.Token).Distinct().OrderByDescending(e => e.Length).Select(Regex.Escape)
        ).ToArray();

        public static readonly Regex Matcher = new Regex(
            $"^\\s*(({string.Join(")|(", Patterns)}))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public sealed class Identifier
        {
            public string Name { get; private set; }
            public static Identifier From(string name) => new Identifier() { Name = name };
        }

        public static IEnumerable<Token> Tokens(string text)
        {
            while (!string.IsNullOrWhiteSpace(text))
            {
                var match = Matcher.Match(text);
                if (match.Success)
                {
                    var groups = match.Groups;
                    var input = groups[1].Value;
                    // token is a literal number
                    if (groups[2].Success)
                    {
                        if (int.TryParse(input, out var number))
                        {
                            yield return Token.From(input, number);
                        }
                        else
                        {
                            yield return Token.From(input, double.Parse(input));
                        }
                    }
                    // token is a literal string
                    else if (groups[3].Success)
                    {
                        yield return Token.From(input, input.Trim('\''));
                    }
                    // token is an identifier
                    else if (groups[4].Success)
                    {
                        yield return Token.From(input, Identifier.From(input));
                    }
                    // token is an operator
                    else
                    {
                        yield return Token.From(input, null);
                    }

                    text = text.Substring(match.Length);
                    continue;
                }

                throw new Exception();
            }
        }

        public static void Next(IEnumerator<Token> tokens) => tokens.MoveNext();

        public static void Match(IEnumerator<Token> tokens, string token)
        {
            if (tokens.Current.Input != token)
            {
                throw new Exception();
            }

            Next(tokens);
        }
    }
}
