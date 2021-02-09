#pragma warning disable SA1124 // Do not use regions
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AdaptiveExpressions.Tests
{
    public class BadExpressionTests
    {
        public static IEnumerable<object[]> SyntaxErrorExpressions => new[]
        {
            Test("hello world"),
            Test("a+"),
            Test("a+b*"),
            Test("fun(a, b, c"),
            Test("func(A,b,b,)"),
            Test("\"hello'"),
            Test("'hello'.length()"), // not supported currently
            Test("user.lists.{dialog.listName}"),
            Test("`hi` world")
        };

        public static IEnumerable<object[]> BadExpressions => new[]
        {
            #region General test
            Test("length(func())"), // no such function in children
            Test("func()"), // no such func
            Test("a.func()"), // no such function
            Test("(1.foreach)()"), // error func
            Test("('str'.foreach)()"), // error func
            #endregion

            #region Operators test
            Test("istrue + 1"), // params should be number or string
            Test("one + two + nullObj"), // Operator '+' or add cannot be applied to operands of type 'number' and null object.
            Test("'1' * 2"), // params should be number
            Test("'1' - 2"), // params should be number
            Test("'1' / 2"), // params should be number
            Test("'1' % 2"), // params should be number
            Test("'1' ^ 2"), // params should be number
            Test("1/0"), // $ can not divide 0
            #endregion
            
            #region String functions test
            Test("length(one, 1)"), // length can only have one param
            Test("length(replace(hello))"), // children func error
            Test("replace(hello)"), // replace need three parameters
            Test("replace(one, 'l', 'k')"), // replace only accept string parameter
            Test("replace('hi', 1, 'k')"), // replace only accept string parameter
            Test("replace('hi', 'l', 1)"), // replace only accept string parameter
            Test("replace('hi', '', 1)"), // oldValue cannot be null or string.empty
            Test("replaceIgnoreCase(hello)"), // replaceIgnoreCase need three parameters
            Test("replaceIgnoreCase('hi', '', 1)"), // oldValue cannot be null or string.empty
            Test("replaceIgnoreCase(one, 'l', 'k')"), // replaceIgnoreCase only accept string parameter
            Test("replaceIgnoreCase('hi', 1, 'k')"), // replaceIgnoreCase only accept string parameter
            Test("replaceIgnoreCase('hi', 'l', 1)"), // replaceIgnoreCase only accept string parameter
            Test("split(hello, 'a', 'b')"), // split need one or two parameters
            Test("split(one, 'l')"), // split only accept string parameter
            Test("split(hello, 1)"), // split only accept string parameter
            Test("substring(hello, 0.5)"), // the second parameter of substring must be integer
            Test("substring(hello, 10)"), // the start index is out of the range of the string length
            Test("substring(hello, 0, hello)"), // length is not integer
            Test("substring(hello, 0, 10)"), // the length of substring is out of the range of the original string
            Test("substring(hello, 0, 'hello')"), // length is not integer
            Test("toLower(one)"), // the parameter of toLower must be string
            Test("toLower('hi', 1)"), // the second argument must be a locale string
            Test("toLower('hi', 1)"), // should have 1 param
            Test("toLower('hi', locale, 1)"), // should have 1 or 2 params
            Test("toUpper(one)"), // the parameter of toUpper must be string
            Test("toUpper('hi', 1)"), // the second argument must be a locale string
            Test("toUpper('hi', locale, 1)"), // should have 1 or 2 params
            Test("trim(one)"), // the parameter of trim must be string
            Test("trim('hi', 1)"), // should have 1 param
            Test("endsWith(hello, one)"), // should have string params
            Test("endsWith(one, hello)"), // should have string params
            Test("endsWith(hello)"), // should have two params
            Test("startsWith(hello, one)"), // should have string params
            Test("startsWith(one, hello)"), // should have string params
            Test("startsWith(hello)"), // should have two params
            Test("countWord(hello, 1)"), // should have one param
            Test("countWord(one)"), // should have string param
            Test("countWord(one)"), // should have string param
            Test("addOrdinal(one)"), // should have Integer param
            Test("addOrdinal(one, two)"), // should have one param
            Test("addOrdinal(122332132123123123)"), // should have a 32-bit signed integer
            Test("newGuid(one)"), // should have no parameters
            Test("EOL(one)"), // should have no parameters
            Test("indexOf(hello)"), // should have two parameters
            Test("indexOf(hello, world, one)"), // should have two parameters
            Test("indexOf(hello, one)"), // second parameter should be string
            Test("indexOf(one, hello)"), // first parameter should be list or string
            Test("lastIndexOf(hello)"), // should have two parameters
            Test("lastIndexOf(hello, world, one)"), // should have two parameters
            Test("lastIndexOf(hello, one)"), // second parameter should be string
            Test("lastIndexOf(one, hello)"), // first parameter should be list or string
            Test("sentenceCase(hello, locale, hello)"), // should have 1 or 2 parameters
            Test("sentenceCase(one)"), // first parameter should be string
            Test("sentenceCase(hello, one)"), // the second parameter should be locale string
            Test("titleCase(hello, locale, hello)"), // should have 1 or 2 parameters
            Test("titleCase(one)"), // first parameter should be string
            Test("titleCase(hello, one)"), // the second parameter should be locale string
            
            //Test("titleCase(hello, hello)"), // On Mac OS, a wrong locale string won't throw an error.
            #endregion

            #region Logical comparison functions test
            Test("greater(one, hello)"), // string and integer are not comparable
            Test("greater(one)"), // greater need two parameters
            Test("greaterOrEquals(one, hello)"), // string and integer are not comparable
            Test("greaterOrEquals(one)"), // function need two parameters
            Test("less(1, true)"), // should have rge same type
            Test("less(json('{}'), [])"), // should be comparable
            Test("less(one, hello)"), // string and integer are not comparable
            Test("less(one)"), // function need two parameters
            Test("lessOrEquals(one, hello)"), // string and integer are not comparable
            Test("lessOrEquals(one)"), // function need two parameters
            Test("equals(one)"), // equals must accept two parameters
            Test("exists(1, 2)"), // function need one parameter
            Test("not(false, one)"), // function need one parameter
            #endregion

            #region Conversion functions test
            Test("float(hello)"), // param shoud be float format string
            Test("float(hello, 1)"), // shold have 1 param
            Test("int(hello)"), // param shoud be int format string
            Test("int(1, 1)"), // shold have 1 param
            Test("string(hello, 1)"), // shold have 1 param
            Test("bool(false, 1)"), // shold have 1 param
            Test("array(hello,world)"), // shold have 1 param
            Test("array(one)"), // shold have 1 param
            Test("DataUri(hello, world)"), // shoule have 1 param
            Test("DataUri(false)"), // should have string param
            Test("uriComponent(hello, world)"), // shoule have 1 param
            Test("uriComponent(false)"), // should have string param
            Test("uriComponentToString(hello, world)"), // shoule have 1 param
            Test("uriComponentToString(false)"), // should have string param
            Test("dataUriToBinary(hello, world)"), // shoule have 1 param
            Test("dataUriToBinary(false)"), // should have string param
            Test("dataUriToString(hello, world)"), // shoule have 1 param
            Test("dataUriToString(false)"), // should have string param
            Test("binary(hello, world)"),      // shoule have 1 param
            Test("binary(one)"), // should have string param
            Test("base64(hello, world)"),      // shoule have 1 param
            Test("base64(createArray('a', 'b')"), // should have string param or byte array
            Test("base64ToBinary(hello, world)"), // shoule have 1 param
            Test("base64ToBinary(one)"), // should have string param
            Test("base64ToString(hello, world)"), // shoule have 1 param
            Test("base64ToString(false)"), // should have string param
            Test("formatNumber(1,2,3)"), // invalid locale type
            Test("formatNumber(1,2,'dlkj'"), // invalid locale
            Test("formatNumber(1,2.0)"), // the second parameter should be an integer
            Test("formatNumber(hello,2.0)"), // the first parameter should be a number
            Test("formatNumber(hello,2232131231231)"), // the first parameter should be a 32-bit signed integer
            Test("jsonStringify(hello,2232131231231)"), // shoule have 1 param
            #endregion

            #region Math functions test
            Test("max(hello, one)"), // param should be number
            Test("max()"), // function need 1 or more than 1 parameters
            Test("min(hello, one)"), // param should be number
            Test("min()"), // function need 1 or more than 1 parameters
            Test("add(istrue, 2)"), // param should be number or string
            Test("add()"), // arg count doesn't match
            Test("add(one)"), // add function need 2 or more than two parameters
            Test("sub(hello, 2)"), // param should be number
            Test("sub()"), // arg count doesn't match
            Test("sub(five, six)"), // no such variables
            Test("sub(one)"), // sub function need 2 or more than two parameters
            Test("mul(hello, one)"), // param should be number
            Test("mul(one)"), // mul function need 2 or more than two parameters
            Test("div(one, 0)"), // one cannot be divided by zero
            Test("div(one)"), // // div function need 2 or more than two parameters
            Test("div(hello, one)"), // string hello cannot be divided
            Test("exp(2, hello)"), // exp cannot accept parameter of string
            Test("mod(1, 0)"), // mod cannot accept zero as the second parameter
            Test("mod(5.5, 2)"), //  param should be integer
            Test("mod(5, 2.1)"), //  param should be integer
            Test("mod(5, 2.1 ,3)"), //  need two params
            Test("rand(5)"), //  need two params
            Test("rand(7, 6)"), //  minvalue cannot be greater than maxValue
            Test("rand(21223123221322, 10)"), // the first parameter should be a 32-bit signed integer
            Test("rand(10, 21223123221322)"), // the first parameter should be a 32-bit signed integer
            Test("sum(items)"), //  should have number parameters
            Test("range(hello,one)"), // params should be integer
            Test("range(one,0)"), // the second param should be more than 0
            Test("floor(hello)"), // should have a number parameter
            Test("floor(1.2, 2.1)"), // should have one parameter
            Test("ceiling(hello)"), // should have a number parameter
            Test("ceiling(1.2, 2.1)"), // should have one parameter
            Test("round(hello)"), // should have number parameters
            Test("round(1.2, hello)"), // should have a number as the 2nd parameter
            Test("round(1.2, 2.1)"), // should have integer as the 2nd parameter
            Test("round(1.2, -2)"), // the 2nd parameter should not less than 0
            Test("round(1.2, 16)"), // the 2nd parameter should not greater than 15
            Test("round(1.2, 2, 3)"), // should have one or two number parameters
            Test("round(1.2, 21232123123123123)"), // the second parameter should be a 32-bit signed integer
            Test("abs()"), // should have one parameters
            Test("abs(hello)"), // should have one number parameters
            Test("sqrt()"), // should have one parameters
            Test("sqrt(hello)"), // should have one number parameters
            Test("sqrt(-1)"), // should have one non-nagitive number parameters
            #endregion
            
            #region Date and time function test
            Test("isDefinite(12345)"), // should hava a string or a TimexProperty parameter
            Test("isDefinite('world', 123445)"), // should have only one parameter
            Test("isTime(123445)"), // should hava a string or a TimexProperty parameter
            Test("isTime('world', 123445)"), // should have only one parameter
            Test("isDuration(123445)"), // should hava a string or a TimexProperty parameter
            Test("isDuration('world', 123445)"), // should have only one parameter
            Test("isDate(123445)"), // should hava a string or a TimexProperty parameter
            Test("isDate('world', 123445)"), // should have only one parameter
            Test("isTimeRange(123445)"), // should hava a string or a TimexProperty parameter
            Test("isTimeRange('world', 123445)"), // should have only one parameter
            Test("isDateRange(123445)"), // should hava a string or a TimexProperty parameter
            Test("isDateRange('world', 123445)"), // should have only one parameter
            Test("isPresent(123445)"), // should hava a string or a TimexProperty parameter
            Test("isPresent('world', 123445)"), // should have only one parameter
            Test("addDays('errortime', 1)"), // error datetime format
            Test("addDays(timestamp, 'hi')"), // second param should be integer
            Test("addDays(timestamp)"), // should have 2 or 3 or 4 params
            Test("addDays(timestamp, 1, 'D', locale, hello)"), // should have 2 or 3 or 4 params
            Test("addDays(timestamp, 12345678901234)"), // the second parameter should be a 32-bit signed integer
            Test("addDays(notISOTimestamp, 1)"), // not ISO datetime format
            Test("addHours('errortime', 1)"), // error datetime format
            Test("addHours(timestamp, 'hi')"), // second param should be integer
            Test("addHours(timestamp)"), // should have 2 or 3 params
            Test("addHours(timestamp, 1,'yyyy', locale, 2)"), // should have 2 or 3 or 4 params
            Test("addHours(timestamp, 12345678901234)"), // the second parameter should be a 32-bit signed integer
            Test("addHours(notISOTimestamp, 1)"), // not ISO datetime format
            Test("addMinutes('errortime', 1)"), // error datetime format
            Test("addMinutes(timestamp, 'hi')"), // second param should be integer
            Test("addMinutes(timestamp)"), // should have 2 or 3 params
            Test("addMinutes(timestamp, 1,'yyyy', locale, 2)"), // should have 2 or 3 or 4 params
            Test("addMinutes(timestamp, 12345678901234)"), // the second parameter should be a 32-bit signed integer
            Test("addMinutes(notISOTimestamp, 1)"), // not ISO datetime format
            Test("addSeconds('errortime', 1)"), // error datetime format
            Test("addSeconds(timestamp, 'hi')"), // second param should be integer
            Test("addSeconds(timestamp)"), // should have 2 or 3 or 4 params
            Test("addSeconds(timestamp, 1, 'yyyy', locale, 2)"), // should have 2 or 3 or 4 params
            Test("addSeconds(notISOTimestamp, 1)"), // not ISO datetime format
            Test("addSeconds(timestamp, 12345678901234)"), // the second parameter should be a 32-bit signed integer
            Test("dayOfMonth('errortime')"), // error datetime format
            Test("dayOfMonth(timestamp, 1)"), // should have 1 param
            Test("dayOfMonth(notISOTimestamp)"), // not ISO datetime format
            Test("dayOfWeek('errortime')"), // error datetime format
            Test("dayOfWeek(timestamp, 1)"), // should have 1 param
            Test("dayOfWeek(notISOTimestamp)"), // not ISO datetime format
            Test("dayOfYear('errortime')"), // error datetime format
            Test("dayOfYear(timestamp, 1)"), // should have 1 param
            Test("dayOfYear(notISOTimestamp)"), // not ISO datetime format
            Test("month('errortime')"), // error datetime format
            Test("month(timestamp, 1)"), // should have 1 param
            Test("month(notISOTimestamp)"), // not ISO datetime format
            Test("date('errortime')"), // error datetime format
            Test("date(timestamp, 1)"), // should have 1 param
            Test("date(notISOTimestamp)"), // not ISO datetime format
            Test("year('errortime')"), // error datetime format
            Test("year(timestamp, 1)"), // should have 1 param
            Test("year(notISOTimestamp)"), // not ISO datetime format
            Test("formatDateTime('errortime')"), // error datetime format
            Test("formatDateTime(notValidTimestamp)"), // error datetime format
            Test("formatDateTime(notValidTimestamp2)"), // error datetime format
            Test("formatDateTime(notValidTimestamp3)"), // error datetime format
            Test("formatDateTime({})"), // error valid datetime
            Test("formatDateTime(timestamp, 1)"), // invalid format string
            Test("formatDateTime(timestamp, 'yyyy', locale, hello)"), // should have 1 or 2 or 3params
            Test("formatEpoch('time')"), // error string
            Test("formatEpoch(timestamp, 'yyyy', locale, 1)"), // should have 1 or 2 or 3 params
            Test("formatTicks('string')"), // String is not valid
            Test("formatTicks(2.3)"), // float is not valid
            Test("formatTicks({})"), // object is not valid
            Test("formatTicks({})"), // object is not valid
            Test("formatTicks(9809789278, 'yyyy', locale, 1)"), // should have 1 or 2 or 3 params
            Test("subtractFromTime('errortime', 'yyyy', 1)"), // error datetime format
            Test("subtractFromTime(timestamp, 1, 'W')"), // error time unit
            Test("subtractFromTime(timestamp, timestamp, 'W')"), // error parameters format
            Test("subtractFromTime(timestamp, '1', 'Year')"), // second param should be integer
            Test("subtractFromTime(timestamp, 'yyyy')"), // should have 3 or 4 or 5 params
            Test("subtractFromTime(timestamp, '1', 'Year', 'yyyy', locale, hello)"), // should have 3 or 4 or 5 params
            Test("subtractFromTime(notISOTimestamp, 1, 'Year')"), // not ISO datetime format
            Test("dateReadBack('errortime', 'errortime')"), // error datetime format
            Test("dateReadBack(timestamp)"), // shold have two params
            Test("dateReadBack(notISOTimestamp, addDays(timestamp, 1))"), // not ISO datetime format
            Test("getTimeOfDay('errortime')"), // error datetime format
            Test("getTimeOfDay(timestamp, timestamp)"), // should have 1 param
            Test("getTimeOfDay(notISOTimestamp)"), // not ISO datetime format
            Test("getPastTime(1, 'W')"), // error time unit
            Test("getPastTime(timestamp, 'W')"), // error parameters format
            Test("getPastTime('yyyy', '1')"), // second param should be integer
            Test("getPastTime('yyyy')"), // should have 2 or 3 or 4 params
            Test("getPastTime(1, 'day', 'D', locale, hello)"), // should have 2 or 3 or 4 params
            Test("getFutureTime(1, 'W')"), // error time unit
            Test("getFutureTime(timestamp, 'W')"), // error parameters format
            Test("getFutureTime('yyyy', '1')"), // second param should be integer
            Test("getFutureTime('yyyy')"), // should have 2 or 3 or 4 params
            Test("getFutureTime(1, 'day', 'D', locale, hello)"), // should have 2 or 3 or 4 params
            Test("utcNow('D', locale, hello)"), // should have 0 or 1 or 2 params
            Test("convertFromUTC(notValidTimestamp, timezone)"), // not valid iso timestamp
            Test("convertFromUTC(timestamp, invalidTimezone,'D')"), // not valid timezone
            Test("convertFromUTC(timestamp, timezone, 'a')"),  // not valid format 
            Test("convertFromUTC(timestamp, timezone, 'D', locale, hello)"),  // should have 2 or 3 or 4 params
            Test("convertToUTC(notValidTimestamp, timezone)"), // not valid timestamp
            Test("convertToUTC(timestamp, invalidTimezone, 'D')"), // not valid timezone
            Test("convertToUTC(timestamp, timezone, 'a')"),  // not valid format 
            Test("convertToUTC(timestamp, timezone, 'D', locale, hello)"),  // should have 2 or 3 or 4 params
            Test("addToTime(notValidTimeStamp, one, 'day')"), // not valid timestamp
            Test("addToTime(timeStamp, hello, 'day')"), // interval should be integer
            Test("addToTime(timeStamp, one, 'decade', 'D')"), // not valid time unit 
            Test("addToTime(timeStamp, one, 'week', 'D')"), // not valid format
            Test("addToTime(timeStamp, one, 'week', 'D', locale, one)"), // should have 3 or 4 or 5 params
            Test("startOfDay(notValidTimeStamp)"), // not valid timestamp
            Test("startOfDay(timeStamp, 'A')"), // not valid format
            Test("startOfDay(timeStamp, 'D', 'de-DE', hello)"), // should have 1 or 2 or 3 params
            Test("startOfHour(notValidTimeStamp)"), // not valid timestamp
            Test("startOfHour(timeStamp, 'A')"), // not valid format
            Test("startOfHour(timeStamp, 'D', 'de-DE', hello)"), // should have 1 or 2 or 3 params
            Test("startOfMonth(notValidTimeStamp)"), // not valid timestamp
            Test("startOfMonth(timeStamp, 'A')"), // not valid format
            Test("startOfMonth(timeStamp, 'D', 'de-DE', hello)"), // should have 1 or 2 or 3 params
            Test("ticks(notValidTimeStamp)"), // not valid timestamp
            Test("ticksToDays(12.12)"), // not an integer
            Test("ticksToHours(timestamp)"), // not an integer
            Test("ticksToMinutes(timestamp)"), // not an integer
            Test("dateTimeDiff(notValidTimeStamp,'2018-01-01T08:00:00.000Z')"), // the first parameter is not a valid timestamp
            Test("dateTimeDiff('2017-01-01T08:00:00.000Z',notValidTimeStamp)"), // the second parameter is not a valid timestamp
            Test("dateTimeDiff('2017-01-01T08:00:00.000Z','2018-01-01T08:00:00.000Z', 'years')"), // should only have 2 parameters
            Test("getNextViableDate(hello)"), // should have a "XXXX-MM-DD" format string
            Test("getNextViableDate(one)"), // should have a string parameter
            Test("getNextViableDate('XXXX-10-10', 20)"), // should only have 1 parameter
            Test("getPreviousViableDate(hello)"), // should have a "XXXX-MM-DD" format string
            Test("getPreviousViableDate(one)"), // should have a string parameter
            Test("getPreviousViableDate('XXXX-10-10', 20)"), // should only have 1 parameter
            Test("getNextViableTime(hello)"), // should have a "XX:mm:ss" format string
            Test("getNextViableTime(one)"), // should have a string parameter
            Test("getNextViableTime('XX:12:12', 20)"), // should only have 1 parameter
            Test("getPreviousViableTime(hello)"), // should have a "XX:mm:ss" format string
            Test("getPreviousViableTime(one)"), // should have a string parameter
            Test("getPreviousViableTime('XX:12:12', 20)"), // should only have 1 parameter
            Test("resolve(one)"), // should have string or TimexProperty arguments
            Test("resolve('T14', 'Asia/Tokyo')"), // should only have one parameter
            Test("resolve('12-20')"), // should have a valid TimexPropterty after parsing
            Test("resolve('XXXX-WXX-6')"), // not a valid argument
            #endregion

            #region uri parsing function test
            Test("uriHost(relatibeUri)"),
            Test("uriPath(relatibeUri)"),
            Test("uriPathAndQuery(relatibeUri)"),
            Test("uriPort(relatibeUri)"),
            Test("uriQuery(relatibeUri)"),
            Test("uriScheme(relatibeUri)"),
            #endregion

            #region collection functions test
            Test("sum(items, 'hello')"), // should have 1 parameter
            Test("sum('hello')"), // first param should be list
            Test("average(items, 'hello')"), // should have 1 parameter
            Test("average('hello')"), // first param should be list
            Test("average(hello)"), // first param should be list
            Test("contains('hello world', 'hello', 'new')"), // should have 2 parameter
            Test("count(items, 1)"), // should have 1 parameter
            Test("count(1)"), // first param should be list or string
            Test("reverse(items, 1)"), // should have 1 parameter
            Test("reverse(1)"), // first param should be list or string
            Test("empty(1,2)"), // should have two params
            Test("first(items,2)"), // should have 1 param
            Test("last(items,2)"), // should have 1 param
            Test("join(items, 'p1', 'p2','p3')"), // builtin function should have 2-3 params, 
            Test("join(hello, 'hi')"), // first param must list
            Test("join(items, 1)"), // second param must string 
            Test("join(items, '1', 2)"), // third param must string 
            Test("foreach(hello, item, item)"), // first arg is not list or structure object
            Test("foreach(items, item)"), // should have three parameters
            Test("foreach(items, item, item2, item3)"), // should have three parameters
            Test("foreach(items, add(1), item)"), // Second paramter of foreach is not an identifier
            Test("foreach(items, 1, item)"), // Second paramter error
            Test("foreach(items, x, sum(x))"), // third paramter error
            Test("select(hello, item, item)"), // first arg is not list
            Test("select(items, item)"), // should have three parameters
            Test("select(items, item, item2, item3)"), // should have three parameters
            Test("select(items, add(1), item)"), // Second paramter of foreach is not an identifier
            Test("select(items, 1, item)"), // Second paramter error
            Test("select(items, x, sum(x))"), // third paramter error
            Test("where(hello, item, item)"), // first arg is not list or structure
            Test("where(items, item)"), // should have three parameters
            Test("where(items, item, item2, item3)"), // should have three parameters
            Test("where(items, add(1), item)"), // Second paramter of where is not an identifier
            Test("where(items, 1, item)"), // Second paramter error
            Test("indicesAndValues(items, 1)"), // only one param
            Test("indicesAndValues(1)"), // shoud have array param
            Test("union(one, two)"), // should have collection param
            Test("intersection(one, two)"), // should have collection param
            Test("skip(one, two)"), // should have collection param
            Test("take(one, two)"), // should have collection param
            Test("take(createArray('H','e','l','l','0'),items[5])"), // the second param expr is wrong
            Test("subArray(one,1,4)"), // should have collection param
            Test("subArray(items,-1,4)"), // the second parameter shoule not less than zero
            Test("subArray(items,1,4)"), // the second parameter shoule  less than the length of the collection
            Test("subArray(createArray('H','e','l','l','o'),items[5],5)"), // the second parameter expression is invalid
            Test("subArray(createArray('H','e','l','l','o'),2,items[5])"), // the second parameter expression is invalid
            Test("sortBy(hello, 'x')"), // first param should be list
            Test("sortBy(createArray('H','e','l','l','o'), 1)"), // second param should be string
            Test("sortBy(createArray('H','e','l','l','o'), 'x', hi)"), // second param should be string
            Test("flatten(createArray(1,createArray(2),createArray(createArray(3, 4), createArray(5,6))), 123456789012345)"), // the second parameter should be a 32-bit signed integer
#endregion

            #region Object manipulation and construction functions test
            Test("json(1,2)"), // should have 1 parameter
            Test("json(1)"), // should be string parameter
            Test("json('{\"key1\":value1\"}')"), // invalid json format string 
            Test("addProperty(json('{\"key1\":\"value1\"}'), 'key2','value2','key3')"), // should have 3 parameter
            Test("addProperty(json('{\"key1\":\"value1\"}'), 1,'value2')"), // second param should be string
            Test("addProperty(json('{\"key1\":\"value1\"}'), 'key1', 3)"), // cannot add existing property
            Test("setProperty(json('{\"key1\":\"value1\"}'), 'key2','value2','key3')"), // should have 3 parameter
            Test("setProperty(json('{\"key1\":\"value1\"}'), 1,'value2')"), // second param should be string
            Test("removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), 1))"), // second param should be string
            Test("removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), '1', '2'))"), // should have 2 parameters
            Test("coalesce()"), // should have at least 1 parameter
            Test("xPath(invalidXml, ''sum(/produce/item/count)')"), // not a valid xml
            Test("xPath(xmlStr)"), // should have two params
            Test("xPath(xmlStr, 'getTotal')"), // invalid xpath query
            Test("jPath(hello,'Manufacturers[0].Products[0].Price')"), // not a valid json
            Test("jPath(hello,'Manufacturers[0]/Products[0]/Price')"), // not a valid path
            Test("jPath(jsonStr,'$..Products[?(@.Price >= 100)].Name')"), // no matched node
            Test("merge(json(json1))"), // should have at least two arguments
            Test("merge(json(json1), json(jarray1))"), // arguments should all be JSON objects
            Test("merge(json(jarray1), json(json1))"), // arguments should all be JSON objects
            #endregion

            #region Memory access test
            Test("getProperty(bag, 1)"), // second param should be string
            Test("getProperty(1)"), // if getProperty contains only one parameter, the parameter should be string
            Test("Accessor(1)"), // first param should be string
            Test("Accessor(bag, 1)"), // second should be object
            Test("one[0]"),  // one is not list
            Test("items[3]"), // index out of range
            Test("items[one+0.5]"), // index is not integer
            #endregion
            
            #region Regex
            Test("isMatch('^[a-z]+$')"), // should have 2 parameter
            Test("isMatch('abC', one)"), // second param should be string
            Test("isMatch(1, '^[a-z]+$')"), // first param should be string
            Test("isMatch('abC', '^[a-z+$')"), // bad regular expression
            #endregion

            #region Type Checking
            Test("isString(hello, hello)"), // should have 1 parameter
            Test("isInteger(2, 3)"), // should have 1 parameter
            Test("isFloat(1.2, 3.1)"), // should have 1 parameter
            Test("isArray(createArray(1,2,3), 1)"), // should have 1 parameter
            Test("isObejct(emptyJObject, hello)"), // should have 1 parameter
            Test("isDateTime('2018-03-15T13:00:00.000Z', hello)"), // should have 1 parameter
            Test("isBoolean(false, false)"), // should have 1 parameter
            #endregion

            #region SetPathToValue tests
            Test("setPathToValue(2+3, 4)"), // Not a real path
            Test("setPathToValue(a)"), // Missing value
            #endregion

            #region TriggerTree Tests

            // optional throws because it's a placeholder only interpreted by trigger tree and is removed before evaluation
            Test("optional(true)"), 
            #endregion

            #region StringOrValue
            Test("stringOrValue()"), // should have 1 parameter
            Test("stringOrValue(1)"), // should have string parameter
            Test("stringOrValue('${1/0} item')"), // throw error in evaluation stage
            #endregion
        };

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <returns>object[].</returns>
        public static object[] Test(string input) => new object[] { input };

        [Theory]
        [MemberData(nameof(SyntaxErrorExpressions))]
        public void ParseSyntaxErrors(string exp)
        {
            Assert.Throws<SyntaxErrorException>(() => Expression.Parse(exp));
        }

        [Theory]
        [MemberData(nameof(BadExpressions))]
        public void Evaluate(string exp)
        {
            var passed = false;
            object scope = new
            {
                one = 1.0,
                two = 2.0,
                hello = "hello",
                world = "world",
                istrue = true,
                emptyJObject = new JObject(),
                bag = new
                {
                    three = 3.0,
                    set = new
                    {
                        four = 4.0,
                    },
                    list = new[] { "red", "blue" },
                    index = 3,
                    name = "mybag"
                },
                items = new string[] { "zero", "one", "two" },
                xmlStr = "<?xml version='1.0'?> <produce> <item> <name>Gala</name> <type>apple</type> <count>20</count> </item> <item> <name>Honeycrisp</name> <type>apple</type> <count>10</count> </item> </produce>",
                invalidXml = "<?xml version='1.0'?> <produce> <item> <name>Gala</name> <type>apple</type> <count>20</count> </item> <item> <name>Honeycrisp</name> <type>apple</type> <count>10</count>",
                nestedItems = new[]
                {
                    new { x = 1 },
                    new { x = 2 },
                    new { x = 3 }
                },
                timestamp = "2018-03-15T13:00:00.000Z",
                timestamp2 = "2018-01-01T03:00:00.000Z",
                timezone = "Pacific Standard Time",
                invalidTimeZone = "Local",
                notISOTimestamp = "2018/03/15 13:00:00",
                notValidTimestamp = "2018timestmap",
                notValidTimestamp2 = "1521118800",
                notValidTimestamp3 = "20181115",
                relativeUri = "../catalog/shownew.htm?date=today",
                locale = "en-US",
                json1 = @"{
                          'Enabled': true,
                          'Roles': [ 'User', 'Admin' ]
                        }",
                jarray1 = @"['a', 'b']",
                turn = new
                {
                    recognized = new
                    {
                        entities = new
                        {
                            city = "Seattle"
                        },
                        intents = new
                        {
                            BookFlight = "BookFlight"
                        }
                    }
                },
                dialog = new
                {
                    result = new
                    {
                        title = "Dialog Title",
                        subTitle = "Dialog Sub Title"
                    }
                },
            };

            try
            {
                var (value, error) = Expression.Parse(exp).TryEvaluate(scope);
                if (error != null)
                {
                    passed = true;
                }
            }
            catch
            {
                passed = true;
            }

            Assert.True(passed);
        }
    }
}
