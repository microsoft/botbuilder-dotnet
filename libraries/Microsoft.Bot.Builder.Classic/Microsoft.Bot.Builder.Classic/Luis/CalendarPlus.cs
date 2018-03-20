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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Classic.Luis.BuiltIn.DateTime;

namespace Microsoft.Bot.Builder.Classic.Luis
{
    /// <summary>
    /// Policy for interpreting LUIS resolutions.
    /// </summary>
    public interface ICalendarPlus
    {
        Calendar Calendar { get; }
        CalendarWeekRule WeekRule { get; }
        DayOfWeek FirstDayOfWeek { get; }
        int HourFor(DayPart dayPart);
    }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Gregorian_calendar
    /// </summary>
    public sealed class WesternCalendarPlus : ICalendarPlus
    {
        Calendar ICalendarPlus.Calendar => CultureInfo.InvariantCulture.Calendar;

        DayOfWeek ICalendarPlus.FirstDayOfWeek => DayOfWeek.Sunday;

        CalendarWeekRule ICalendarPlus.WeekRule => CalendarWeekRule.FirstDay;

        int ICalendarPlus.HourFor(DayPart dayPart)
        {
            switch (dayPart)
            {
                case DayPart.MO: return 9;
                case DayPart.MI: return 12;
                case DayPart.AF: return 15;
                case DayPart.EV: return 18;
                case DayPart.NI: return 21;
                default: throw new NotImplementedException();
            }
        }
    }
}
