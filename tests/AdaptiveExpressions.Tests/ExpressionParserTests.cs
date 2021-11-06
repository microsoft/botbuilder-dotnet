﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1124 // Do not use regions

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AdaptiveExpressions.Tests
{
    public class ExpressionParserTests
    {
        private static readonly string NullStr = null;

        private readonly object scope = new Dictionary<string, object>
        {
            {
                "jsonContainsDatetime", "{\"date\": \"/Date(634250351766060665)/\", \"invalidDate\": \"/Date(whatever)/\"}"
            },
            { "$index", "index" },
            {
                "alist", new List<A>() { new A("item1"), new A("item2") }
            },
            {
                "a:b", "stringa:b"
            },
            {
                "emptyList", new List<object>()
            },
            {
                "emptyObject", new Dictionary<string, object>()
            },
            {
                "emptyJObject", new JObject()
            },
            {
                "emptyAnonymousObject", new { }
            },
            {
                "path", new Dictionary<string, object>()
                {
                    {
                        "array", new List<int>() { 1 }
                    }
                }
            },
            { "one", 1.0 },
            { "two", 2.0 },
            { "hello", "hello" },
            { "world", "world" },
            { "newExpr", "new land" },
            { "cit", "cit" },
            { "y", "y" },
            { "istrue", true },
            { "nullObj", NullStr },
            {
                "bag", new Dictionary<string, object>
                {
                    { "three", 3.0 },
                    { "set", new { four = 4.0 } },
                    { "list", new[] { "red", "blue" } },
                    { "index", 3 },
                    { "name", "mybag" }
                }
            },
            { "items", new string[] { "zero", "one", "two" } },
            {
                "nestedItems",
                new[]
                {
                    new { x = 1 },
                    new { x = 2 },
                    new { x = 3 }
                }
            },
            {
                "user",
                new
                {
                    income = 100.1,
                    outcome = 120.1,
                    nickname = "John",
                    lists = new
                    {
                        todo = new[]
                        {
                            "todo1",
                            "todo2",
                            "todo3",
                        }
                    },
                    listType = "todo",
                }
            },
            { "byteArr", new byte[] { 3, 5, 1, 12 } },
            { "timestamp", "2018-03-15T13:00:00.000Z" },
            { "notISOTimestamp", "2018/03/15 13:00:00" },
            { "validFullDateTimex", new TimexProperty("2020-02-20") },
            { "invalidFullDateTimex", new TimexProperty("xxxx-02-20") },
            { "validHourTimex", new TimexProperty("2020-02-20T07:30") },
            { "validTimeRange", new TimexProperty() { PartOfDay = "morning" } },
            { "validNow", new TimexProperty() { Now = true } },
            { "invalidHourTimex", new TimexProperty("2001-02-20") },
            { "timestampObj", DateTime.Parse("2018-03-15T13:00:00.000Z").ToUniversalTime() },
            { "timestampObj2", DateTime.Parse("2018-01-02T02:00:00.000Z").ToUniversalTime() },
            { "timestampObj3", DateTime.Parse("2018-01-01T08:00:00.000Z").ToUniversalTime() },
            { "unixTimestamp", 1521118800 },
            { "unixTimestampFraction", 1521118800.5 },
            { "ticks", 637243624200000000 },
            {
                "json1", @"{
                          'FirstName': 'John',
                          'LastName': 'Smith',
                          'Enabled': false,
                          'Roles': [ 'User' ]
                        }"
            },
            {
                "json2", @"{
                          'Enabled': true,
                          'Roles': [ 'Customer', 'Admin' ]
                        }"
            },
            {
                "json3", @"{
                          'Age': 36,
                        }"
            },
            { "xmlStr", "<?xml version='1.0'?> <produce> <item> <name>Gala</name> <type>apple</type> <count>20</count> </item> <item> <name>Honeycrisp</name> <type>apple</type> <count>10</count> </item> </produce>" },
            {
                "jsonStr", @"{
                          'Stores': [
                            'Lambton Quay',
                            'Willis Street'
                          ],
                          'Manufacturers': [
                            {
                              'Name': 'Acme Co',
                              'Products': [
                                {
                                  'Name': 'Anvil',
                                  'Price': 50
                                }
                              ]
                            },
                            {
                              'Name': 'Contoso',
                              'Products': [
                                {
                                  'Name': 'Elbow Grease',
                                  'Price': 99.95
                                },
                                {
                                  'Name': 'Headlight Fluid',
                                  'Price': 4
                                }
                              ]
                            }
                          ]
                        }"
            },
            {
                "turn", new
                {
                    recognized = new
                    {
                        entities = new Dictionary<string, object>
                        {
                            {
                                "city",
                                new[]
                                {
                                    "Seattle"
                                }
                            },
                            {
                                "ordinal",
                                new[]
                                {
                                    "1",
                                    "2",
                                    "3"
                                }
                            },
                            {
                                "CompositeList1",
                                new[]
                                {
                                    new[]
                                    {
                                        "firstItem"
                                    }
                                }
                            },
                            {
                                "CompositeList2",
                                new[]
                                {
                                    new[]
                                    {
                                        "firstItem",
                                        "secondItem"
                                    }
                                }
                            }
                        },
                        intents = new
                        {
                            BookFlight = "BookFlight",
                            BookHotel = new[]
                            {
                                new
                                {
                                    Where = "Bellevue",
                                    Time = "Tomorrow",
                                    People= "2"
                                },
                                new
                                {
                                    Where = "Kirkland",
                                    Time = "Today",
                                    People = "4"
                                }
                            }
                        }
                    }
                }
            },
            {
                "dialog",
                new
                {
                    x=3,
                    instance = new
                    {
                        xxx = "instance",
                        yyy = new
                        {
                            instanceY = "instanceY"
                        }
                    },
                    options = new
                    {
                        xxx = "options",
                        yyy = new[] { "optionY1", "optionY2" }
                    },
                    title = "Dialog Title",
                    subTitle = "Dialog Sub Title"
                }
            },
            {
                "doubleNestedItems",
                new object[][]
                {
                    new object[]
                    {
                        new { x = 1 },
                        new { x = 2 }
                    },
                    new object[]
                    {
                        new { x = 3 }
                    }
                }
            },
            {
                "callstack", new object[]
                {
                    new
                    {
                        x = 3,
                        instance = new
                        {
                            xxx = "instance",
                            yyy = new
                            {
                                instanceY = "instanceY"
                            }
                        },
                        options = new
                        {
                            xxx = "options",
                            yyy = new[] { "optionY1", "optionY2" }
                        },
                        title = "Dialog Title",
                        subTitle = "Dialog Sub Title"
                    },
                    new { x = 2, y = 2 },
                    new { x = 1, y = 1, z = 1 }
                }
            }
        };

        private readonly object scopeForThreadLocale = new Dictionary<string, object>
        {
            { "timestamp", "2018-03-15T13:00:00.000Z" },
            { "unixTimestamp", 1521118800 },
            { "ticks", 637243624200000000 },
            {
                "turn",
                new
                {
                    activity = new
                    {
                        locale = "es-ES"
                    }
                }
            }
        };

        public static HashSet<string> One { get; set; } = new HashSet<string> { "one" };

        public static HashSet<string> OneTwo { get; set; } = new HashSet<string> { "one", "two" };

        public static IEnumerable<object[]> Data => new[]
        {
            #region locale specific tests
            
            //on *nix OS, 'de-DE' will return 'MM.dd.YY HH:mm:ss', on Windows it's 'MM.dd.YYYY HH:mm:ss'
            Test("replace(addDays(timestamp, 1, '', 'de-DE'), '20', '')", "16.03.18 13:00:00"),
            Test("replace(addHours(timestamp, 2, '', 'de-DE'), '20', '')", "15.03.18 15:00:00"),
            Test("replace(addMinutes(timestamp, 30, '', 'de-DE'), '20', '')", "15.03.18 13:30:00"),
            Test("replace(addToTime('2018-01-01T00:00:00.000Z', 1, 'Week', '', 'de-DE'), '20', '')", "08.01.18 00:00:00"),
            Test("startOfDay('2018-03-15T13:30:30.000Z', '', 'fr-FR')", "15/03/2018 00:00:00"),
            Test("startOfHour('2018-03-15T13:30:30.000Z', '', 'fr-FR')", "15/03/2018 13:00:00"),
            Test("startOfMonth('2018-03-15T13:30:30.000Z', '', 'fr-FR')", "01/03/2018 00:00:00"),
            Test("replace(convertToUTC('01/01/2018 00:00:00', 'Pacific Standard Time', '', 'de-DE'), '20', '')", "01.01.18 08:00:00"),
            Test("replace(convertFromUTC('2018-01-02T02:00:00.000Z', 'Pacific Standard Time', '', 'de-DE'), '20', '')", "01.01.18 18:00:00"),
            Test("substring(utcNow('', 'de-DE'), 0, 6)", DateTime.UtcNow.ToString(new CultureInfo("de-DE")).Substring(0, 6)),
            Test("substring(getPastTime(1,'Day', '', 'de-DE'), 0, 6)", DateTime.UtcNow.AddDays(-1).ToString(new CultureInfo("de-DE")).Substring(0, 6)),
            Test("replace(subtractFromTime(timestamp, 1, 'Hour', '', 'de-DE'), '20', '')", "15.03.18 12:00:00"),
            Test("replace(formatEpoch(unixTimestamp, '', 'de-DE'), '20', '')", "15.03.18 13:00:00"),
            Test("replace(formatTicks(ticks, '', 'de-DE'), '2020', '20')", "06.05.20 11:47:00"),
            Test("replace(formatDateTime('2018-03-15', '', 'de-DE'), '20', '')", "15.03.18 00:00:00"),
            Test("substring(getFutureTime(1,'Year', '', 'de-DE'), 0, 10)", DateTime.UtcNow.AddYears(1).ToString(new CultureInfo("de-DE")).Substring(0, 10)),
            Test("replace(addDays(timestamp, 1, '', 'de-DE'), '20', '')", "16.03.18 13:00:00"),
            Test("toUpper('lowercase', 'en-US')", "LOWERCASE"),
            Test("toLower('I AM WHAT I AM', 'fr-FR')", "i am what i am"),
            Test("string(user.income, 'fr-FR')", "100,1"),
            Test("string(user.income, 'en-US')", "100.1"),
            Test("sentenceCase('a', 'fr-FR')", "A"),
            Test("sentenceCase('abc', 'en-US')", "Abc"),
            Test("sentenceCase('aBC', 'fr-FR')", "Abc"),
            Test("titleCase('a', 'en-US')", "A"),
            Test("titleCase('abc dEF', 'en-US')", "Abc Def"),
            #endregion

            #region accessor and element
            Test("`hi\\``", "hi`"),  // `hi\`` -> hi`
            Test("`hi\\y`", "hi\\y"), // `hi\y` -> hi\y
            Test("`\\${a}`", "${a}"), // `\${a}` -> ${a}
            Test("\"ab\\\"cd\"", "ab\"cd"), // "ab\"cd" -> ab"cd
            Test("\"ab`cd\"", "ab`cd"), // "ab`cd" -> ab`cd
            Test("\"ab\\ncd\"", "ab\ncd"),  // "ab\ncd" -> ab [newline] cd
            Test("\"ab\\ycd\"", "ab\\ycd"), //"ab\ycd" -> ab\ycd
            Test("'ab\\'cd'", "ab'cd"), // 'ab\'cd' -> ab'cd
            Test("alist[0].Name", "item1"),
            Test("$index", "index"),
            #endregion

            #region string interpolation test
            Test("``", string.Empty),
            Test("`hi`", "hi"),
            Test(@"`hi\``", "hi`"),
            Test("`${world}`", "world"),
            Test(@"`hi ${string('jack`')}`", "hi jack`"),
            Test(@"`\${world}`", "${world}"), // use escape character
            Test("length(`hello ${world}`)", "hello world".Length),
            Test("json(`{'foo': '${hello}','item': '${world}'}`).foo", "hello"),
            Test("`hello ${world}` == 'hello world'", true),
            Test("`hello ${world}` != 'hello hello'", true),
            Test("`hello ${user.nickname}` == 'hello John'", true),
            Test("`hello ${user.nickname}` != 'hello Dong'", true),
            Test("`hi\\`[1,2,3]`", "hi`[1,2,3]"),
            Test("`hi ${join([\'jack\\`\', \'queen\', \'king\'], ',')}`", "hi jack\\`,queen,king"),
            Test("json(`{\"foo\":${{text:\"hello\"}},\"item\": \"${world}\"}`).foo.text", "hello"),
            Test("json(`{\"foo\":${{\"text\":\"hello\"}},\"item\": \"${world}\"}`).foo.text", "hello"),
            Test("`{expr: hello all}`", "{expr: hello all}"),
            #endregion

            #region SetPathToProperty test
            Test("setPathToValue(path.simple, 3) + path.simple", 6),
            Test("setPathToValue(path.simple, 5) + path.simple", 10),
            Test("setPathToValue(path.array[0], 7) + path.array[0]", 14),
            Test("setPathToValue(path.array[1], 9) + path.array[1]", 18),

            //Test("setPathToValue(path.darray[2][0], 11) + path.darray[2][0]", 22),
            //Test("setPathToValue(path.darray[2][3].foo, 13) + path.darray[2][3].foo", 26),
            //Test("setPathToValue(path.overwrite, 3) + setPathToValue(path.overwrite[0], 4) + path.overwrite[0]", 11),
            //Test("setPathToValue(path.overwrite[0], 3) + setPathToValue(path.overwrite, 4) + path.overwrite", 11),
            //Test("setPathToValue(path.overwrite.prop, 3) + setPathToValue(path.overwrite, 4) + path.overwrite", 11),
            //Test("setPathToValue(path.overwrite.prop, 3) + setPathToValue(path.overwrite[0], 4) + path.overwrite[0]", 11),
            Test("setPathToValue(path.x, null)", null),
            #endregion

            #region Operators test
            Test("user.income-user.outcome", -20.0),
            Test("user.income - user.outcome", -20.0),
            Test("user.income != user.outcome", true),
            Test("user.income!=user.outcome", true),
            Test("user.income == user.outcome", false),
            Test("user.income==user.outcome", false),
            Test("1 + 2", 3),
            Test("1 +\r\n 2", 3),
            Test("- 1 + 2", 1),
            Test("- 1\r\n + 2", 1),
            Test("+ 1 + 2", 3),
            Test("1 - 2", -1),
            Test("1 - (-2)", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 *\r\n 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("1 + 2\r\n * 3", 7),
            Test("4 / 2", 2),
            Test("1 + 3 / 2", 2),
            Test("(1 + 3) / 2", 2),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0, new HashSet<string> { "one", "two", "bag.three" }),
            Test("(one + two) * bag.set.four", 12.0, new HashSet<string> { "one", "two", "bag.set.four" }),
            Test("hello + nullObj", "hello"),
            Test("one + two + hello + world", "3helloworld"),
            Test("one + two + hello + one + two", "3hello12"),
            Test("2^2", 4.0),
            Test("2^\r\n2", 4.0),
            Test("3^2^2", 81.0),
            Test("3\r\n^2^2", 81.0),
            Test("one > 0.5 && two < 2.5", true),
            Test("one > 0.5\r\n && two < 2.5", true),
            Test("one > 0.5 || two < 1.5", true),
            Test("one > 0.5 ||\r\n two < 1.5", true),
            Test("5 % 2", 1),
            Test("!(one == 1.0)", false),
            Test("!!(one == 1.0)", true),
            Test("!exists(xione) || !!exists(two)", true),
            Test("(1 + 2) == (4 - 1)", true),
            Test("!!exists(one) == !!exists(one)", true),
            Test("!(one == 1.0)", false, new HashSet<string> { "one" }),
            Test("!!(one == 1.0)", true, new HashSet<string> { "one" }),
            Test("!(one == 1.0) || !!(two == 2.0)", true, OneTwo),
            Test("!true", false),
            Test("!!true", true),
            Test("!(one == 1.0) || !!(two == 2.0)", true),
            Test("hello == 'hello'", true),
            Test("hello == 'world'", false),
            Test("(1 + 2) != (4 - 1)", false),
            Test("!!exists(one) != !!exists(one)", false),
            Test("!!exists(one) !=\r\n !!exists(one)", false),
            Test("hello!= 'hello'", false),
            Test("hello!= 'world'", true),
            Test("hello != \"hello\"", false),
            Test("hello != \"world\"", true),
            Test("(1 + 2) >= (4 - 1)", true),
            Test("(2 + 2) >= (4 - 1)", true),
            Test("float(5.5) >= float(4 - 1)", true),
            Test("(1 + 2) <= (4 - 1)", true),
            Test("(2 + 2) <= (4 - 1)", false),
            Test("float(5.5) <= float(4 - 1)", false),
            Test("'string'&'builder'", "stringbuilder"),
            Test("\"string\"&\"builder\"", "stringbuilder"),
            Test("\"string\"&\r\n\"builder\"", "stringbuilder"),
            Test("one > 0.5 && two < 2.5", true, OneTwo),
            Test("notThere > 4", false),
            Test("float(5.5) && float(0.0)", true),
            Test("hello && \"hello\"", true),
            Test("items || ((2 + 2) <= (4 - 1))", true), // true || false
            Test("0 || false", true), // true || false
            Test("!(hello)", false), // false
            Test("!(10)", false),
            Test("!(0)", false),
            Test("one > 0.5 || two < 1.5", true, OneTwo),
            Test("one / 0 || two", true),
            Test("0/3", 0),
            Test("True == true", true),
            #endregion

            #region  String functions test
            Test("concat(hello,world)", "helloworld"),
            Test("concat(hello,\r\nworld)", "helloworld"),
            Test("concat('hello','world')", "helloworld"),
            Test("concat(nullObj,'world')", "world"),
            Test("concat('hello',nullObj)", "hello"),
            Test("concat(\"hello\",\"world\")", "helloworld"),
            Test("add(hello,world)", "helloworld"),
            Test("add('hello','world')", "helloworld"),
            Test("add(nullObj,'world')", "world"),
            Test("add('hello',nullObj)", "hello"),
            Test("add(\"hello\",\"world\")", "helloworld"),
            Test("length('hello')", 5),
            Test("length(\"hello\")", 5),
            Test("length(nullObj)", 0),
            Test("length(concat(hello,world))", 10),
            Test("length(concat('[]', 'abc'))", 5),
            Test("length(\r\nconcat(hello,\r\nworld))", 10),
            Test("length(hello + world)", 10),
            Test("count('hello')", 5),
            Test("count(\"hello\")", 5),
            Test("count(concat(hello,world))", 10),
            Test("replace('hello', 'l', 'k')", "hekko"),
            Test("replace('hello', 'L', 'k')", "hello"),
            Test("replace(nullObj, 'L', 'k')", string.Empty),
            Test("replace('hello', 'L', 'k')", "hello"),
            Test("replace('hello', 'l', nullObj)", "heo"),
            Test("replace(\"hello'\", \"'\", '\"')", "hello\""),
            Test("replace('hello\"', '\"', \"'\")", "hello'"),
            Test("replace('hello\"', '\"', '\n')", "hello\n"),
            Test("replace('hello\n', '\n', '\\\\')", "hello\\"),
            Test(@"replace('hello\\', '\\', '\\\\')", @"hello\\"),
            Test(@"replace('hello\n', '\n', '\\\\')", @"hello\\"),
            Test("replaceIgnoreCase('hello', 'L', 'k')", "hekko"),
            Test("replaceIgnoreCase(nullObj, 'L', 'k')", string.Empty),
            Test("split('token1 token2 token3', ' ')", new string[] { "token1", "token2", "token3" }),
            Test("split('token1 token2 token3', '  ')", new string[] { "token1 token2 token3" }),
            Test("split('token one', '')", new string[] { "t", "o", "k", "e", "n", " ", "o", "n", "e" }),
            Test("split('hello','e')", new string[] { "h", "llo" }),
            Test("split('hello','')", new string[] { "h", "e", "l", "l", "o" }),
            Test("split('','')", new string[] { }),
            Test("split(nullObj,'e')", new string[] { string.Empty }),
            Test("split('hello')", new string[] { "h", "e", "l", "l", "o" }),
            Test("split('hello',nullObj)", new string[] { "h", "e", "l", "l", "o" }),
            Test("substring('hello', 0, 5)", "hello"),
            Test("substring('hello', 0, 3)", "hel"),
            Test("substring('hello', 3)", "lo"),
            Test("substring(nullObj, 3)", string.Empty),
            Test("substring('hello', 0, bag.index)", "hel"),
            Test("toLower('UpCase')", "upcase"),
            Test("toLower(nullObj)", string.Empty),
            Test("toUpper('lowercase')", "LOWERCASE"),
            Test("toUpper(nullObj)", string.Empty),
            Test("toLower(toUpper('lowercase'))", "lowercase"),
            Test("trim(' hello ')", "hello"),
            Test("trim(' hello')", "hello"),
            Test("trim(nullObj)", string.Empty),
            Test("trim('hello')", "hello"),
            Test("endsWith('hello','o')", true),
            Test("endsWith('hello','a')", false),
            Test("endsWith(hello,'o')", true),
            Test("endsWith(hello,'a')", false),
            Test("endsWith(nullObj,'h')", false),
            Test("endsWith('hello', nullObj)", true),
            Test("startsWith('hello','h')", true),
            Test("startsWith(nullObj,'h')", false),
            Test("startsWith('hello', nullObj)", true),
            Test("startsWith('hello','a')", false),
            Test("countWord(hello)", 1),
            Test("countWord(nullObj)", 0),
            Test("countWord(concat(hello, ' ', world))", 2),
            Test("addOrdinal(11)", "11th"),
            Test("addOrdinal(11 + 1)", "12th"),
            Test("addOrdinal(11 + 2)", "13th"),
            Test("addOrdinal(11 + 10)", "21st"),
            Test("addOrdinal(11 + 11)", "22nd"),
            Test("addOrdinal(11 + 12)", "23rd"),
            Test("addOrdinal(11 + 13)", "24th"),
            Test("addOrdinal(-1)", "-1"), // original string value
            Test("join(createArray('a','b', 'c', 'd'), '\n')", "a\nb\nc\nd"),
            Test("sentenceCase('a')", "A"),
            Test("sentenceCase('abc')", "Abc"),
            Test("sentenceCase('aBC')", "Abc"),
            Test("titleCase('a')", "A"),
            Test("titleCase('abc dEF')", "Abc Def"),
            #endregion

            #region  Logical comparison functions test
            Test("and(1 == 1, 1 < 2, 1 > 2)", false),
            Test("and(1 == 1,\r\n 1 < 2,\r\n 1 > 2)", false),
            Test("and(!true, !!true)", false), // false && true
            Test("and(!!true, !!true)", true), // true && true
            Test("and(hello != 'world', bool('true'))", true), // true && true
            Test("and(hello == 'world', bool('true'))", false), // false && true
            Test("or(!exists(one), !!exists(one))", true), // false && true
            Test("or(!exists(one), !exists(one))", false), // false && false
            Test("greater(one, two)", false, OneTwo),
            Test("greater(one , 0.5) && less(two , 2.5)", true), // true && true
            Test("greater(one , 0.5) || less(two , 1.5)", true), // true || false
            Test("greater(5, 2)", true),
            Test("greater(2, 2)", false),
            Test("greater(one, two)", false),
            Test("greaterOrEquals((1 + 2) , (4 - 1))", true),
            Test("greaterOrEquals((2 + 2) , (4 - 1))", true),
            Test("greaterOrEquals(float(5.5) , float(4 - 1))", true),
            Test("greaterOrEquals(one, one)", true),
            Test("greaterOrEquals(one, two)", false),
            Test("greaterOrEquals(one, one)", true, One),
            Test("greaterOrEquals(one, two)", false, OneTwo),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true),
            Test("less(one, two)", true, OneTwo),
            Test("lessOrEquals(one, one)", true, new HashSet<string> { "one" }),
            Test("lessOrEquals(one, two)", true, OneTwo),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),
            Test("lessOrEquals((1 + 2) , (4 - 1))", true),
            Test("lessOrEquals((2 + 2) , (4 - 1))", false),
            Test("lessOrEquals(float(5.5) , float(4 - 1))", false),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),
            Test("equals(hello, 'hello')", true),
            Test("equals(bag.index, 3)", true),
            Test("equals(bag.index, 2)", false),
            Test("equals(max(createArray(1, 2, 3, 4)), 4.0)", true),
            Test("equals(max(createArray(1, 2, 3, 4), 5.0), 5)", true),
            Test("equals(hello == 'world', bool('true'))", false), // false, true
            Test("equals(hello == 'world', bool(0))", false), // false, true
            Test("if(!exists(one), 'r1', 'r2')", "r2"), // false
            Test("if(!!exists(one), 'r1', 'r2')", "r1"), // true
            Test("if(0, 'r1', 'r2')", "r1"), // true
            Test("if(bool('true'), 'r1', 'r2')", "r1"), // true
            Test("if(istrue, 'r1', 'r2')", "r1"), // true
            Test("if(bag.name == null, \"hello\",  bag.name)", "mybag"),
            Test("if(user.name == null, \"hello\",  user.name)", "hello"), // user.name don't exist
            Test("if(user.name == null, '',  user.name)", string.Empty), // user.name don't exist
            Test("if(one > 0, one, two)", 1),
            Test("if(one < 0, one, two)", 2),
            Test("exists(one)", true),
            Test("exists(xxx)", false),
            Test("exists(one.xxx)", false),
            Test("not(one != null)", false),
            Test("not(not(one != null))", true),
            Test("not(false)", true),
            Test("not(one == 1.0)", false, new HashSet<string> { "one" }),
            Test("not(not(one == 1.0))", true, new HashSet<string> { "one" }),
            Test("not(false)", true),
            Test("and(one > 0.5, two < 2.5)", true, OneTwo),
            Test("and(float(5.5), float(0.0))", true),
            Test("and(hello, \"hello\")", true),
            Test("or(items, (2 + 2) <= (4 - 1))", true), // true || false
            Test("or(0, false)", true), // true || false
            Test("not(hello)", false), // false
            Test("not(10)", false),
            Test("not(0)", false),
            Test("if(hello, 'r1', 'r2')", "r1"),
            Test("if(null, 'r1', 'r2')", "r2"),
            Test("if(hello * 5, 'r1', 'r2')", "r2"),
            Test("if(10, 'r1', 'r2')", "r1"),
            Test("emptyList == []", true),
            Test("emptyList != []", false),
            Test("emptyList == {}", false),
            Test("emptyObject == {}", true),
            Test("emptyObject != {}", false),
            Test("emptyObject == []", false),
            Test("emptyJObject == {}", true),
            Test("emptyJObject != {}", false),
            Test("emptyJObject == []", false),
            Test("emptyAnonymousObject == {}", true),
            Test("emptyAnonymousObject != {}", false),
            Test("emptyAnonymousObject == []", false),
            Test("emptyList == [ ]", true),
            Test("emptyList == {  }", false),
            Test("emptyObject == {  }", true),
            Test("emptyObject == [  ]", false),
            Test("{} == null", false),
            Test("{} != null", true),
            Test("[] == null", false),
            Test("{} != null", true),
            Test("{} == {}", true),
            Test("[] == []", true),
            Test("{} != []", true),
            Test("[] == {}", false),
            Test("null < 1", false),
            Test("null >= 1", false),
            #endregion

            #region  Conversion functions test
            Test("float('10.333')", 10.333f),
            Test("float('10')", 10.0f),
            Test("int('10')", 10),
            Test("int(12345678912345678 + 1)", 12345678912345679),
            Test("string('str')", "str"),
            Test("string('str\"')", "str\""),
            Test("string(one)", "1"),
            Test("string(bool(1))", "true"),
            Test("string(bag.set)", "{\"four\":4.0}"),
            Test("bool(1)", true),
            Test("bool(0)", true),
            Test("bool(null)", false),
            Test("bool(hello * 5)", false),
            Test("bool('false')", true),
            Test("bool('hi')", true),
            Test("[1,2,3]", new List<object> { 1, 2, 3 }),
            Test("[1,2,3, [4,5]]", new List<object> { 1, 2, 3, new List<object> { 4, 5 } }),
            Test("\"[1,2,3]\"", "[1,2,3]"),
            Test("[1, bool(0), string(bool(1)), float(\'10\')]", new List<object> { 1, true, "true", 10.0 }),
            Test("[\"a\", \"b[]\", \"c[][][]\"][1]", "b[]"),
            Test("[\'a\', [\'b\', \'c\']][1][0]", "b"),
            Test("union([\"a\", \"b\", \"c\"], [\"d\", [\"e\", \"f\"], \"g\"][1])", new List<string> { "a", "b", "c", "e", "f" }),
            Test("union([\"a\", \"b\", \"c\"], [\"d\", [\"e\", \"f\"], \"g\"][1])[1]",  "b"),
            Test("createArray('h', 'e', 'l', 'l', 'o')", new List<object> { "h", "e", "l", "l", "o" }),
            Test("createArray('h',\r\n 'e',\r\n 'l',\r\n 'l',\r\n 'o')", new List<object> { "h", "e", "l", "l", "o" }),
            Test("createArray(1, bool(0), string(bool(1)), float('10'))", new List<object> { 1, true, "true", 10.0f }),
            Test("createArray()", new List<object> { }),
            Test("[]", new List<object> { }),
            Test("binary(hello)", new byte[] { 104, 101, 108, 108, 111 }),
            Test("count(binary(hello))", 5),
            Test("base64(hello)", "aGVsbG8="),
            Test("base64(byteArr)", "AwUBDA=="),
            Test("base64ToBinary(base64(byteArr))", new byte[] { 3, 5, 1, 12 }),
            Test("base64ToString(base64(hello))", "hello"),
            Test("base64(base64ToBinary(\"AwUBDA==\"))", "AwUBDA=="),
            Test("dataUri(hello)", "data:text/plain;charset=utf-8;base64,aGVsbG8="),
            Test("dataUriToBinary(base64(hello))", new byte[] { 97, 71, 86, 115, 98, 71, 56, 61 }),
            Test("dataUriToString(dataUri(hello))", "hello"),
            Test("xml('{\"person\": {\"name\": \"Sophia Owen\", \"city\": \"Seattle\"}}')", $"<root type=\"object\">{Environment.NewLine}  <person type=\"object\">{Environment.NewLine}    <name type=\"string\">Sophia Owen</name>{Environment.NewLine}    <city type=\"string\">Seattle</city>{Environment.NewLine}  </person>{Environment.NewLine}</root>"),
            Test("uriComponent('http://contoso.com')", "http%3A%2F%2Fcontoso.com"),
            Test("uriComponentToString('http%3A%2F%2Fcontoso.com')", "http://contoso.com"),
            Test("json(jsonContainsDatetime).date", "/Date(634250351766060665)/"),
            Test("json(jsonContainsDatetime).invalidDate", "/Date(whatever)/"),
            Test("formatNumber(20.0000, 2, 'en-US')", "20.00"),
            Test("formatNumber(12.123, 2, 'en-US')", "12.12"),
            Test("formatNumber(1.551, 2, 'en-US')", "1.55"),
            Test("formatNumber(12.123, 4, 'en-US')", "12.1230"),
            Test("formatNumber(12000.3, 4, 'fr-fr') == '12\x00A0000,3000' || formatNumber(12000.3, 4, 'fr-fr') == '12\x202F000,3000'", true),
            #endregion

            #region  Math functions test
            Test("add(1, 2, 3)", 6),
            Test("add(1, 2)", 3),
            Test("add(1.0, 2.0)", 3.0),
            Test("add(mul(1, 2), 3)", 5),
            Test("max(mul(1, 2), 5) ", 5),
            Test("max(5) ", 5),
            Test("max(4, 5) ", 5),
            Test("max(createArray(1, 2, 3, 4))", 4),
            Test("max(createArray(1, 2, 3, 4),5.0)", 5.0),
            Test("max(1, 4, 5) ", 5),
            Test("min(mul(1, 2), 5) ", 2),
            Test("min(createArray(1, 2, 3, 4))", 1),
            Test("min(createArray(1, 2, 3, 4),5)", 1),
            Test("min(4, 5) ", 4),
            Test("min(4) ", 4),
            Test("min(1.0, two) + max(one, 2.0)", 3.0, OneTwo),
            Test("sub(2, 1)", 1),
            Test("sub(2, 1, 1)", 0),
            Test("sub(2.0, 0.5)", 1.5),
            Test("mul(2, 5)", 10),
            Test("mul(2, 5, 2)", 20),
            Test("div(mul(2, 5), 2)", 5),
            Test("div(5, 2)", 2),
            Test("div(5, 2 ,2)", 1),
            Test("exp(2,2)", 4.0),
            Test("mod(5,2)", 1),
            Test("rand(1, 2)", 1),
            Test("rand(2, 3)", 2),
            Test("range(1,4)", new[] { 1, 2, 3, 4 }),
            Test("range(-1,6)", new[] { -1, 0, 1, 2, 3, 4 }),
            Test("floor(3.51)", 3),
            Test("floor(4.00)", 4),
            Test("ceiling(3.51)", 4),
            Test("ceiling(4.00)", 4),
            Test("round(3.51)", 4),
            Test("round(3.55, 1)", 3.6),
            Test("round(3.12134, 3)", 3.121),
            #endregion

            #region  Date and time function test

            // init dateTime: 2018-03-15T13:00:00Z
            Test("isDefinite('helloworld')", false),
            Test("isDefinite('2012-12-21')", true),
            Test("isDefinite('XXXX-12-21')", false),
            Test("isDefinite(validFullDateTimex)", true),
            Test("isDefinite(invalidFullDateTimex)", false),
            Test("isTime(validHourTimex)", true),
            Test("isTime(invalidHourTimex)", false),
            Test("isDuration('PT30M')", true),
            Test("isDuration('2012-12-21T12:30')", false),
            Test("isDate('PT30M')", false),
            Test("isDate('2012-12-21T12:30')", true),
            Test("isTimeRange('PT30M')", false),
            Test("isTimeRange(validTimeRange)", true),
            Test("isDateRange('PT30M')", false),
            Test("isDateRange('2012-02')", true),
            Test("isPresent('PT30M')", false),
            Test("isPresent(validNow)", true),
            Test("addDays(timestamp, 1)", "2018-03-16T13:00:00.000Z"),
            Test("addDays(timestampObj, 1)", "2018-03-16T13:00:00.000Z"),
            Test("addDays(timestamp, 1,'MM-dd-yy')", "03-16-18"),
            Test("addHours(timestamp, 1)", "2018-03-15T14:00:00.000Z"),
            Test("addHours(timestampObj, 1)", "2018-03-15T14:00:00.000Z"),
            Test("addHours(timestamp, 1,'MM-dd-yy hh-mm')", "03-15-18 02-00"),
            Test("addMinutes(timestamp, 1)", "2018-03-15T13:01:00.000Z"),
            Test("addMinutes(timestampObj, 1)", "2018-03-15T13:01:00.000Z"),
            Test("addMinutes(timestamp, 1, 'MM-dd-yy hh-mm')", "03-15-18 01-01"),
            Test("addSeconds(timestamp, 1)", "2018-03-15T13:00:01.000Z"),
            Test("addSeconds(timestampObj, 1)", "2018-03-15T13:00:01.000Z"),
            Test("addSeconds(timestamp, 1, 'MM-dd-yy hh-mm-ss')", "03-15-18 01-00-01"),
            Test("dayOfMonth(timestamp)", 15),
            Test("dayOfMonth(timestampObj)", 15),
            Test("dayOfWeek(timestamp)", 4), // Thursday
            Test("dayOfWeek(timestampObj)", 4), // Thursday
            Test("dayOfYear(timestamp)", 74),
            Test("dayOfYear(timestampObj)", 74),
            Test("month(timestamp)", 3),
            Test("month(timestampObj)", 3),
            Test("date(timestamp)", "3/15/2018"), // Default. TODO
            Test("date(timestampObj)", "3/15/2018"),
            Test("year(timestamp)", 2018),
            Test("year(timestampObj)", 2018),
            Test("length(utcNow())", 24),
            Test("utcNow('MM-DD-YY')", DateTime.UtcNow.ToString("MM-DD-YY")),
            Test("formatDateTime(notISOTimestamp)", "2018-03-15T13:00:00.000Z"),
            Test("formatDateTime(notISOTimestamp, 'MM-dd-yy')", "03-15-18"),
            Test("formatDateTime('2018-03-15')", "2018-03-15T00:00:00.000Z"),
            Test("formatDateTime(timestampObj)", "2018-03-15T13:00:00.000Z"),
            Test("formatEpoch(unixTimestamp)", "2018-03-15T13:00:00.000Z"),
            Test("formatEpoch(unixTimestampFraction)", "2018-03-15T13:00:00.500Z"),
            Test("formatTicks(ticks)", "2020-05-06T11:47:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Year')", "2017-03-15T13:00:00.000Z"),
            Test("subtractFromTime(timestampObj, 1, 'Year')", "2017-03-15T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Month')", "2018-02-15T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Week')", "2018-03-08T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Day')", "2018-03-14T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Hour')", "2018-03-15T12:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Minute')", "2018-03-15T12:59:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Second')", "2018-03-15T12:59:59.000Z"),
            Test("dateReadBack(timestamp, addDays(timestamp, 1))", "tomorrow"),
            Test("dateReadBack(timestampObj, addDays(timestamp, 1))", "tomorrow"),
            Test("dateReadBack(addDays(timestamp, 1),timestamp)", "yesterday"),
            Test("getTimeOfDay('2018-03-15T00:00:00.000Z')", "midnight"),
            Test("getTimeOfDay(timestampObj)", "afternoon"),
            Test("getTimeOfDay('2018-03-15T08:00:00.000Z')", "morning"),
            Test("getTimeOfDay('2018-03-15T12:00:00.000Z')", "noon"),
            Test("getTimeOfDay('2018-03-15T13:00:00.000Z')", "afternoon"),
            Test("getTimeOfDay('2018-03-15T18:00:00.000Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T22:00:00.000Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T23:00:00.000Z')", "night"),
            Test("getPastTime(1,'Year','MM-dd-yy')", DateTime.UtcNow.AddYears(-1).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Month','MM-dd-yy')", DateTime.UtcNow.AddMonths(-1).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Week','MM-dd-yy')", DateTime.UtcNow.AddDays(-7).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Day','MM-dd-yy')", DateTime.UtcNow.AddDays(-1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Year','MM-dd-yy')", DateTime.UtcNow.AddYears(1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Month','MM-dd-yy')", DateTime.UtcNow.AddMonths(1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Week','MM-dd-yy')", DateTime.UtcNow.AddDays(7).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Day','MM-dd-yy')", DateTime.UtcNow.AddDays(1).ToString("MM-dd-yy")),
            Test("convertFromUTC('2018-01-02T02:00:00.000Z', 'Pacific Standard Time', 'D', 'en-US')", "Monday, January 1, 2018"),
            Test("convertFromUTC(timestampObj2, 'Pacific Standard Time', 'D', 'en-US')", "Monday, January 1, 2018"),
            Test("convertFromUTC('2018-01-02T01:00:00.000Z', 'America/Los_Angeles', 'D', 'en-US')", "Monday, January 1, 2018"),
            Test("convertToUTC('01/01/2018 00:00:00', 'Pacific Standard Time')", "2018-01-01T08:00:00.000Z"),
            Test("addToTime('2018-01-01T08:00:00.000Z', 1, 'Day', 'D', 'en-US')", "Tuesday, January 2, 2018"),
            Test("addToTime('2018-01-01T00:00:00.000Z', 1, 'Week')", "2018-01-08T00:00:00.000Z"),
            Test("addToTime(timestampObj2, 1, 'Week')", "2018-01-09T02:00:00.000Z"),
            Test("startOfDay('2018-03-15T13:30:30.000Z')", "2018-03-15T00:00:00.000Z"),
            Test("startOfDay(timestampObj2)", "2018-01-02T00:00:00.000Z"),
            Test("startOfHour('2018-03-15T13:30:30.000Z')", "2018-03-15T13:00:00.000Z"),
            Test("startOfHour(timestampObj)", "2018-03-15T13:00:00.000Z"),
            Test("startOfMonth('2018-03-15T13:30:30.000Z')", "2018-03-01T00:00:00.000Z"),
            Test("startOfMonth(timestampObj)", "2018-03-01T00:00:00.000Z"),
            Test("ticks('2018-01-01T08:00:00.000Z')", 636503904000000000),
            Test("dateTimeDiff('2019-01-01T08:00:00.000Z','2018-01-01T08:00:00.000Z')", 315360000000000),
            Test("dateTimeDiff('2017-01-01T08:00:00.000Z','2018-01-01T08:00:00.000Z')", -315360000000000),
            Test("dateTimeDiff(timestampObj,timestampObj2)", 62604000000000),
            Test("ticks(timestampObj3)", 636503904000000000),
            Test("ticksToDays(2193385800000000)", 2538.64097222),
            Test("ticksToHours(2193385800000000)", 60927.383333333331),
            Test("ticksToMinutes(2193385811100000)", 3655643.0185),
            Test("isMatch(getPreviousViableDate('XXXX-07-10'), '20[0-9]{2}-07-10')", true),
            Test("isMatch(getPreviousViableDate('XXXX-07-10', 'Asia/Shanghai'), '20[0-9]{2}-07-10')", true),
            Test("getPreviousViableDate('XXXX-02-29')", "2020-02-29"),
            Test("getPreviousViableDate('XXXX-02-29', 'Pacific Standard Time')", "2020-02-29"),
            Test("isMatch(getNextViableDate('XXXX-07-10'), '202[0-9]-07-10')", true),
            Test("isMatch(getNextViableDate('XXXX-07-10', 'Europe/London'), '202[0-9]-07-10')", true),
            Test("getNextViableDate('XXXX-02-29')", "2024-02-29"),
            Test("getNextViableDate('XXXX-02-29', 'America/Los_Angeles')", "2024-02-29"),
            Test("isMatch(getNextViableTime('TXX:40:20'), 'T[0-2][0-9]:40:20')", true),
            Test("isMatch(getNextViableTime('TXX:40:20', 'Asia/Tokyo'), 'T[0-2][0-9]:40:20')", true),
            Test("isMatch(getNextViableTime('TXX:05:10'), 'T[0-2][0-9]:05:10')", true),
            Test("isMatch(getNextViableTime('TXX:05:10', 'Europe/Paris'), 'T[0-2][0-9]:05:10')", true),
            Test("isMatch(getPreviousViableTime('TXX:40:20'), 'T[0-2][0-9]:40:20')", true),
            Test("isMatch(getPreviousViableTime('TXX:40:20', 'Eastern Standard Time'), 'T[0-2][0-9]:40:20')", true),
            Test("isMatch(getPreviousViableTime('TXX:05:10'), 'T[0-2][0-9]:05:10')", true),
            Test("isMatch(getPreviousViableTime('TXX:05:10', 'Central Standard Time'), 'T[0-2][0-9]:05:10')", true),

            #endregion

            #region uri parsing function test
            Test("uriHost('https://www.localhost.com:8080')", "www.localhost.com"),
            Test("uriPath('http://www.contoso.com/catalog/shownew.htm?date=today')", "/catalog/shownew.htm"),
            Test("uriPathAndQuery('http://www.contoso.com/catalog/shownew.htm?date=today')", "/catalog/shownew.htm?date=today"),
            Test("uriPort('http://www.localhost:8080')", 8080),
            Test("uriQuery('http://www.contoso.com/catalog/shownew.htm?date=today')", "?date=today"),
            Test("uriScheme('http://www.contoso.com/catalog/shownew.htm?date=today')", "http"),
            #endregion

            #region  collection functions test
            Test("sum(createArray(1, 2))", 3),
            Test("sum(createArray(one, two, 3))", 6.0),
            Test("average(createArray(1, 2))", 1.5),
            Test("average(createArray(one, two, 3))", 2.0),
            Test("contains('hello world', 'hello')", true),
            Test("contains('hello world', 'hellow')", false),
            Test("contains('hello world',\r\n 'hellow')", false),
            Test("contains(items, 'zero')", true),
            Test("contains(items, 'hi')", false),
            Test("contains(bag, 'three')", true),
            Test("contains(bag, 'xxx')", false),
            Test("concat(null, [1, 2], null)", new List<object> { 1, 2 }),
            Test("concat(createArray(1, 2), createArray(3, 4))", new List<object> { 1, 2, 3, 4 }),
            Test("concat(['a', 'b'], ['b', 'c'], ['c', 'd'])", new List<object> { "a", "b", "b", "c", "c", "d" }),
            Test("count(split(hello,'e'))", 2),
            Test("count(createArray('h', 'e', 'l', 'l', 'o'))", 5),
            Test("empty('')", true),
            Test("empty('a')", false),
            Test("empty(bag)", false),
            Test("empty(items)", false),
            Test("first(items)", "zero"),
            Test("first('hello')", "h"),
            Test("first(createArray(0, 1, 2))", 0),
            Test("first(1)", null),
            Test("first(nestedItems).x", 1, new HashSet<string> { "nestedItems" }),
            Test("join(items,',')", "zero,one,two"),
            Test("join(createArray('a', 'b', 'c'), '.')", "a.b.c"),
            Test("join(createArray('a', 'b', 'c'), ',', ' and ')", "a,b and c"),
            Test("join(createArray('a', 'b'), ',', ' and ')", "a and b"),
            Test("join(createArray(\r\n'a',\r\n 'b'), ','\r\n,\r\n ' and ')", "a and b"),
            Test("join(foreach(dialog, item, item.key), ',')", "x,instance,options,title,subTitle"),
            Test("join(foreach(dialog, item => item.key), ',')", "x,instance,options,title,subTitle"),
            Test("foreach(dialog, item, item.value)[1].xxx", "instance"),
            Test("foreach(dialog, item=>item.value)[1].xxx", "instance"),
            Test("join(foreach(items, item, item), ',')", "zero,one,two"),
            Test("join(foreach(items, item=>item), ',')", "zero,one,two"),
            Test("join(foreach(indicesAndValues(items), item, item.value), ',')", "zero,one,two"),
            Test("join(foreach(nestedItems, i, i.x + first(nestedItems).x), ',')", "2,3,4", new HashSet<string> { "nestedItems" }),
            Test("join(foreach(items, item, concat(item, string(count(items)))), ',')", "zero3,one3,two3", new HashSet<string> { "items" }),
            Test("join(select(items, item, item), ',')", "zero,one,two"),
            Test("join(select(items, item=> item), ',')", "zero,one,two"),
            Test("join(select(nestedItems, i, i.x + first(nestedItems).x), ',')", "2,3,4", new HashSet<string> { "nestedItems" }),
            Test("join(select(items, item, concat(item, string(count(items)))), ',')", "zero3,one3,two3", new HashSet<string> { "items" }),
            Test("join(where(items, item, item == 'two'), ',')", "two"),
            Test("join(where(items, item => item == 'two'), ',')", "two"),
            Test("string(where(dialog, item, item.value=='Dialog Title'))", "{\"title\":\"Dialog Title\"}"),
            Test("first(where(indicesAndValues(items), elt, elt.index > 1)).value", "two"),
            Test("first(where(indicesAndValues(bag), elt, elt.index == \"three\")).value", 3),
            Test("join(foreach(where(nestedItems, item, item.x > 1), result, result.x), ',')", "2,3", new HashSet<string> { "nestedItems" }),
            Test("join(foreach(doubleNestedItems, items, join(foreach(items, item, concat(y, string(item.x))), ',')), ',')", "y1,y2,y3"),
            Test("join(foreach(doubleNestedItems, items, join(foreach(items, item, items[0].x), ',')), ',')", "1,1,3"),
            Test("count(where(doubleNestedItems, items, count(where(items, item, item.x == 1)) == 1))", 1),
            Test("count(where(doubleNestedItems, items, count(where(items, item, count(items) == 1)) == 1))", 1),
            Test("last(items)", "two"),
            Test("last('hello')", "o"),
            Test("last(createArray(0, 1, 2))", 2),
            Test("last(1)", null),
            Test("count(union(createArray('a', 'b')))", 2),
            Test("count(union(createArray('a', 'b'), createArray('b', 'c'), createArray('b', 'd')))", 4),
            Test("count(intersection(createArray('a', 'b')))", 2),
            Test("count(intersection(createArray('a', 'b'), createArray('b', 'c'), createArray('b', 'd')))", 1),
            Test("skip(createArray('H','e','l','l','0'),2)", new List<object> { "l", "l", "0" }),
            Test("take(createArray('H','e','l','l','0'),2)", new List<object> { "H", "e" }),
            Test("subArray(createArray('H','e','l','l','o'),2,5)", new List<object> { "l", "l", "o" }),
            Test("count(newGuid())", 36),
            Test("indexOf(newGuid(), '-')", 8),
            Test("EOL()", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n"),
            Test("indexOf(nullObj, '-')", -1),
            Test("indexOf(hello, nullObj)", 0),
            Test("indexOf(hello, '-')", -1),
            Test("indexOf(json('[\"a\", \"b\"]'), 'a')", 0),
            Test("indexOf(json('[\"a\", \"b\"]'), 'c')", -1),
            Test("indexOf([\'abc\', \'def\', \'ghi\'], \'def\')", 1),
            Test("indexOf(createArray('abc', 'def', 'ghi'), 'def')", 1),
            Test("indexOf(createArray('abc', 'def', 'ghi'), 'klm')", -1),
            Test("lastIndexOf(newGuid(), '-')", 23),
            Test("lastIndexOf(hello, '-')", -1),
            Test("lastIndexOf(nullObj, '-')", -1),
            Test("lastIndexOf(hello, nullObj)", 4),
            Test("lastIndexOf(json('[\"a\", \"b\", \"a\"]'), 'a')", 2),
            Test("lastIndexOf(json('[\"a\", \"b\"]'), 'c')", -1),
            Test("lastIndexOf(createArray('abc', 'def', 'ghi', 'def'), 'def')", 3),
            Test("lastIndexOf(createArray('abc', 'def', 'ghi'), 'klm')", -1),
            Test("length(newGuid())", 36),
            Test("sortBy(items)", new List<object> { "one", "two", "zero" }),
            Test("sortBy(nestedItems, 'x')[0].x", 1),
            Test("sortByDescending(items)", new List<object> { "zero", "two", "one" }),
            Test("sortByDescending(nestedItems, 'x')[0].x", 3),
            Test("flatten(createArray(1,createArray(2),createArray(createArray(3, 4), createArray(5,6))))", new List<object> { 1, 2, 3, 4, 5, 6 }),
            Test("flatten(createArray(1,createArray(2),createArray(createArray(3, 4), createArray(5,6))), 1)", new List<object> { 1, 2, new List<object>() { 3, 4 }, new List<object>() { 5, 6 } }),
            Test("unique(createArray(1, 5, 1))", new List<object>() { 1, 5 }),
            #endregion

            #region  Object manipulation and construction functions
            Test("string(addProperty(json('{\"key1\":\"value1\"}'), 'key2','value2'))", "{\"key1\":\"value1\",\"key2\":\"value2\"}"),
            Test("string(setProperty(json('{\"key1\":\"value1\"}'), 'key1','value2'))", "{\"key1\":\"value2\"}"),
            Test("string(removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), 'key2'))", "{\"key1\":\"value1\"}"),
            Test("coalesce(nullObj,hello,nullObj)", "hello"),
            Test("xPath(xmlStr,'/produce/item/name')", new[] { "<name>Gala</name>", "<name>Honeycrisp</name>" }),
            Test("xPath(xmlStr,'sum(/produce/item/count)')", 30),
            Test("jPath(jsonStr,'Manufacturers[0].Products[0].Price')", 50),
            Test("jPath(jsonStr,'$..Products[?(@.Price >= 50)].Name')", new[] { "Anvil", "Elbow Grease" }),
            Test("{text: 'hello'}.text", "hello"),
            Test("string(addProperty({'key1':'value1'}, 'key2','value2'))", "{\"key1\":\"value1\",\"key2\":\"value2\"}"),
            Test("foreach(items, x, addProperty({}, 'a', x))[0].a", "zero"),
            Test("foreach(items, x => addProperty({}, 'a', x))[0].a", "zero"),
            Test("string(setProperty({'key1':'value1'}, 'key1','value2'))", "{\"key1\":\"value2\"}"),
            Test("string(setProperty({}, 'key1','value2'))", "{\"key1\":\"value2\"}"),
            Test("string([{a: 1}, {b: 2}, {c: 3}][0])", "{\"a\":1}"),
            Test("string({obj: {'name': 'adams'}})", "{\"obj\":{\"name\":\"adams\"}}"),
            Test("string({obj: {'name': 'adams'}, txt: {utter: 'hello'}})", "{\"obj\":{\"name\":\"adams\"},\"txt\":{\"utter\":\"hello\"}}"),
            Test("{a: 1, b: newExpr}.b", "new land"),
            Test("{name: user.name}.name", null),
            Test("{name: user.nickname}.name", "John"),
            Test("setProperty({}, 'name', user.name).name", null),
            Test("setProperty({name: 'Paul'}, 'name', user.name).name", null),
            Test("setProperty({}, 'name', user.nickname).name", "John"),
            Test("addProperty({}, 'name', user.name).name", null),
            Test("string(merge(json(json1), json(json2)))", "{\"FirstName\":\"John\",\"LastName\":\"Smith\",\"Enabled\":true,\"Roles\":[\"Customer\",\"Admin\"]}"),
            Test("string(merge(json(json1), json(json2), json(json3)))", "{\"FirstName\":\"John\",\"LastName\":\"Smith\",\"Enabled\":true,\"Roles\":[\"Customer\",\"Admin\"],\"Age\":36}"),
            #endregion

            #region  Memory access
            Test("getProperty(bag, concat('na','me'))", "mybag"),
            Test("getProperty('bag').index", 3),
            Test("getProperty('a:b')", "stringa:b"),
            Test("getProperty(concat('he', 'llo'))", "hello"),
            Test("items[2]", "two", new HashSet<string> { "items[2]" }),
            Test("bag.list[bag.index - 2]", "blue", new HashSet<string> { "bag.list", "bag.index" }),
            Test("items[nestedItems[1].x]", "two", new HashSet<string> { "items", "nestedItems[1].x" }),
            Test("bag['name']", "mybag"),
            Test("bag[substring(concat('na','me','more'), 0, length('name'))]", "mybag"),
            Test("items[1+1]", "two"),
            Test("getProperty(null, 'p')", null),
            Test("(getProperty(null, 'p'))[1]", null),
            #endregion

            #region Regex
            Test("isMatch('abc', '^[ab]+$')", false), // simple character classes ([abc]), "+" (one or more)
            Test("isMatch('abb', '^[ab]+$')", true), // simple character classes ([abc])
            Test("isMatch('123', '^[^abc]+$')", true), // complemented character classes ([^abc])
            Test("isMatch('12a', '^[^abc]+$')", false), // complemented character classes ([^abc])
            Test("isMatch('123', '^[^a-z]+$')", true), // complemented character classes ([^a-z])
            Test("isMatch('12a', '^[^a-z]+$')", false), // complemented character classes ([^a-z])
            Test("isMatch('a1', '^[a-z]?[0-9]$')", true), // "?" (zero or one)
            Test("isMatch('1', '^[a-z]?[0-9]$')", true), // "?" (zero or one)
            Test("isMatch('1', '^[a-z]*[0-9]$')", true), // "*" (zero or more)
            Test("isMatch('abc1', '^[a-z]*[0-9]$')", true), // "*" (zero or more)
            Test("isMatch('ab', '^[a-z]{1}$')", false), // "{x}" (exactly x occurrences)
            Test("isMatch('ab', '^[a-z]{1,2}$')", true), // "{x,y}" (at least x, at most y, occurrences)
            Test("isMatch('abc', '^[a-z]{1,}$')", true), // "{x,}" (x occurrences or more)
            Test("isMatch('Name', '^(?i)name$')", true), // "(?i)x" (x ignore case)
            Test("isMatch('FORTUNE', '(?i)fortune|future')", true), // "x|y" (alternation)
            Test("isMatch('FUTURE', '(?i)fortune|future')", true), // "x|y" (alternation)
            Test("isMatch('A', '(?i)fortune|future')", false), // "x|y" (alternation)
            Test("isMatch('abacaxc', 'ab.+?c')", true), // "+?" (lazy versions)
            Test("isMatch('abacaxc', 'ab.*?c')", true), // "*?" (lazy versions)
            Test("isMatch('abacaxc', 'ab.??c')", true), // "??" (lazy versions)
            Test("isMatch('12abc34', '([0-9]+)([a-z]+)([0-9]+)')", true), // "(...)" (simple group)
            Test("isMatch('12abc', '([0-9]+)([a-z]+)([0-9]+)')", false), // "(...)" (simple group)
            Test(@"isMatch('a', '\\w{1}')", true), // "\w" (match [a-zA-Z0-9_])
            Test(@"isMatch('1', '\\d{1}')", true), // "\d" (match [0-9])
            Test(@"isMatch('12.5', '[0-9]+(\\.5)')", true), // "\." (match .)
            Test(@"isMatch('12x5', '[0-9]+(\\.5)')", false), // "\." (match .)
            #endregion

            #region type checking
            Test("isString('abc')", true),
            Test("isString(123)", false),
            Test("isString(null)", false),
            Test("isInteger('abc')", false),
            Test("isInteger(123)", true),
            Test("isInteger(null)", false),
            Test("isFloat('abc')", false),
            Test("isFloat(123.234)", true),
            Test("isFloat(null)", false),
            Test("isArray(createArray(1,2,3))", true),
            Test("isArray(123.234)", false),
            Test("isArray(null)", false),
            Test("isObject(null)", false),
            Test("isObject(emptyJObject)", true),
            Test("isObject(dialog)", true),
            Test("isObject(123.234)", false),
            Test("isBoolean(null)", false),
            Test("isBoolean(2 + 3)", false),
            Test("isBoolean(2 > 1)", true),
            Test("isDateTime(2 + 3)", false),
            Test("isDateTime(null)", false),
            Test("isDateTime(timestamp)", true),
            Test("isDateTime(timestampObj)", true),
            #endregion

            #region Empty expression
            Test(string.Empty, string.Empty),
            Test(string.Empty, string.Empty),
            #endregion

            #region TriggerTree Tests
            Test("ignore(true)", true),
            #endregion
        };

        public static IEnumerable<object[]> DataForThreadLocale => new[]
        {
            Test("replace(addDays(timestamp, 1, '', 'en-US'), '20', '')", "3/16/18 1:00:00 PM"),
            Test("addDays(timestamp, 1, 'D')", "vendredi 16 mars 2018"),
            Test("addHours(timestamp, 2, 'D')", "jeudi 15 mars 2018"),
            Test("addMinutes(timestamp, 30, '')", "15/03/2018 13:30:00"),
            Test("addToTime('2018-01-01T00:00:00.000Z', 1, 'Week', 'D')", "lundi 8 janvier 2018"),
            Test("startOfDay('2018-03-15T13:30:30.000Z', 'D')", "jeudi 15 mars 2018"),
            Test("startOfHour('2018-03-15T13:30:30.000Z', '')", "15/03/2018 13:00:00"),
            Test("startOfMonth('2018-03-15T13:30:30.000Z', '')", "01/03/2018 00:00:00"),
            Test("convertToUTC('01/01/2018 00:00:00', 'Pacific Standard Time', 'D')", "lundi 1 janvier 2018"),
            Test("convertFromUTC('2018-01-02T02:00:00.000Z', 'Pacific Standard Time', '')", "01/01/2018 18:00:00"),
            Test("utcNow('D')", DateTime.UtcNow.ToString("D", new CultureInfo("fr-FR"))),
            Test("getPastTime(1,'Day', 'D')", DateTime.UtcNow.AddDays(-1).ToString("D", new CultureInfo("fr-FR"))),
            Test("subtractFromTime(timestamp, 1, 'Hour', '')", "15/03/2018 12:00:00"),
            Test("formatEpoch(unixTimestamp, '')", "15/03/2018 13:00:00"),
            Test("formatTicks(ticks, '')", "06/05/2020 11:47:00"),
            Test("formatDateTime('2018-03-15', 'D')", "jeudi 15 mars 2018"),
            Test("getFutureTime(1, 'Year', 'D')", DateTime.UtcNow.AddYears(1).ToString("D", new CultureInfo("fr-FR"))),
            Test("addDays(timestamp, 1, '')", "16/03/2018 13:00:00"),
            Test("toUpper('lowercase')", "LOWERCASE"),
            Test("toLower('I AM WHAT I AM')", "i am what i am"),
            Test("string(100.1)", "100,1"),
            Test("sentenceCase('a')", "A"),
            Test("sentenceCase('abc')", "Abc"),
            Test("sentenceCase('aBC')", "Abc"),
            Test("titleCase('a')", "A"),
            Test("titleCase('abc dEF')", "Abc Def")
        };

        public static object[] Test(string input, object value, HashSet<string> paths = null) => new object[] { input, value, paths };

        public static bool IsNumber(object value) =>
           value is sbyte
           || value is byte
           || value is short
           || value is ushort
           || value is int
           || value is uint
           || value is long
           || value is ulong
           || value is float
           || value is double
           || value is decimal;

        [Theory]
        [MemberData(nameof(Data))]
        public void Evaluate(string input, object expected, HashSet<string> expectedRefs)
        {
            var parsed = Expression.Parse(input);
            Assert.NotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(scope);
            Assert.Null(msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.True(expectedRefs.SetEquals(actualRefs));
            }

            // ToString re-parse
            var newExpression = Expression.Parse(parsed.ToString());
            var newActual = newExpression.TryEvaluate(scope).value;
            AssertObjectEquals(actual, newActual);
        }

        [Theory]
        [MemberData(nameof(DataForThreadLocale))]
        public void EvaluateWithLocale(string input, object expected, HashSet<string> expectedRefs)
        {
            var parsed = Expression.Parse(input);
            Assert.NotNull(parsed);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var opts = new Options() { Locale = "fr-FR" };
            var (actual, msg) = parsed.TryEvaluate(scopeForThreadLocale, opts);
            Assert.Null(msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.True(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} acutal: {string.Join(',', actualRefs)}");
            }

            // ToString re-parse
            var newExpression = Expression.Parse(parsed.ToString());
            var newActual = newExpression.TryEvaluate(scopeForThreadLocale, opts).value;
            AssertObjectEquals(actual, newActual);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void EvaluateInOtherCultures(string input, object expected, HashSet<string> expectedRefs)
        {
            var cultureList = new List<string>() { "de-DE", "fr-FR", "es-ES" };
            foreach (var newCultureInfo in cultureList)
            {
                var originalCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo(newCultureInfo);
                var parsed = Expression.Parse(input);
                Assert.NotNull(parsed);
                var (actual, msg) = parsed.TryEvaluate(scope);
                Assert.Null(msg);
                AssertObjectEquals(expected, actual);
                if (expectedRefs != null)
                {
                    var actualRefs = parsed.References();
                    Assert.True(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} actual: {string.Join(',', actualRefs)}");
                }

                // ToString re-parse
                var newExpression = Expression.Parse(parsed.ToString());
                var newActual = newExpression.TryEvaluate(scope).value;
                AssertObjectEquals(actual, newActual);

                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void EvaluateJson(string input, object expected, HashSet<string> expectedRefs)
        {
            var jsonScope = JToken.FromObject(scope);
            var parsed = Expression.Parse(input);
            Assert.NotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(jsonScope);
            Assert.Null(msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.True(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} acutal: {string.Join(',', actualRefs)}");
            }
        }

        [Fact]
        public void TestAccumulatePath()
        {
            var memory = new SimpleObjectMemory(new
            {
                f = "foo",
                b = "bar",
                z = new
                {
                    z = "zar"
                },
                n = 2
            });

            // normal case, note, we doesn't append a " yet
            var exp = Expression.Parse("a[f].b[n].z");
            var (path, left, err) = FunctionUtils.TryAccumulatePath(exp, memory, null);
            Assert.Equal("a['foo'].b[2].z", path);

            // normal case
            exp = Expression.Parse("a[z.z][z.z].y");
            (path, left, err) = FunctionUtils.TryAccumulatePath(exp, memory, null);
            Assert.Equal("a['zar']['zar'].y", path);

            // normal case
            exp = Expression.Parse("a.b[z.z]");
            (path, left, err) = FunctionUtils.TryAccumulatePath(exp, memory, null);
            Assert.Equal("a.b['zar']", path);

            // stop evaluate at middle
            exp = Expression.Parse("json(x).b");
            (path, left, err) = FunctionUtils.TryAccumulatePath(exp, memory, null);
            Assert.Equal("b", path);
        }

        [Fact]
        public void TestTryEvaluateOfT()
        {
            AssertResult<bool>("true", true);
            AssertResult<bool>("false", false);
            AssertResult<string>("'this is a test'", "this is a test");
            AssertResult<byte>(byte.MaxValue.ToString(), byte.MaxValue);
            AssertResult<short>(short.MaxValue.ToString(), short.MaxValue);
            AssertResult<int>(int.MaxValue.ToString(), int.MaxValue);
            AssertResult<long>(int.MaxValue.ToString(), int.MaxValue);
            AssertResult<ushort>(ushort.MaxValue.ToString(), ushort.MaxValue);
            AssertResult<uint>(uint.MaxValue.ToString(), uint.MaxValue);
            AssertResult<ulong>(uint.MaxValue.ToString(), uint.MaxValue);
            AssertResult<float>(15.32322F.ToString(CultureInfo.InvariantCulture), 15.32322F);
            AssertResult<double>(15.32322.ToString(CultureInfo.InvariantCulture), 15.32322);
        }

        [Fact]
        public void TestEvaluationOptions()
        {
            var mockMemory = new Dictionary<string, object>();

            var options = new Options
            {
                NullSubstitution = (path) => $"{path} is undefined"
            };

            object value = null;
            string error = null;

            // normal case null value is substituted
            var exp = Expression.Parse("foo");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals("foo is undefined", value);

            // in boolean context, substitution is not allowed, use raw value instead
            exp = Expression.Parse("if(foo, 1, 2)");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(2, value);

            // in boolean context, substitution is not allowed, use raw value instead
            exp = Expression.Parse("foo && true");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(false, value);

            // in boolean context, substitution is not allowed, use raw value instead
            exp = Expression.Parse("foo || true");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(true, value);

            // in boolean context, substitution is not allowed, use raw value instead
            exp = Expression.Parse("foo == 'foo is undefined'");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(false, value);

            // in boolean context, substitution is not allowed, use raw value instead
            exp = Expression.Parse("bool(foo)");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(false, value);

            // in boolean context, substitution is not allowed, use raw value instead
            exp = Expression.Parse("not(foo)");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(true, value);

            // concat is evaluated in boolean context also, use raw value
            exp = Expression.Parse("if(concat(foo, 'bar'), 1, 2)");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            AssertObjectEquals(1, value);

            // index is not boolean context, but it also requires raw value
            exp = Expression.Parse("a[b]");
            (value, error) = exp.TryEvaluate(mockMemory, options);
            Assert.True(error != null);
        }

        private void AssertResult<T>(string text, T expected)
        {
            var memory = new object();
            var (result, error) = Expression.Parse(text).TryEvaluate<T>(memory);
            Assert.Equal(expected, result);
            Assert.Null(error);
        }

        private void AssertObjectEquals(object expected, object actual)
        {
            if (IsNumber(actual) && IsNumber(expected))
            {
                if (actual is int || actual is long)
                {
                    Assert.True(expected is int || expected is long);
                    Assert.Equal(Convert.ToInt64(expected), Convert.ToInt64(actual));
                }
                else
                {
                    Assert.True(Convert.ToSingle(actual) == Convert.ToSingle(expected));
                }
            }

            // Compare two lists
            else if (expected is IList expectedList
                && actual is IList actualList)
            {
                Assert.Equal(expectedList.Count, actualList.Count);
                for (var i = 0; i < expectedList.Count; i++)
                {
                    AssertObjectEquals(ResolveValue(expectedList[i]), ResolveValue(actualList[i]));
                }
            }
            else
            {
                Assert.Equal(expected, actual);
            }
        }

        private object ResolveValue(object obj)
        {
            object value;
            if (!(obj is JValue jval))
            {
                value = obj;
            }
            else
            {
                value = jval.Value;
                if (jval.Type == JTokenType.Integer)
                {
                    value = jval.ToObject<int>();
                }
                else if (jval.Type == JTokenType.String)
                {
                    value = jval.ToObject<string>();
                }
                else if (jval.Type == JTokenType.Boolean)
                {
                    value = jval.ToObject<bool>();
                }
                else if (jval.Type == JTokenType.Float)
                {
                    value = jval.ToObject<float>();
                }
            }

            return value;
        }

        private class A
        {
            public A(string name)
            {
                this.Name = name;
            }

            public string Name { get; set; }
        }
    }
}
