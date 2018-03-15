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
using System.Linq;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Builder.Classic.Luis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.Bot.Builder.Classic.Luis.BuiltIn.DateTime;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public sealed class LuisResolutionTests
    {
        struct TestCase
        {
            public string Text { get; set; }
            public DateTimeResolution Resolution { get; set; }
            public Range<DateTime>[] Ranges { get; set; }
        }

        public static readonly DateTime Now = new DateTime(2013, 08, 04);
        public static readonly ICalendarPlus Calendar = new WesternCalendarPlus();

        private static readonly IReadOnlyList<TestCase> TestCases = new[]
        {
            // https://github.com/Microsoft/BotBuilder/issues/2088
            new TestCase()
            {
                Text = "2020-W01",
                Resolution = new DateTimeResolution(year: 2020, week: 1),
                Ranges = new []
                {
                    Range.From(new DateTime(2020, 01, 01), new DateTime(2020, 01, 08)),
                }
            },
            new TestCase()
            {
                Text = "2019-W52",
                Resolution = new DateTimeResolution(year: 2019, week: 52),
                Ranges = new []
                {
                    Range.From(new DateTime(2019, 12, 22), new DateTime(2019, 12, 29)),
                }
            },
            new TestCase()
            {
                Text = "2018-W01-1",
                Resolution = new DateTimeResolution(year: 2018, week: 1, dayOfWeek: DayOfWeek.Monday),
                Ranges = new []
                {
                    Range.From(new DateTime(2018, 1, 1), new DateTime(2018, 1, 2)),
                }
            },
            new TestCase()
            {
                Text = "2018-01",
                Resolution = new DateTimeResolution(year: 2018, month: 1),
                Ranges = new []
                {
                    Range.From(new DateTime(2018, 1, 1), new DateTime(2018, 2, 1)),
                }
            },
            new TestCase()
            {
                Text = "2018-01-01",
                Resolution = new DateTimeResolution(year: 2018, month: 1, day: 1),
                Ranges = new []
                {
                    Range.From(new DateTime(2018, 1, 1), new DateTime(2018, 1, 2)),
                }
            },
            // examples from https://www.luis.ai/Help/#PreBuiltEntities
            new TestCase()
            {
                Text = "2015-08-15",
                Resolution = new DateTimeResolution(year: 2015, month: 8, day: 15),
                Ranges = new []
                {
                    Range.From(new DateTime(2015, 08, 15), new DateTime(2015, 08, 16)),
                }
            },
            new TestCase()
            {
                Text = "XXXX-WXX-1",
                Resolution = new DateTimeResolution(year: -1, week: -1, dayOfWeek: DayOfWeek.Monday),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 08, 05), new DateTime(2013, 08, 06)),
                    Range.From(new DateTime(2013, 08, 12), new DateTime(2013, 08, 13)),
                    Range.From(new DateTime(2013, 08, 19), new DateTime(2013, 08, 20)),
                    Range.From(new DateTime(2013, 08, 26), new DateTime(2013, 08, 27)),
                    Range.From(new DateTime(2013, 09, 02), new DateTime(2013, 09, 03)),
                }
            },
            new TestCase()
            {
                Text = "2015-W34",
                Resolution = new DateTimeResolution(year: 2015, week: 34),
                Ranges = new []
                {
                    Range.From(new DateTime(2015, 08, 16), new DateTime(2015, 08, 23)),
                }
            },
            // TODO: need to look at comment = "weekof"
            new TestCase()
            {
                Text = "XXXX-09-30",
                Resolution = new DateTimeResolution(year: -1, month: 9, day: 30),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 09, 30), new DateTime(2013, 10, 01)),
                    Range.From(new DateTime(2014, 09, 30), new DateTime(2014, 10, 01)),
                    Range.From(new DateTime(2015, 09, 30), new DateTime(2015, 10, 01)),
                    Range.From(new DateTime(2016, 09, 30), new DateTime(2016, 10, 01)),
                    Range.From(new DateTime(2017, 09, 30), new DateTime(2017, 10, 01)),
                }
            },
            // TODO: need to look at comment = "ampm"
            new TestCase()
            {
                Text = "T03:00",
                Resolution = new DateTimeResolution(hour: 3, minute: 0),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 08, 04, 03, 00, 00), new DateTime(2013, 08, 04, 03, 01, 00)),
                    Range.From(new DateTime(2013, 08, 05, 03, 00, 00), new DateTime(2013, 08, 05, 03, 01, 00)),
                    Range.From(new DateTime(2013, 08, 06, 03, 00, 00), new DateTime(2013, 08, 06, 03, 01, 00)),
                    Range.From(new DateTime(2013, 08, 07, 03, 00, 00), new DateTime(2013, 08, 07, 03, 01, 00)),
                    Range.From(new DateTime(2013, 08, 08, 03, 00, 00), new DateTime(2013, 08, 08, 03, 01, 00)),
                }
            },
            new TestCase()
            {
                Text = "T16",
                Resolution = new DateTimeResolution(hour: 16),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 08, 04, 16, 00, 00), new DateTime(2013, 08, 04, 17, 00, 00)),
                    Range.From(new DateTime(2013, 08, 05, 16, 00, 00), new DateTime(2013, 08, 05, 17, 00, 00)),
                    Range.From(new DateTime(2013, 08, 06, 16, 00, 00), new DateTime(2013, 08, 06, 17, 00, 00)),
                    Range.From(new DateTime(2013, 08, 07, 16, 00, 00), new DateTime(2013, 08, 07, 17, 00, 00)),
                    Range.From(new DateTime(2013, 08, 08, 16, 00, 00), new DateTime(2013, 08, 08, 17, 00, 00)),
                }
            },
            new TestCase()
            {
                Text = "2015-08-15TMO",
                Resolution = new DateTimeResolution(year: 2015, month: 8, day: 15, dayPart: DayPart.MO),
                Ranges = new []
                {
                    Range.From(new DateTime(2015, 08, 15, 09, 00, 00), new DateTime(2015, 08, 15, 12, 00, 00)),
                }
            },
            new TestCase()
            {
                Text = "2015-08-14TNI",
                Resolution = new DateTimeResolution(year: 2015, month: 8, day: 14, dayPart: DayPart.NI),
                Ranges = new []
                {
                    Range.From(new DateTime(2015, 08, 14, 21, 00, 00), new DateTime(2015, 08, 15, 09, 00, 00)),
                }
            },
            // other examples based on poking the service
            // https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=752a2d86f21e47879c8e3ae88ca4c009&q=set%20an%20alarm%20for%20every%20monday%20at%209%20am
            new TestCase()
            {
                Text = "TMO",
                Resolution = new DateTimeResolution(dayPart: DayPart.MO),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 08, 04, 09, 00, 00), new DateTime(2013, 08, 04, 12, 00, 00)),
                    Range.From(new DateTime(2013, 08, 05, 09, 00, 00), new DateTime(2013, 08, 05, 12, 00, 00)),
                    Range.From(new DateTime(2013, 08, 06, 09, 00, 00), new DateTime(2013, 08, 06, 12, 00, 00)),
                    Range.From(new DateTime(2013, 08, 07, 09, 00, 00), new DateTime(2013, 08, 07, 12, 00, 00)),
                    Range.From(new DateTime(2013, 08, 08, 09, 00, 00), new DateTime(2013, 08, 08, 12, 00, 00)),
                }
            },
            new TestCase()
            {
                Text = "XXXX-09-W02",
                Resolution = new DateTimeResolution(year: -1, month: 9, dayOfWeek: DayOfWeek.Tuesday),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 09, 03), new DateTime(2013, 09, 04)),
                    Range.From(new DateTime(2013, 09, 10), new DateTime(2013, 09, 11)),
                    Range.From(new DateTime(2013, 09, 17), new DateTime(2013, 09, 18)),
                    Range.From(new DateTime(2013, 09, 24), new DateTime(2013, 09, 25)),
                    Range.From(new DateTime(2014, 09, 02), new DateTime(2014, 09, 03)),
                }
            },
            new TestCase()
            {
                Text = "2018",
                Resolution = new DateTimeResolution(year: 2018),
                Ranges = new []
                {
                    Range.From(new DateTime(2018, 01, 01), new DateTime(2019, 01, 01)),
                }
            },
            new TestCase()
            {
                Text = "XXXX-01",
                Resolution = new DateTimeResolution(year: -1, month: 1),
                Ranges = new []
                {
                    Range.From(new DateTime(2014, 01, 01), new DateTime(2014, 02, 01)),
                    Range.From(new DateTime(2015, 01, 01), new DateTime(2015, 02, 01)),
                    Range.From(new DateTime(2016, 01, 01), new DateTime(2016, 02, 01)),
                    Range.From(new DateTime(2017, 01, 01), new DateTime(2017, 02, 01)),
                    Range.From(new DateTime(2018, 01, 01), new DateTime(2018, 02, 01)),
                }
            },
            new TestCase()
            {
                Text = "XXXX-XX-01",
                Resolution = new DateTimeResolution(year: -1, month: -1, day: 1),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 09, 01), new DateTime(2013, 09, 02)),
                    Range.From(new DateTime(2013, 10, 01), new DateTime(2013, 10, 02)),
                    Range.From(new DateTime(2013, 11, 01), new DateTime(2013, 11, 02)),
                    Range.From(new DateTime(2013, 12, 01), new DateTime(2013, 12, 02)),
                    Range.From(new DateTime(2014, 01, 01), new DateTime(2014, 01, 02)),
                }
            },
            // https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=752a2d86f21e47879c8e3ae88ca4c009&q=set%20an%20alarm%20for%20every%20monday%20at%209%20am
            new TestCase()
            {
                Text = "XXXX-WXX-1T09",
                Resolution = new DateTimeResolution(year: -1, week: -1, dayOfWeek: DayOfWeek.Monday, hour: 9),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 08, 05, 09, 00, 00), new DateTime(2013, 08, 05, 10, 00, 00)),
                    Range.From(new DateTime(2013, 08, 12, 09, 00, 00), new DateTime(2013, 08, 12, 10, 00, 00)),
                    Range.From(new DateTime(2013, 08, 19, 09, 00, 00), new DateTime(2013, 08, 19, 10, 00, 00)),
                    Range.From(new DateTime(2013, 08, 26, 09, 00, 00), new DateTime(2013, 08, 26, 10, 00, 00)),
                    Range.From(new DateTime(2013, 09, 02, 09, 00, 00), new DateTime(2013, 09, 02, 10, 00, 00)),
                }
            },
            // https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=752a2d86f21e47879c8e3ae88ca4c009&q=set%20an%20alarm%20for%20every%20day
            new TestCase()
            {
                Text = "XXXX-XX-XX",
                Resolution = new DateTimeResolution(year: -1, month: -1, day: -1),
                Ranges = new []
                {
                    Range.From(new DateTime(2013, 08, 04), new DateTime(2013, 08, 05)),
                    Range.From(new DateTime(2013, 08, 05), new DateTime(2013, 08, 06)),
                    Range.From(new DateTime(2013, 08, 06), new DateTime(2013, 08, 07)),
                    Range.From(new DateTime(2013, 08, 07), new DateTime(2013, 08, 08)),
                    Range.From(new DateTime(2013, 08, 08), new DateTime(2013, 08, 09)),
                }
            },
            new TestCase()
            {
                Text = "PAST_REF",
                Resolution = new DateTimeResolution(reference: Reference.PAST_REF),
                Ranges = new []
                {
                    Range.From(DateTime.MinValue, Now),
                }
            },
            new TestCase()
            {
                Text = "PRESENT_REF",
                Resolution = new DateTimeResolution(reference: Reference.PRESENT_REF),
                Ranges = new []
                {
                    Range.From(Now, Now),
                }
            },
            new TestCase()
            {
                Text = "FUTURE_REF",
                Resolution = new DateTimeResolution(reference: Reference.FUTURE_REF),
                Ranges = new []
                {
                    Range.From(Now, DateTime.MaxValue),
                }
            },
        };

        [TestMethod]
        public void Luis_Resolution_DateTime_Parse()
        {
            foreach (var test in TestCases)
            {
                DateTimeResolution actual;
                Assert.IsTrue(DateTimeResolution.TryParse(test.Text, out actual), test.Text);
                Assert.AreEqual(test.Resolution, actual, test.Text);
            }
        }

        [TestMethod]
        public void Luis_Resolution_DateTime_Interpret()
        {
            foreach (var test in TestCases)
            {
                var interpretation = StrictEntityToType.Interpret(test.Resolution, Now, Calendar.Calendar, Calendar.WeekRule, Calendar.FirstDayOfWeek, Calendar.HourFor);
                var head = interpretation.Take(5).ToArray();
                CollectionAssert.AreEqual(head, test.Ranges, test.Text);
            };
        }
    }
}
