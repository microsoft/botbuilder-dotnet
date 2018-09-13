using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterScanner : ITokenScanner
    {
        static readonly List<Func<Token, Options, ITag>> _scanners = new List
            <Func<Token, Options, ITag>>
            {
                //ScanSeasonNames(token, options),
                ScanMonthNames,
                ScanDayNames,
                ScanDayPortions,
                ScanTimes,
                ScanUnits,
            };

        public IList<Token> Scan(IList<Token> tokens, Options options)
        {
            tokens.ForEach(token =>
                {
                    foreach (var scanner in _scanners)
                    {
                        var tag = scanner(token, options);
                        if (tag != null)
                        {
                            token.Tag(tag);
                            break;
                        }
                    }
                });

            return tokens;
        }

        static ITag ScanUnits(Token token, Options options)
        {
            ITag tag = null;
            UnitPatterns.ForEach(item =>
            {
                if (item.Pattern.IsMatch(token.Value))
                {
                    var type = (Type)item.Unit;
                    var hasCtorWithOptions = type.GetConstructors().Any(ctor =>
                    {
                        var parameters = ctor.GetParameters().ToArray();
                        return
                            parameters.Length == 1
                            && parameters.First().ParameterType == typeof(Options);
                    });
                    var ctorParameters = hasCtorWithOptions
                        ? new[] { options }
                        : new object[0];

                    tag = Activator.CreateInstance(
                        type,
                        ctorParameters) as ITag;

                    return;
                }
            });
            return tag;
        }

        static ITag ScanTimes(Token token, Options options)
        {
            var match = _timePattern.Match(token.Value);
            if (match.Success)
            {
                return new RepeaterTime(match.Value);
            }
            return null;
        }

        static ITag ScanDayPortions(Token token, Options options)
        {
            ITag tag = null;
            DayPortionPatterns.ForEach(item =>
                {
                    if (item.Pattern.IsMatch(token.Value))
                    {
                        tag = new EnumRepeaterDayPortion(item.Portion);
                        return;
                    }
                });
            return tag;
        }

        static ITag ScanDayNames(Token token, Options options)
        {
            ITag tag = null;
            DayPatterns.ForEach(item =>
                {
                    if (item.Pattern.IsMatch(token.Value))
                    {
                        tag = new RepeaterDayName(item.Day);
                        return;
                    }
                });
            return tag;
        }

        static ITag ScanMonthNames(Token token, Options options)
        {
            ITag tag = null;
            MonthPatterns.ForEach(item =>
                {
                    if (item.Pattern.IsMatch(token.Value))
                    {
                        tag = new RepeaterMonthName(item.Month);
                        return;
                    }
                });
            return tag;
        }

        static ITag ScanSeasonNames(Token token, Options options)
        {
            throw new NotImplementedException();
        }

        static readonly Regex _timePattern =
            @"^\d{1,2}(:?\d{2})?([\.:]?\d{2})?$".Compile();

        static readonly List<PatternDayPortion> DayPortionPatterns = new List<PatternDayPortion>
            {
                new PatternDayPortion() {Pattern = "^ams?$".Compile(), Portion = DayPortion.AM},
                new PatternDayPortion()  {Pattern = "^pms?$".Compile(), Portion = DayPortion.PM},
                new PatternDayPortion()  {Pattern = "^mornings?$".Compile(), Portion = DayPortion.MORNING},
                new PatternDayPortion()  {Pattern = "^afternoons?$".Compile(), Portion = DayPortion.AFTERNOON},
                new PatternDayPortion()  {Pattern = "^evenings?$".Compile(), Portion = DayPortion.EVENING},
                new PatternDayPortion()  {Pattern = "^(night|nite)s?$".Compile(), Portion = DayPortion.NIGHT},
            };
        public class PatternDayPortion
        {
            public Regex Pattern { get; set; }
            public DayPortion Portion { get; set; }
        }
        static readonly List<PatternDay> DayPatterns = new List<PatternDay>
            {
                new PatternDay() {Pattern ="^m[ou]n(day)?$".Compile(), Day = DayOfWeek.Monday},
                new PatternDay() {Pattern = "^t(ue|eu|oo|u|)s(day)?$".Compile(), Day = DayOfWeek.Tuesday},
                new PatternDay() {Pattern = "^tue$".Compile(), Day = DayOfWeek.Tuesday},
                new PatternDay() {Pattern = "^we(dnes|nds|nns)day$".Compile(), Day = DayOfWeek.Wednesday},
                new PatternDay() {Pattern = "^wed$".Compile(), Day = DayOfWeek.Wednesday},
                new PatternDay() {Pattern = "^th(urs|ers)day$".Compile(), Day = DayOfWeek.Thursday},
                new PatternDay() {Pattern = "^thu$".Compile(), Day = DayOfWeek.Thursday},
                new PatternDay() {Pattern = "^fr[iy](day)?$".Compile(), Day = DayOfWeek.Friday},
                new PatternDay() {Pattern = "^sat(t?[ue]rday)?$".Compile(), Day = DayOfWeek.Saturday},
                new PatternDay() {Pattern = "^su[nm](day)?$".Compile(), Day = DayOfWeek.Sunday},
            };
        public class PatternDay
        {
            public Regex Pattern { get; set; }
            public DayOfWeek Day { get; set; }
        }
        static readonly List<PatternMonth> MonthPatterns = new List<PatternMonth>
            {
                new PatternMonth() {Pattern = "^jan\\.?(uary)?$".Compile(), Month = MonthName.January},
                new PatternMonth()  {Pattern = "^feb\\.?(ruary)?$".Compile(), Month = MonthName.February},
                new PatternMonth()  {Pattern = "^mar\\.?(ch)?$".Compile(), Month = MonthName.March},
                new PatternMonth()  {Pattern = "^apr\\.?(il)?$".Compile(), Month = MonthName.April},
                new PatternMonth()  {Pattern = "^may$".Compile(), Month = MonthName.May},
                new PatternMonth()  {Pattern = "^jun\\.?e?$".Compile(), Month = MonthName.June},
                new PatternMonth()  {Pattern = "^jul\\.?y?$".Compile(), Month = MonthName.July},
                new PatternMonth()  {Pattern = "^aug\\.?(ust)?$".Compile(), Month = MonthName.August},
                new PatternMonth()
                    {
                        Pattern = "^sep\\.?(t\\.?|tember)?$".Compile(),
                        Month = MonthName.September
                    },
                new PatternMonth()  {Pattern = "^oct\\.?(ober)?$".Compile(), Month = MonthName.October},
                new PatternMonth()  {Pattern = "^nov\\.?(ember)?$".Compile(), Month = MonthName.November},
                new PatternMonth()  {Pattern = "^dec\\.?(ember)?$".Compile(), Month = MonthName.December},
            };

        public class PatternMonth
        {
            public Regex Pattern { get; set; }
            public MonthName Month { get; set; }
        }
        static readonly List<PatternUnit> UnitPatterns = new List<PatternUnit>
            {
                new PatternUnit() { Pattern = "^years?$".Compile(), Unit = typeof(RepeaterYear) },
                new PatternUnit() { Pattern = "^seasons?$".Compile(), Unit = typeof(RepeaterSeason) },
                new PatternUnit() { Pattern = "^months?$".Compile(), Unit = typeof(RepeaterMonth) },
                new PatternUnit() { Pattern = "^fortnights?$".Compile(), Unit = typeof(RepeaterFortnight) },
                new PatternUnit() { Pattern = "^weeks?$".Compile(), Unit = typeof(RepeaterWeek) },
                new PatternUnit() { Pattern = "^weekends?$".Compile(), Unit = typeof(RepeaterWeekend) },
                new PatternUnit() { Pattern = "^days?$".Compile(), Unit = typeof(RepeaterDay) },
                new PatternUnit() { Pattern = "^hours?$".Compile(), Unit = typeof(RepeaterHour) },
                new PatternUnit() { Pattern = "^minutes?$".Compile(), Unit = typeof(RepeaterMinute) },
                new PatternUnit() { Pattern = "^seconds?$".Compile(), Unit = typeof(RepeaterSecond) }
            };

        public class PatternUnit
        {
            public Regex Pattern { get; set; }
            public Type Unit { get; set; }
        }
    }
}