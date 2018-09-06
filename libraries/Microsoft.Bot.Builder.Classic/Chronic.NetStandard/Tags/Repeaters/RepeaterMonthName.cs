using System;
using Chronic;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterMonthName : Repeater<MonthName>
    {
        const int SecondsInMonth = 2592000; // 30 days * 24 h * 60 min * 60 sec

        DateTime? _currentMonthBegin;

        public RepeaterMonthName(MonthName type)
            : base(type)
        {
        }

        public int GetMonthIndex()
        {
            return (int)Value;
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            var now = Now.Value;

            if (_currentMonthBegin == null)
            {
                var index = GetMonthIndex();
                
                if (pointer == Pointer.Type.Future)
                {
                    if (now.Month < index)
                    {
                        _currentMonthBegin = Time.New(Now.Value.Year, index);
                    }
                    else if (now.Month > index)
                    {
                        _currentMonthBegin = Time.New(Now.Value.Year + 1, index);
                    }
                }
                else if (pointer == Pointer.Type.None)
                {
                    if (now.Month <= index)
                    {
                        _currentMonthBegin = Time.New(Now.Value.Year, index);
                    }
                    else if (now.Month > index)
                    {
                        _currentMonthBegin = Time.New(Now.Value.Year + 1, index);
                    }
                }
                else if (pointer == Pointer.Type.Past)
                {
                    if (now.Month >= index)
                    {
                        _currentMonthBegin = Time.New(Now.Value.Year, index);
                    }
                    else if (now.Month < index)
                    {
                        _currentMonthBegin = Time.New(Now.Value.Year - 1, index);
                    }
                }
                else
                {
                    throw new ArgumentException(
                        "Unable to handle pointer " + pointer + ".");
                }
                if (_currentMonthBegin == null)
                {
                    throw new IllegalStateException(
                        "Current month should be set by now.");
                }
            }
            else
            {
                if (pointer == Pointer.Type.Future)
                {
                    _currentMonthBegin = Time.New(_currentMonthBegin.Value.Year + 1, _currentMonthBegin.Value.Month);
                }
                else if (pointer == Pointer.Type.Past)
                {
                    _currentMonthBegin = Time.New(_currentMonthBegin.Value.Year - 1, _currentMonthBegin.Value.Month);
                }
                else
                {
                    throw new ArgumentException(
                        "Unable to handle pointer " + pointer + ".");
                }
            }

            var cur_month_year = _currentMonthBegin.Value.Year;
            var cur_month_month = _currentMonthBegin.Value.Month;
            var next_month_year = 0;
            var next_month_month = 0;

            if (cur_month_month == 12)
            {
                next_month_year = cur_month_year + 1;
                next_month_month = 1;
            }
            else
            {
                next_month_year = cur_month_year;
                next_month_month = cur_month_month + 1;
            }

            return new Span(_currentMonthBegin.Value,
                            Time.New(next_month_year, next_month_month));
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            Span span;
            if (pointer == Pointer.Type.Past)
            {
                span = GetNextSpan(pointer);
            }
            else if (pointer == Pointer.Type.Future ||
                pointer == Pointer.Type.None)
            {
                span = GetNextSpan(Pointer.Type.None);
            }
            else
            {
                throw new ArgumentException(
                    "Unable to handle pointer " + pointer + ".");
            }
            return span;
        }

        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            throw new IllegalStateException("Not implemented.");
        }

        public override int GetWidth()
        {
            // WARN: Does not use Calendar
            return RepeaterMonthName.SecondsInMonth;
        }

        public override string ToString()
        {
            return base.ToString() + "-monthname-" + Value;
        }
    }
}