using System;
using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;

namespace Chronic
{
    public static class TokenCollectionExtensions
    {
        public static IEnumerable<Token> DealiasAndDisambiguateTimes(
            this IEnumerable<Token> @this, Options options)
        {
            // handle aliases of am/pm
            // 5:00 in the morning => 5:00 am
            // 7:00 in the evening => 7:00 pm
            var tokens = @this.ToList();
            var dayPortionIndex = -1;
            var tokenSize = tokens.Count();
            for (var i = 0; dayPortionIndex == -1 && i < tokenSize; i++)
            {
                Token token = tokens[i];
                if (token.GetTag<IRepeaterDayPortion>() != null)
                {
                    dayPortionIndex = i;
                }
            }

            var timeIndex = -1;
            for (var i = 0; timeIndex == -1 && i < tokenSize; i++)
            {
                var t = tokens[i];
                if (t.GetTag<RepeaterTime>() != null)
                {
                    timeIndex = i;
                }
            }

            if (dayPortionIndex != -1 && timeIndex != -1)
            {
                var t1 = tokens[dayPortionIndex];
                var t1Tag = t1.GetTag<IRepeaterDayPortion>();

                var t1TagType = t1Tag.RawValue;
                if (t1TagType.Equals(DayPortion.MORNING))
                {
                    t1.Untag<IRepeaterDayPortion>();
                    t1.Tag(new EnumRepeaterDayPortion(DayPortion.AM));
                }
                else if (t1TagType.Equals(DayPortion.AFTERNOON) ||
                    t1TagType.Equals(DayPortion.EVENING) ||
                    t1TagType.Equals(DayPortion.NIGHT))
                {
                    t1.Untag<IRepeaterDayPortion>();
                    t1.Tag(new EnumRepeaterDayPortion(DayPortion.PM));
                }
            }

            if (options.AmbiguousTimeRange != 0)
            {
                var ambiguousTokens = new List<Token>();
                for (var i = 0; i < tokenSize; i++)
                {
                    var token = tokens[i];
                    ambiguousTokens.Add(token);
                    Token nextToken = (i < tokenSize - 1) ? tokens[i + 1] : null;
                    if (token.IsTaggedAs<RepeaterTime>() &&
                        token.GetTag<RepeaterTime>().Value.IsAmbiguous &&
                            (nextToken == null ||
                                nextToken.GetTag<IRepeaterDayPortion>() == null))
                    {
                        var disambiguator = new Token("disambiguator");
                        disambiguator.Tag(
                            new IntegerRepeaterDayPortion(
                                options.AmbiguousTimeRange));
                        ambiguousTokens.Add(disambiguator);
                    }
                }
                tokens = ambiguousTokens;
            }

            return tokens;
        }

        public static IList<IRepeater> GetRepeaters(
            this IEnumerable<Token> @this)
        {
            return @this
                .Where(token => token.IsTaggedAs<IRepeater>())
                .Select(token => token.GetTag<IRepeater>())
                .OrderBy(tag => tag.GetWidth())
                .Reverse()
                .ToList();
        }

        public static Span GetAnchor(this IEnumerable<Token> @this,
                                     Options options)
        {
            var grabber = new Grabber(Grabber.Type.This);
            Pointer.Type pointer = Pointer.Type.Future;
            List<Token> tokens = @this.ToList();

            IList<IRepeater> repeaters = tokens.GetRepeaters();
            for (int i = 0; i < repeaters.Count; i++)
            {
                tokens.RemoveAt(tokens.Count - 1);
            }

            if (tokens.Count > 0 && tokens[0].IsTaggedAs<Grabber>())
            {
                grabber = tokens[0].GetTag<Grabber>();
                tokens.RemoveAt(tokens.Count - 1);
            }

            IRepeater head = repeaters[0];
            repeaters.RemoveAt(0);

            head.Now = options.Clock();

            Span outerSpan;
            Grabber.Type grabberType = grabber.Value;
            if (grabberType == Grabber.Type.Last)
            {
                outerSpan = head.GetNextSpan(Pointer.Type.Past);
            }
            else if (grabberType == Grabber.Type.This)
            {
                if (repeaters.Count > 0)
                {
                    outerSpan = head.GetCurrentSpan(Pointer.Type.None);
                }
                else
                {
                    outerSpan = head.GetCurrentSpan(options.Context);
                }
            }
            else if (grabberType == Grabber.Type.Next)
            {
                outerSpan = head.GetNextSpan(Pointer.Type.Future);
            }
            else
            {
                throw new ArgumentException("Invalid grabber type " +
                    grabberType + ".");
            }
            
            Logger.Log(() => "Handler-class: " + head.GetType().Name);
            Logger.Log(() => "--" + outerSpan.ToString());
            Span anchor = repeaters.FindWithin(outerSpan, pointer, options);
            return anchor;
        }

        public static Span FindWithin(this IList<IRepeater> tags, Span span,
                                      Pointer.Type pointer, Options options)
        {
            Logger.Log(() => "--" + span.ToString());
            if (tags.Count == 0)
            {
                return span;
            }
            var head = tags[0];
            Logger.Log(() => "#" + head.ToString());
            var rest = tags.Skip(1).ToList();
            head.Now = (pointer == Pointer.Type.Future ? span.Start : span.End);
            var h = head.GetCurrentSpan(Pointer.Type.None);

            if (span.Contains(h.Start) || span.Contains(h.End))
            {
                return FindWithin(rest, h, pointer, options);
            }
            return null;
        }
    }
}