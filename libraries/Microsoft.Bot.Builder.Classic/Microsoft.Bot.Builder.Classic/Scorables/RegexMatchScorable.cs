// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    [Serializable]
    public abstract class AttributeString : Attribute, IEquatable<AttributeString>
    {
        protected abstract string Text { get; }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Text})";
        }

        bool IEquatable<AttributeString>.Equals(AttributeString other)
        {
            return other != null
                && object.Equals(this.Text, other.Text);
        }

        public override bool Equals(object other)
        {
            return base.Equals(other as AttributeString);
        }

        public override int GetHashCode()
        {
            return this.Text.GetHashCode();
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Scorables
{
    /// <summary>
    /// This attribute is used to specify the regular expression pattern to be used 
    /// when applying the regular expression scorable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public sealed class RegexPatternAttribute : AttributeString
    {
        /// <summary>
        /// The regular expression pattern.
        /// </summary>
        public readonly string Pattern;

        /// <summary>
        /// Construct the <see cref="RegexPatternAttribute"/>. 
        /// </summary>
        /// <param name="pattern">The regular expression pattern.</param>
        public RegexPatternAttribute(string pattern)
        {
            SetField.NotNull(out this.Pattern, nameof(pattern), pattern);
        }

        protected override string Text
        {
            get
            {
                return this.Pattern;
            }
        }
    }
}

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    public sealed class RegexMatchScorableFactory : IScorableFactory<IResolver, Match>
    {
        private readonly Func<string, Regex> make;

        public RegexMatchScorableFactory(Func<string, Regex> make)
        {
            SetField.NotNull(out this.make, nameof(make), make);
        }

        IScorable<IResolver, Match> IScorableFactory<IResolver, Match>.ScorableFor(IEnumerable<MethodInfo> methods)
        {
            var specs =
                from method in methods
                from pattern in InheritedAttributes.For<RegexPatternAttribute>(method)
                select new { method, pattern };

            var scorableByMethod = methods.ToDictionary(m => m, m => new MethodScorable(m));

            // for a given regular expression pattern, fold the corresponding method scorables together to enable overload resolution
            var scorables =
                from spec in specs
                group spec by spec.pattern into patterns
                let method = patterns.Select(m => scorableByMethod[m.method]).ToArray().Fold(BindingComparer.Instance)
                let regex = this.make(patterns.Key.Pattern)
                select new RegexMatchScorable<IBinding, IBinding>(regex, method);

            var all = scorables.ToArray().Fold(MatchComparer.Instance);

            return all;
        }
    }

    /// <summary>
    /// Static helper methods for RegexMatchScorable.
    /// </summary>
    public static partial class RegexMatchScorable
    {
        public static readonly Func<Capture, string> GetOriginalString
            = (Func<Capture, string>)
            typeof(Capture)
            .GetMethod("GetOriginalString", BindingFlags.Instance | BindingFlags.NonPublic)
            .CreateDelegate(typeof(Func<Capture, string>));

        /// <summary>
        /// Calculate a normalized 0-1 score for a regular expression match.
        /// </summary>
        public static double ScoreFor(Match match)
        {
            var numerator = match.Value.Length;
            var denominator = GetOriginalString(match).Length;
            var score = ((double)numerator) / denominator;
            return score;
        }
    }

    /// <summary>
    /// Scorable to represent a regular expression match against an activity's text.
    /// </summary>
    [Serializable]
    public sealed class RegexMatchScorable<InnerState, InnerScore> : ResolverScorable<RegexMatchScorable<InnerState, InnerScore>.Scope, Match, InnerState, InnerScore>
    {
        private readonly Regex regex;

        public sealed class Scope : ResolverScope<InnerScore>
        {
            public readonly Regex Regex;
            public readonly Match Match;

            public Scope(Regex regex, Match match, IResolver inner)
                : base(inner)
            {
                SetField.NotNull(out this.Regex, nameof(regex), regex);
                SetField.NotNull(out this.Match, nameof(match), match);
            }

            public override bool TryResolve(Type type, object tag, out object value)
            {
                var name = tag as string;
                if (name != null)
                {
                    var capture = this.Match.Groups[name];
                    if (capture != null && capture.Success)
                    {
                        if (type.IsAssignableFrom(typeof(Capture)))
                        {
                            value = capture;
                            return true;
                        }
                        else if (type.IsAssignableFrom(typeof(string)))
                        {
                            value = capture.Value;
                            return true;
                        }
                    }
                }

                if (type.IsAssignableFrom(typeof(Regex)))
                {
                    value = this.Regex;
                    return true;
                }

                if (type.IsAssignableFrom(typeof(Match)))
                {
                    value = this.Match;
                    return true;
                }

                var captures = this.Match.Captures;
                if (type.IsAssignableFrom(typeof(CaptureCollection)))
                {
                    value = captures;
                    return true;
                }

                // i.e. for IActivity
                return base.TryResolve(type, tag, out value);
            }
        }

        public RegexMatchScorable(Regex regex, IScorable<IResolver, InnerScore> inner)
            : base(inner)
        {
            SetField.NotNull(out this.regex, nameof(regex), regex);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.regex}, {this.inner})";
        }

        protected override async Task<Scope> PrepareAsync(IResolver resolver, CancellationToken token)
        {
            IMessageActivity message;
            if (!resolver.TryResolve(null, out message))
            {
                return null;
            }

            var text = message.Text;
            if (text == null)
            {
                return null;
            }

            var match = this.regex.Match(text);
            if (!match.Success)
            {
                return null;
            }

            var scope = new Scope(this.regex, match, resolver);
            scope.Item = resolver;
            scope.Scorable = this.inner;
            scope.State = await this.inner.PrepareAsync(scope, token);
            return scope;
        }

        protected override Match GetScore(IResolver resolver, Scope state)
        {
            return state.Match;
        }
    }

    public sealed class MatchComparer : IComparer<Match>
    {
        public static readonly IComparer<Match> Instance = new MatchComparer();

        private MatchComparer()
        {
        }

        int IComparer<Match>.Compare(Match one, Match two)
        {
            Func<Match, Pair<bool, double>> PairFor = match => Pair.Create
            (
                match.Success,
                RegexMatchScorable.ScoreFor(match)
            );

            var pairOne = PairFor(one);
            var pairTwo = PairFor(two);
            return pairOne.CompareTo(pairTwo);
        }
    }
}
