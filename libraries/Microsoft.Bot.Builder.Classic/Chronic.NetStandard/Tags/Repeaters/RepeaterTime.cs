using System;
using System.Linq;
using Chronic;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterTime : Repeater<Tick>
    {
        DateTime? _currentTime;
        const int SecondsInHour = 60 * 60;

        public RepeaterTime(string value)
            : base(null)
        {
            var t = value.Replace(":", "");
            Tick tick;
            if (t.Length <= 2)
            {
                var hours = int.Parse(t);
                tick = new Tick((hours == 12 ? 0 : hours) * SecondsInHour, true);
            }
            else if (t.Length == 3)
            {
                int hoursInSeconds = int.Parse(t.Substring(0, 1)) * SecondsInHour;
                int minutesInSeconds = int.Parse(t.Substring(1)) * 60;
                tick = new Tick(hoursInSeconds + minutesInSeconds, true);
            }
            else if (t.Length == 4)
            {
                var ambiguous = (value.Contains(":") &&
                                    int.Parse(t.Substring(0, 1)) != 0 &&
                                        int.Parse(t.Substring(0, 2)) <= 12);
                int hours = int.Parse(t.Substring(0, 2));
                int hoursInSeconds = hours * 60 * 60;
                int minutesInSeconds = int.Parse(t.Substring(2)) * 60;
                if (hours == 12)
                {
                    tick = new Tick(0 * 60 * 60 + minutesInSeconds, ambiguous);
                }
                else
                {
                    tick = new Tick(hoursInSeconds + minutesInSeconds, ambiguous);
                }
            }
            else if (t.Length == 5)
            {
                int hoursInSeconds = int.Parse(t.Substring(0, 1)) * 60 * 60;
                int minutesInSeconds = int.Parse(t.Substring(1, 2)) * 60;
                int seconds = int.Parse(t.Substring(3));
                tick = new Tick(hoursInSeconds + minutesInSeconds + seconds,
                                true);
            }
            else if (t.Length == 6)
            {
                bool ambiguous = (value.Contains(":") &&
                    int.Parse(t.Substring(0, 1)) != 0 &&
                        int.Parse(t.Substring(0, 2)) <= 12);
                int hours = int.Parse(t.Substring(0, 2));
                int hoursInSeconds = hours * 60 * 60;
                int minutesInSeconds = int.Parse(t.Substring(2, 2)) * 60;
                int seconds = int.Parse(t.Substring(4, 2));
                //type = new Tick(hoursInSeconds + minutesInSeconds + seconds, ambiguous);
                if (hours == 12)
                {
                    tick = new Tick(0 * 60 * 60 + minutesInSeconds + seconds,
                                    ambiguous);
                }
                else
                {
                    tick = new Tick(
                        hoursInSeconds + minutesInSeconds + seconds, ambiguous);
                }
            }
            else
            {
                throw new ArgumentException("Time cannot exceed six digits");
            }
            Value = tick;
        }


        public override string ToString()
        {
            return base.ToString() + "-time-" + Value;
        }

        public override int GetWidth()
        {
            return 1;
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            var halfDay = RepeaterDay.DAY_SECONDS / 2;
            var fullDay = RepeaterDay.DAY_SECONDS;

            var now = Now.Value;
            var tick = Value;
            var first = false;

            if (_currentTime == null)
            {
                first = true;
                var midnight = now.Date;
                var yesterdayMidnight = midnight.AddDays(-1);
                var tomorrowMidnight = midnight.AddDays(1);

                var dstFix = (midnight - midnight.ToUniversalTime()).Seconds -
                    (tomorrowMidnight - tomorrowMidnight.ToUniversalTime()).Seconds;

                var done = false;
                DateTime[] candidateDates = null;

                if (pointer == Pointer.Type.Future)
                {
                    candidateDates = tick.IsAmbiguous
                        ? new DateTime[]
                            {
                                midnight.AddSeconds(tick.ToInt32() + dstFix),
                                midnight.AddSeconds(tick.ToInt32() + halfDay + dstFix),
                                tomorrowMidnight.AddSeconds(tick.ToInt32())
                            }
                        : new DateTime[]
                            {
                                midnight.AddSeconds(tick.ToInt32() + dstFix),
                                tomorrowMidnight.AddSeconds(tick.ToInt32())
                            };

                    foreach (var date in candidateDates)
                    {
                        if (date >= now)
                        {
                            _currentTime = date;
                            done = true;
                            break;
                        }
                    }
                }
                else
                {
                    candidateDates = tick.IsAmbiguous
                        ? new DateTime[]
                            {
                                midnight.AddSeconds(tick.ToInt32() + halfDay + dstFix),
                                midnight.AddSeconds(tick.ToInt32() + dstFix),
                                yesterdayMidnight.AddSeconds(tick.ToInt32() + halfDay)
                            }
                        : new DateTime[]
                            {
                                midnight.AddSeconds(tick.ToInt32() + dstFix),
                                yesterdayMidnight.AddSeconds(tick.ToInt32())
                            };

                    foreach (var date in candidateDates)
                    {
                        if (date <= now)
                        {
                            _currentTime = date;
                            done = true;
                            break;
                        }
                    }
                }


                if (!done && _currentTime == null)
                {
                    throw new IllegalStateException(
                        "Current time cannot be null at this point.");
                }
            }

            if (!first)
            {
                var increment = tick.IsAmbiguous ? halfDay : fullDay;
                var direction = (int)pointer;
                _currentTime = _currentTime.Value.AddSeconds(direction * increment);
            }

            return new Span(_currentTime.Value,
                            _currentTime.Value.AddSeconds(GetWidth()));
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            if (pointer == Pointer.Type.None)
            {
                pointer = Pointer.Type.Future;
            }
            return NextSpan(pointer);
        }

        public override Span GetOffset(Span span, int amount,
                                       Pointer.Type pointer)
        {
            throw new NotImplementedException();
        }
    }
}