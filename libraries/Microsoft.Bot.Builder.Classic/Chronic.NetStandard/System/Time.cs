using System;

namespace Chronic
{
    public class Time
    {
        public static readonly int[] DaysInMonthInLeapYears = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        public static readonly int[] DaysInMonthInRegularYears = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public static DateTime New(int year, int month, int day, TimeSpan time)
        {
            return New(year, month, day).Add(time);
        }

        public static DateTime New(DateTime date, int hour = 0, int minute = 0, int second = 0)
        {
            return New(date.Year, date.Month, date.Day, hour, minute, second);
        }

        public static DateTime New(int year, int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0)
        {
            if (second >= 60)
            {
                minute += second / 60;
                second = second % 60;
            }

            if (minute >= 60)
            {
                hour += minute / 60;
                minute = minute % 60;
            }

            if (hour >= 24)
            {
                day += hour / 24;
                hour = hour % 24;
            }

            // determine if there is a day overflow. this is complicated by our crappy calendar
            // system (non-constant number of days per month)
            if (day > 56)
                throw new Exception(
                    "day must be no more than 56 (makes month resolution easier)");

            var days_this_month = DateTime.IsLeapYear(year)
                ? DaysInMonthInLeapYears[month - 1]
                : DaysInMonthInRegularYears[month - 1];

            if (day > days_this_month)
            {
                month += day / days_this_month;
                day = day % days_this_month;
            }


            if (month > 12)
            {
                if (month % 12 == 0)
                {
                    year += (month - 12) / 12;
                    month = 12;
                }
                else
                {
                    year += month / 12;
                    month = month % 12;
                }

            }
            return new DateTime(year, month, day, hour, minute, second);
        }

        public static bool IsMonthOverflow(int year, int month, int day)
        {
            if (month > 12 || month < 1)
            {
                throw new ArgumentException(String.Format(
                    "Expected month between 1 and 12 but got {0}", month),
                    "month");
            }

            return day > DaysInMonth(year, month);
        }

        public static int DaysInMonth(int year, int month)
        {
            return DateTime.IsLeapYear(year)
                ? DaysInMonthInLeapYears[month - 1]
                : DaysInMonthInRegularYears[month - 1];
        }
    }
}
