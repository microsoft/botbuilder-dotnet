using System;
using Chronic;
using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterSeason : RepeaterUnit
    {
        public static readonly int SEASON_SECONDS = 7862400; // (91 * 24 * 60 * 60);

        public RepeaterSeason() : base(UnitName.Season)
        {
            
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            throw new IllegalStateException("Not implemented.");
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            throw new NotImplementedException();
        }

        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            throw new NotImplementedException();
        }

        public override int GetWidth()
        {
            // WARN: Does not use Calendar
            return RepeaterSeason.SEASON_SECONDS;
        }


        public override string ToString()
        {
            return base.ToString() + "-season";
        }

    }
}