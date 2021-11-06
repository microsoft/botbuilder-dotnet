﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1202 // Elements should be ordered by access
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.BuiltinFunctions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    public class TemplatesTest
    {
        [Fact]
        public void TestBasic()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("2.lg"));

            var evaled = templates.Evaluate("wPhrase");
            var options = new List<string> { "Hi", "Hello", "Hiya" };

            Assert.True(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [Fact]
        public void TestBasicTemplateReference()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("3.lg"));

            var evaled = templates.Evaluate("welcome_user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)" };

            Assert.True(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [Fact]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = templates.Evaluate("welcome_user", new { userName = userName }).ToString();
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.True(evaled.Contains(userName), $"The result {evaled} does not contiain `{userName}`");
        }

        [Fact]
        public void TestIfElseTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("5.lg"));

            string evaled = templates.Evaluate("time_of_day_readout", new { timeOfDay = "morning" }).ToString();
            Assert.True(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = templates.Evaluate("time_of_day_readout", new { timeOfDay = "evening" }).ToString();
            Assert.True(evaled == "Good evening" || evaled == "Evening! ", $"Evaled is {evaled}");
        }

        [Fact]
        public void TestMultiline()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Multiline.lg"));
            string evaled = templates.Evaluate("template1").ToString();
            var generatedTemplates = Templates.ParseText(evaled);
            var result = generatedTemplates.Evaluate("generated1");
            Assert.Equal("hi", result);

            evaled = templates.Evaluate("template2", new { evaluateNow = "please input" }).ToString();
            generatedTemplates = Templates.ParseText(evaled);
            result = generatedTemplates.Evaluate("generated2", new { name = "jack" });
            Assert.Equal("please input jack", result.ToString().Trim());

            evaled = templates.Evaluate("template3").ToString();
            Assert.Equal("markdown\n## Manage the knowledge base\n", evaled.Replace("\r\n", "\n"));

            evaled = templates.Evaluate("template4").ToString();
            Assert.Equal("## Manage the knowledge base", evaled);

            evaled = templates.Evaluate("template5").ToString();
            Assert.Equal(string.Empty, evaled);
        }

        [Fact]
        public void TestBasicConditionalTemplateWithoutDefault()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("5.lg"));

            string evaled = templates.Evaluate("time_of_day_readout_without_default", new { timeOfDay = "morning" }).ToString();
            Assert.True(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = templates.Evaluate("time_of_day_readout_without_default2", new { timeOfDay = "morning" }).ToString();
            Assert.True(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            object evaledNull = templates.Evaluate("time_of_day_readout_without_default2", new { timeOfDay = "evening" });
            Assert.Null(evaledNull);
        }

        [Fact]
        public void TestMultiLineExprInLG()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MultiLineExpr.lg"));

            string evaled = templates.Evaluate("ExprInCondition", new { userName = "Henry", day = "Monday" }).ToString();
            Assert.True(evaled == "Not today", $"Evaled is {evaled}");

            evaled = templates.Evaluate("definition").ToString();
            Assert.True(evaled == "10", $"Evaled is {evaled}");

            evaled = templates.Evaluate("template").ToString();
            Assert.True(evaled == "15", $"Evaled is {evaled}");
        }

        [Fact]
        public void TestExpandText()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ExpandText.lg"));

            var scope = new JObject
            {
                ["@answer"] = "hello ${user.name}",
                ["user"] = new JObject
                {
                    ["name"] = "vivian"
                }
            };

            // - ${length(expandText(@answer))}
            var evaled = templates.Evaluate("template", scope);
            Assert.Equal("hello vivian".Length, evaled);

            // Parse text content
            evaled = templates.EvaluateText("${length(expandText(@answer))}", scope);
            Assert.Equal("hello vivian".Length, evaled);
        }

        [Fact]
        public void TestBasicSwitchCaseTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("switchcase.lg"));

            string evaled = templates.Evaluate("greetInAWeek", new { day = "Saturday" }).ToString();
            Assert.True(evaled == "Happy Saturday!");

            evaled = templates.Evaluate("greetInAWeek", new { day = "Monday" }).ToString();
            Assert.True(evaled == "Work Hard!");
        }

        [Fact]
        public void TestBasicTemplateRefWithParameters()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("6.lg"));
            string evaled = templates.Evaluate("welcome", null).ToString();
            Assert.True(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = templates.Evaluate("welcome", new { userName = "DL" }).ToString();
            Assert.True(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [Fact]
        public void TestBasicListSupport()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("BasicList.lg"));
            Assert.Equal(templates.Evaluate("BasicJoin", new { items = new[] { "1" } }), "1");
            Assert.Equal(templates.Evaluate("BasicJoin", new { items = new[] { "1", "2" } }), "1, 2");
            Assert.Equal(templates.Evaluate("BasicJoin", new { items = new[] { "1", "2", "3" } }), "1, 2 and 3");
        }

        [Fact]
        public void TestBasicExtendedFunctions()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("6.lg"));
            var alarms = new[]
            {
                new
                {
                    time = "7 am",
                    date = "tomorrow"
                },
                new
                {
                    time = "8 pm",
                    date = "tomorrow"
                }
            };

            // var alarmStrs = alarms.Select(x => engine.EvaluateTemplate("ShowAlarm", new { alarm = x })).ToList() ;
            // var evaled = engine.EvaluateTemplate("ShowAlarms", new { alarms = alarmStrs });
            // Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            var evaled = templates.Evaluate("ShowAlarmsWithForeach", new { alarms = alarms });
            Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            evaled = templates.Evaluate("ShowAlarmsWithLgTemplate", new { alarms = alarms });
            Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            evaled = templates.Evaluate("ShowAlarmsWithDynamicLgTemplate", new { alarms = alarms, templateName = "ShowAlarm" });
            Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = templates.EvaluateTemplate("ShowAlarmsWithMemberForeach", new { alarms = alarms });
            // Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = templates.EvaluateTemplate("ShowAlarmsWithHumanize", new { alarms = alarms });
            // Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = templates.EvaluateTemplate("ShowAlarmsWithMemberHumanize", new { alarms = alarms });
            // Assert.Equal("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);
        }

        [Fact]
        public void TestCaseInsensitive()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("CaseInsensitive.lg"));
            var alarms = new[]
            {
                new
                {
                    time = "7 am",
                    date = "tomorrow"
                },
                new
                {
                    time = "8 pm",
                    date = "tomorrow"
                }
            };

            var evaled = templates.Evaluate("ShowAlarms", new { alarms = alarms });
            Assert.Equal("You have two alarms", evaled);

            evaled = templates.Evaluate("greetInAWeek", new { day = "Saturday" });
            Assert.Equal("Happy Saturday!", evaled);
        }

        [Fact]
        public void TestListWithOnlyOneElement()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("8.lg"));
            var evaled = templates.Evaluate("ShowTasks", new { recentTasks = new[] { "Task1" } });
            Assert.Equal("Your most recent task is Task1. You can let me know if you want to add or complete a task.", evaled);
        }

        [Fact]
        public void TestTemplateNameWithDotIn()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("TemplateNameWithDot.lg"));
            Assert.Equal(templates.Evaluate("Hello.World", null), "Hello World");
            Assert.Equal(templates.Evaluate("Hello", null), "Hello World");
        }

        [Fact]
        public void TestMultiLine()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MultilineTextForAdaptiveCard.lg"));
            var evaled1 = templates.Evaluate("wPhrase", string.Empty);
            var options1 = new List<string> { "\r\ncardContent\r\n", "hello", "\ncardContent\n" };
            Assert.True(options1.Contains(evaled1), $"Evaled is {evaled1}");

            var evaled2 = templates.Evaluate("nameTemplate", new { name = "N" });
            var options2 = new List<string> { "\r\nN\r\n", "N", "\nN\n" };
            Assert.True(options2.Contains(evaled2), $"Evaled is {evaled2}");

            var evaled3 = templates.Evaluate("adaptivecardsTemplate", string.Empty);

            var evaled4 = templates.Evaluate("refTemplate", string.Empty);
            var options4 = new List<string> { "\r\nhi\r\n", "\nhi\n" };
            Assert.True(options4.Contains(evaled4), $"Evaled is {evaled4}");
        }

        [Fact]
        public void TestTemplateRef()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("TemplateRef.lg"));

            var scope = new
            {
                time = "morning",
                name = "Dong Lei"
            };
            Assert.Equal(templates.Evaluate("Hello", scope), "Good morning Dong Lei");
        }

        [Fact]
        public void TestEscapeCharacter()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EscapeCharacter.lg"));
            var evaled = templates.Evaluate("wPhrase", null);
            Assert.Equal(evaled, "Hi \r\n\t\\");

            evaled = templates.Evaluate("AtEscapeChar", null);
            Assert.Equal(evaled, "Hi{1+1}[wPhrase]{wPhrase()}${wPhrase()}2${1+1}");

            evaled = templates.Evaluate("otherEscape", null);
            Assert.Equal(evaled, @"Hi \y \");

            evaled = templates.Evaluate("escapeInExpression", null);
            Assert.Equal(evaled, "Hi hello\\\\");

            evaled = templates.Evaluate("escapeInExpression2", null);
            Assert.Equal(evaled, "Hi hello'");

            evaled = templates.Evaluate("escapeInExpression3", null);
            Assert.Equal(evaled, "Hi hello\"");

            evaled = templates.Evaluate("escapeInExpression4", null);
            Assert.Equal(evaled, "Hi hello\"");

            evaled = templates.Evaluate("escapeInExpression5", null);
            Assert.Equal(evaled, "Hi hello\n");

            evaled = templates.Evaluate("escapeInExpression6", null);
            Assert.Equal(evaled, "Hi hello\n");

            var todos = new[] { "A", "B", "C" };
            evaled = templates.Evaluate("showTodo", new { todos });
            Assert.Equal(((string)evaled).Replace("\r\n", "\n"), "\n    Your most recent 3 tasks are\n    * A\n* B\n* C\n    ");
            
            evaled = templates.Evaluate("showTodo", null);
            Assert.Equal(((string)evaled).Replace("\r\n", "\n"), "\n    You don't have any \"t\\\\odo'\".\n    ");

            evaled = templates.Evaluate("getUserName", null);
            Assert.Equal(evaled, "super \"x man\"");

            evaled = templates.Evaluate("structure1", null);
            Assert.Equal(evaled.ToString().Replace("\r\n", "\n").Replace("\n", string.Empty), "{  \"lgType\": \"struct\",  \"list\": [    \"a\",    \"b|c\"  ]}");

            evaled = templates.Evaluate("nestedSample", null);
            Assert.Equal(evaled.ToString(), "i like three movies, they are \"\\\"name1\", \"name2\" and \"{name3\"");

            evaled = templates.Evaluate("dollarsymbol");
            Assert.Equal("$ $ ${'hi'} hi", evaled);
        }

        [Fact]
        public void TestAnalyzer()
        {
            var testData = new object[]
            {
                new
                {
                    name = "orderReadOut",
                    variableOptions = new string[] { "orderType", "userName", "base", "topping", "bread", "meat" },
                    templateRefOptions = new string[] { "wPhrase", "pizzaOrderConfirmation", "sandwichOrderConfirmation" }
                },
                new
                {
                    name = "sandwichOrderConfirmation",
                    variableOptions = new string[] { "bread", "meat" },
                    templateRefOptions = new string[] { }
                },
                new
                {
                    name = "template1",

                    // TODO: input.property should really be: customer.property but analyzer needs to be
                    variableOptions = new string[] { "alarms", "customer", "tasks[0]", "age", "city" },
                    templateRefOptions = new string[] { "template2", "template3", "template4", "template5", "template6" }
                },
                new
                {
                    name = "coffee_to_go_order",
                    variableOptions = new string[] { "coffee", "userName", "size", "price" },
                    templateRefOptions = new string[] { "wPhrase", "LatteOrderConfirmation", "MochaOrderConfirmation", "CuppuccinoOrderConfirmation" }
                },
                new
                {
                    name = "structureTemplate",
                    variableOptions = new string[] { "text", "newText" },
                    templateRefOptions = new string[] { "ST2" }
                },
            };

            foreach (var testItem in testData)
            {
                var templates = Templates.ParseFile(GetExampleFilePath("analyzer.lg"));
                var evaled1 = templates.AnalyzeTemplate(testItem.GetType().GetProperty("name").GetValue(testItem).ToString());
                var variableEvaled = evaled1.Variables;
                var variableEvaledOptions = testItem.GetType().GetProperty("variableOptions").GetValue(testItem) as string[];
                Assert.Equal(variableEvaledOptions.Length, variableEvaled.Count);
                variableEvaledOptions.ToList().ForEach(element => Assert.Equal(variableEvaled.Contains(element), true));
                var templateEvaled = evaled1.TemplateReferences;
                var templateEvaledOptions = testItem.GetType().GetProperty("templateRefOptions").GetValue(testItem) as string[];
                Assert.Equal(templateEvaledOptions.Length, templateEvaled.Count);
                templateEvaledOptions.ToList().ForEach(element => Assert.Equal(templateEvaled.Contains(element), true));
            }
        }

        [Fact]
        public void TestlgTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = templates.Evaluate("TemplateC", string.Empty);
            var options = new List<string> { "Hi", "Hello" };
            Assert.Equal(options.Contains(evaled), true);

            evaled = templates.Evaluate("TemplateD", new { b = "morning" });
            options = new List<string> { "Hi morning", "Hello morning" };
            Assert.Equal(options.Contains(evaled), true);
        }

        [Fact]
        public void TestTemplateAsFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("TemplateAsFunction.lg"));
            string evaled = templates.Evaluate("Test2", string.Empty).ToString();

            Assert.Equal(evaled, "hello world");

            evaled = templates.Evaluate("Test3", string.Empty).ToString();
            Assert.Equal(evaled, "hello world");

            evaled = templates.Evaluate("Test4", string.Empty).ToString();

            Assert.Equal(evaled.Trim(), "hello world");

            evaled = templates.Evaluate("dupNameWithTemplate").ToString();
            Assert.Equal(evaled, "2");

            evaled = templates.Evaluate("foo", new { property = "Show" }).ToString();
            Assert.Equal(evaled, "you made it!");
        }

        [Fact]
        public void TestAnalyzelgTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = templates.AnalyzeTemplate("TemplateD");
            var variableEvaled = evaled.Variables;
            var options = new List<string>() { "b" };
            Assert.Equal(variableEvaled.Count, options.Count);
            options.ForEach(e => Assert.Equal(variableEvaled.Contains(e), true));
        }

        [Fact]
        public void TestImportLgFiles()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("import.lg"));

            // Assert 6.lg is imported only once when there are several relative paths which point to the same file.
            // Assert import cycle loop is handled well as expected when a file imports itself.
            Assert.Equal(14, templates.AllTemplates.Count());

            string evaled = templates.Evaluate("basicTemplate", null).ToString();
            Assert.True(evaled == "Hi" || evaled == "Hello");

            evaled = templates.Evaluate("welcome", null).ToString();
            Assert.True(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = templates.Evaluate("template3", null).ToString();
            Assert.True(evaled == "Hi 2" || evaled == "Hello 2");

            evaled = templates.Evaluate("welcome", new { userName = "DL" }).ToString();
            Assert.True(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");

            evaled = templates.Evaluate("basicTemplate2", null).ToString();
            Assert.True(evaled == "Hi 2" || evaled == "Hello 2");

            // Assert 6.lg of relative path is imported from text.
            templates = Templates.ParseText("[import](./6.lg)\r\n# basicTemplate\r\n- Hi\r\n- Hello\r\n", GetExampleFilePath("xx.lg"));

            Assert.Equal(8, templates.AllTemplates.Count());
            evaled = templates.Evaluate("basicTemplate", null).ToString();
            Assert.True(evaled == "Hi" || evaled == "Hello");

            evaled = templates.Evaluate("welcome", null).ToString();
            Assert.True(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = templates.Evaluate("welcome", new { userName = "DL" }).ToString();
            Assert.True(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [Fact]
        public void TestRegex()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Regex.lg"));
            var evaled = templates.Evaluate("wPhrase", string.Empty);
            Assert.Equal(evaled, "Hi");

            evaled = templates.Evaluate("wPhrase", new { name = "jack" });
            Assert.Equal(evaled, "Hi jack");

            evaled = templates.Evaluate("wPhrase", new { name = "morethanfive" });
            Assert.Equal(evaled, "Hi");
        }

        [Fact]
        public void TestExpandTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            // without scope
            var evaled = templates.ExpandTemplate("FinalGreeting");
            Assert.Equal(4, evaled.Count);
            var expectedResults = new List<string>() { "Hi Morning", "Hi Evening", "Hello Morning", "Hello Evening" };
            expectedResults.ForEach(x => Assert.Equal(true, evaled.Contains(x)));

            // with scope
            evaled = templates.ExpandTemplate("TimeOfDayWithCondition", new { time = "evening" });
            Assert.Equal(2, evaled.Count);
            expectedResults = new List<string>() { "Hi Evening", "Hello Evening" };
            expectedResults.ForEach(x => Assert.Equal(true, evaled.Contains(x)));

            // with scope
            evaled = templates.ExpandTemplate("greetInAWeek", new { day = "Sunday" });
            Assert.Equal(2, evaled.Count);
            expectedResults = new List<string>() { "Nice Sunday!", "Happy Sunday!" };
            expectedResults.ForEach(x => Assert.Equal(true, evaled.Contains(x)));
        }

        [Fact]
        public void TestExpandTemplateWithRef()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var alarms = new[]
            {
                new
                {
                    time = "7 am",
                    date = "tomorrow"
                },
                new
                {
                    time = "8 pm",
                    date = "tomorrow"
                }
            };

            var evaled = templates.ExpandTemplate("ShowAlarmsWithLgTemplate", new { alarms = alarms });
            Assert.Equal(2, evaled.Count);
            Assert.Equal("You have 2 alarms, they are 8 pm at tomorrow", evaled[0]);
            Assert.Equal("You have 2 alarms, they are 8 pm of tomorrow", evaled[1]);
        }

        [Fact]
        public void TestExpandTemplateWithRefInMultiLine()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var alarms = new[]
            {
                new
                {
                    time = "7 am",
                    date = "tomorrow"
                },
                new
                {
                    time = "8 pm",
                    date = "tomorrow"
                }
            };

            var evaled = templates.ExpandTemplate("ShowAlarmsWithMultiLine", new { alarms = alarms });
            Assert.Equal(2, evaled.Count);
            var eval1Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm at tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm at tomorrow\n" };
            var eval2Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm of tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm of tomorrow\n" };
            Assert.Equal(true, eval1Options.Contains(evaled[0]));
            Assert.Equal(true, eval2Options.Contains(evaled[1]));
        }

        [Fact]
        public void TestExpandTemplateWithFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var alarms = new[]
            {
                new
                {
                    time = "7 am",
                    date = "tomorrow"
                },
                new
                {
                    time = "8 pm",
                    date = "tomorrow"
                }
            };

            var evaled = templates.ExpandTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            var evalOptions = new List<string>()
            {
                "You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow",
                "You have 2 alarms, 7 am at tomorrow and 8 pm of tomorrow",
                "You have 2 alarms, 7 am of tomorrow and 8 pm at tomorrow",
                "You have 2 alarms, 7 am of tomorrow and 8 pm of tomorrow"
            };

            Assert.Equal(1, evaled.Count);
            Assert.Equal(true, evalOptions.Contains(evaled[0]));

            evaled = templates.ExpandTemplate("T2");
            Assert.Equal(1, evaled.Count);
            Assert.Equal(true, evaled[0].ToString() == "3" || evaled[0].ToString() == "5");

            evaled = templates.ExpandTemplate("T3");
            Assert.Equal(1, evaled.Count);
            Assert.Equal(true, evaled[0].ToString() == "3" || evaled[0].ToString() == "5");

            evaled = templates.ExpandTemplate("T4");
            Assert.Equal(1, evaled.Count);
            Assert.Equal(true, evaled[0].ToString() == "ey" || evaled[0].ToString() == "el");
        }

        [Fact]
        public void TestExpandTemplateWithIsTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var evaled = templates.ExpandTemplate("template2", new { templateName = "Greeting" });
            Assert.Equal(2, evaled.Count);
            Assert.Equal("Hi", evaled[0]);
            Assert.Equal("Hello", evaled[1]);

            evaled = templates.ExpandTemplate("template2", new { templateName = "xxx" });
            Assert.Equal(2, evaled.Count);
            Assert.Equal("Morning", evaled[0]);
            Assert.Equal("Evening", evaled[1]);
        }

        [Fact]
        public void TestExpandTemplateWithTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var evaled = templates.ExpandTemplate("template3", new { templateName = "Greeting" });
            Assert.Equal(2, evaled.Count);
            Assert.Equal("Hi", evaled[0]);
            Assert.Equal("Hello", evaled[1]);
        }

        [Fact]
        public void TestExpandTemplateWithDoubleQuotation()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var evaled = templates.ExpandTemplate("ExpanderT1");
            Assert.Equal(2, evaled.Count);
            var expectedResults = new List<string>()
            {
                "{\"lgType\":\"MyStruct\",\"text\":\"Hi \\\"quotes\\\" allowed\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hi \\\"quotes\\\" allowed\",\"speak\":\"what's your age?\"}"
            };

            for (var i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(JToken.DeepEquals(JObject.Parse(expectedResults[i]), JObject.Parse(evaled[i].ToString())));
            }
        }

        [Fact]
        public void TestExpandTemplateWithEscapeCharacter()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EscapeCharacter.lg"));
            var evaled = templates.ExpandTemplate("wPhrase", null);
            Assert.Equal(evaled[0], "Hi \r\n\t\\");

            evaled = templates.ExpandTemplate("AtEscapeChar", null);
            Assert.Equal(evaled[0], "Hi{1+1}[wPhrase]{wPhrase()}${wPhrase()}2${1+1}");

            evaled = templates.ExpandTemplate("otherEscape", null);
            Assert.Equal(evaled[0], @"Hi \y \");

            evaled = templates.ExpandTemplate("escapeInExpression", null);
            Assert.Equal(evaled[0], "Hi hello\\\\");

            evaled = templates.ExpandTemplate("escapeInExpression2", null);
            Assert.Equal(evaled[0], "Hi hello'");

            evaled = templates.ExpandTemplate("escapeInExpression3", null);
            Assert.Equal(evaled[0], "Hi hello\"");

            evaled = templates.ExpandTemplate("escapeInExpression4", null);
            Assert.Equal(evaled[0], "Hi hello\"");

            evaled = templates.ExpandTemplate("escapeInExpression5", null);
            Assert.Equal(evaled[0], "Hi hello\n");

            evaled = templates.ExpandTemplate("escapeInExpression6", null);
            Assert.Equal(evaled[0], "Hi hello\n");

            var todos = new[] { "A", "B", "C" };
            evaled = templates.ExpandTemplate("showTodo", new { todos });
            Assert.Equal(evaled[0].ToString().Replace("\r\n", "\n"), "\n    Your most recent 3 tasks are\n    * A\n* B\n* C\n    ");

            evaled = templates.ExpandTemplate("showTodo", null);
            Assert.Equal(evaled[0].ToString().Replace("\r\n", "\n"), "\n    You don't have any \"t\\\\odo'\".\n    ");

            evaled = templates.ExpandTemplate("getUserName", null);
            Assert.Equal(evaled[0], "super \"x man\"");

            evaled = templates.ExpandTemplate("structure1", null);
            Assert.Equal(evaled[0].ToString().Replace("\r\n", "\n").Replace("\n", string.Empty), "{  \"lgType\": \"struct\",  \"list\": [    \"a\",    \"b|c\"  ]}");

            evaled = templates.ExpandTemplate("dollarsymbol");
            Assert.Equal(evaled[0], "$ $ ${'hi'} hi");
        }

        [Fact]
        public void TestExpandTemplateWithEmptyListInStructuredLG()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            var data = new Dictionary<string, object>()
            {
                { "Name", "NAME" },
                { "Address", "ADDRESS" },
            };

            var input = new
            {
                Data = data
            };

            var name = "PointOfInterestSuggestedActionName";
            var evaled = templates.ExpandTemplate(name, input).ToList();
            Assert.Equal(JObject.Parse(evaled[0].ToString())["text"].ToString(), "NAME at ADDRESS");
            Assert.Equal(JObject.Parse(evaled[0].ToString())["speak"].ToString(), "NAME at ADDRESS");
            Assert.Equal(JObject.Parse(evaled[0].ToString())["attachments"].Count(), 0);
            Assert.Equal(JObject.Parse(evaled[0].ToString())["attachmentlayout"].ToString(), "list");
            Assert.Equal(JObject.Parse(evaled[0].ToString())["inputhint"].ToString(), "ignoringInput");
        }

        [Fact]
        public void TestExpandTemplateWithStrictMode()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/StrictModeFalse.lg"));
            
            var evaled = templates.ExpandTemplate("StrictFalse");
            Assert.Equal("null", evaled[0].ToString());

            templates = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/StrictModeTrue.lg"));

            var exception = Assert.Throws<Exception>(() => templates.ExpandTemplate("StrictTrue"));
            Assert.True(exception.Message.Contains("'variable_not_defined' evaluated to null. [StrictTrue]  Error occurred when evaluating '-${variable_not_defined}'"));
        }

        [Fact]
        public void TestEvalExpression()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EvalExpression.lg"));

            var userName = "MS";
            var evaled = templates.Evaluate("template1", new { userName });
            Assert.Equal(evaled, "Hi MS");

            evaled = templates.Evaluate("template2", new { userName });
            Assert.Equal(evaled, "Hi MS");

            evaled = templates.Evaluate("template3", new { userName });
            Assert.Equal(evaled, "HiMS");

            evaled = templates.Evaluate("template4", new { userName });
            var eval1Options = new List<string>() { "\r\nHi MS\r\n", "\nHi MS\n" };
            Assert.True(eval1Options.Contains(evaled));

            evaled = templates.Evaluate("template5", new { userName });
            var eval2Options = new List<string>() { "\r\nHiMS\r\n", "\nHiMS\n" };
            Assert.True(eval2Options.Contains(evaled));

            evaled = templates.Evaluate("template6", new { userName });
            Assert.Equal(evaled, "goodmorning");

            evaled = templates.Evaluate("template7");
            Assert.Equal(evaled, "{\"a\":\"hello\"}");

            evaled = templates.Evaluate("template8");
            Assert.Equal(evaled, "{\"user\":{\"name\":\"Allen\"}}");

            var value = JToken.FromObject(new { count = 13 });
            evaled = templates.Evaluate("template9", new { value });
            Assert.Equal(evaled, "{\"ctx\":{\"count\":13}}");

            evaled = templates.Evaluate("template10");
            Assert.Equal(evaled, 13L);

            evaled = templates.Evaluate("template11");
            Assert.Equal(evaled, 18L);
        }

        [Fact]
        public void TestRecursiveTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("RecursiveTemplate.lg"));
            var evaled = templates.Evaluate("RecursiveAccumulate", new { number = 10 });
            Assert.Equal(evaled, 55L);

            evaled = templates.Evaluate("RecursiveFactorial", new { number = 5 });
            Assert.Equal(evaled, 1 * 2 * 3 * 4 * 5L);

            evaled = templates.Evaluate("RecursiveFibonacciSequence", new { number = 5 });
            Assert.Equal(evaled, 5L);
        }

        [Fact]
        public void TemplateCRUD_Normal()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("CrudInit.lg"));

            Assert.Equal(2, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            Assert.Equal("template1", templates[0].Name);
            Assert.Equal(3, templates[0].SourceRange.Range.Start.Line);
            Assert.Equal(8, templates[0].SourceRange.Range.End.Line);
            Assert.Equal("template2", templates[1].Name);
            Assert.Equal(9, templates[1].SourceRange.Range.Start.Line);
            Assert.Equal(12, templates[1].SourceRange.Range.End.Line);

            // Add a template
            templates.AddTemplate("newtemplate", new List<string> { "age", "name" }, "- hi ");
            Assert.Equal(3, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            var newTemplate = templates[2];
            Assert.Equal("newtemplate", newTemplate.Name);
            Assert.Equal(2, newTemplate.Parameters.Count);
            Assert.Equal("age", newTemplate.Parameters[0]);
            Assert.Equal("name", newTemplate.Parameters[1]);
            Assert.Equal("- hi ", newTemplate.Body);
            Assert.Equal(14, newTemplate.SourceRange.Range.Start.Line);
            Assert.Equal(15, newTemplate.SourceRange.Range.End.Line);

            // add another template
            templates.AddTemplate("newtemplate2", null, "- hi2 ");
            Assert.Equal(4, templates.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            newTemplate = templates[3];
            Assert.Equal("newtemplate2", newTemplate.Name);
            Assert.Equal(0, newTemplate.Parameters.Count);
            Assert.Equal("- hi2 ", newTemplate.Body);
            Assert.Equal(16, newTemplate.SourceRange.Range.Start.Line);
            Assert.Equal(17, newTemplate.SourceRange.Range.End.Line);

            // update a middle template
            templates.UpdateTemplate("newtemplate", "newtemplateName", new List<string> { "newage", "newname" }, "- new hi\r\n#hi");
            Assert.Equal(4, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            newTemplate = templates[2];
            Assert.Equal("newtemplateName", newTemplate.Name);
            Assert.Equal(2, newTemplate.Parameters.Count);
            Assert.Equal("newage", newTemplate.Parameters[0]);
            Assert.Equal("newname", newTemplate.Parameters[1]);
            Assert.Equal("- new hi\n- #hi", newTemplate.Body.Replace("\r\n", "\n"));
            Assert.Equal(14, newTemplate.SourceRange.Range.Start.Line);
            Assert.Equal(16, newTemplate.SourceRange.Range.End.Line);
            Assert.Equal(17, templates[3].SourceRange.Range.Start.Line);
            Assert.Equal(18, templates[3].SourceRange.Range.End.Line);

            // update the tailing template
            templates.UpdateTemplate("newtemplate2", "newtemplateName2", new List<string> { "newage2", "newname2" }, "- new hi\r\n#hi2\r\n");
            Assert.Equal(4, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            newTemplate = templates[3];
            Assert.Equal("newtemplateName2", newTemplate.Name);
            Assert.Equal(2, newTemplate.Parameters.Count);
            Assert.Equal("- new hi\n- #hi2\n", newTemplate.Body.Replace("\r\n", "\n"));
            Assert.Equal(17, newTemplate.SourceRange.Range.Start.Line);
            Assert.Equal(19, newTemplate.SourceRange.Range.End.Line);

            // delete a middle template
            templates.DeleteTemplate("newtemplateName");
            Assert.Equal(3, templates.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            Assert.Equal(14, templates[2].SourceRange.Range.Start.Line);
            Assert.Equal(16, templates[2].SourceRange.Range.End.Line);

            // delete the tailing template
            templates.DeleteTemplate("newtemplateName2");
            Assert.Equal(2, templates.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            Assert.Equal(9, templates[1].SourceRange.Range.Start.Line);
            Assert.Equal(12, templates[1].SourceRange.Range.End.Line);
        }

        [Fact]
        public void TemplateCRUD_RepeatAdd()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("CrudInit.lg"));

            // Add a template
            templates.AddTemplate("newtemplate", new List<string> { "age", "name" }, "- hi ");
            Assert.Equal(3, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            var newTemplate = templates[2];
            Assert.Equal("newtemplate", newTemplate.Name);
            Assert.Equal(2, newTemplate.Parameters.Count);
            Assert.Equal("age", newTemplate.Parameters[0]);
            Assert.Equal("name", newTemplate.Parameters[1]);
            Assert.Equal("- hi ", newTemplate.Body);
            Assert.Equal(14, newTemplate.SourceRange.Range.Start.Line);
            Assert.Equal(15, newTemplate.SourceRange.Range.End.Line);

            // add another template
            templates.AddTemplate("newtemplate2", null, "- hi2 ");
            Assert.Equal(4, templates.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            newTemplate = templates[3];
            Assert.Equal("newtemplate2", newTemplate.Name);
            Assert.Equal(0, newTemplate.Parameters.Count);
            Assert.Equal("- hi2 ", newTemplate.Body);
            Assert.Equal(16, newTemplate.SourceRange.Range.Start.Line);
            Assert.Equal(17, newTemplate.SourceRange.Range.End.Line);

            // add an exist template
            var exception = Assert.Throws<Exception>(() => templates.AddTemplate("newtemplate", null, "- hi2 "));
            Assert.Equal(TemplateErrors.TemplateExist("newtemplate"), exception.Message);
        }

        [Fact]
        public void TemplateCRUD_RepeatDelete()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("CrudInit.lg"));

            // Delete template
            templates.DeleteTemplate("template1");
            Assert.Equal(1, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            Assert.Equal("template2", templates[0].Name);
            Assert.Equal(3, templates[0].SourceRange.Range.Start.Line);
            Assert.Equal(6, templates[0].SourceRange.Range.End.Line);

            // Delete a template that does not exist
            templates.DeleteTemplate("xxx");
            Assert.Equal(1, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(0, templates.Diagnostics.Count);
            Assert.Equal("template2", templates[0].Name);
            Assert.Equal(3, templates[0].SourceRange.Range.Start.Line);
            Assert.Equal(6, templates[0].SourceRange.Range.End.Line);

            // Delete all template
            templates.DeleteTemplate("template2");
            Assert.Equal(0, templates.Count);
            Assert.Equal(0, templates.Imports.Count);
            Assert.Equal(1, templates.Diagnostics.Count);
            Assert.Equal(DiagnosticSeverity.Warning, templates.Diagnostics[0].Severity);
            Assert.Equal(TemplateErrors.NoTemplate, templates.Diagnostics[0].Message);
        }

        [Fact]
        public void TemplateCRUD_Diagnostic()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("CrudInit.lg"));

            // add error template name (error in template)
            templates.AddTemplate("newtemplate#$%", new List<string> { "age", "name" }, "- hi ");
            Assert.Equal(1, templates.Diagnostics.Count);
            var diagnostic = templates.Diagnostics[0];
            Assert.Equal(TemplateErrors.InvalidTemplateName("newtemplate#$%"), diagnostic.Message);
            Assert.Equal(14, diagnostic.Range.Start.Line);
            Assert.Equal(14, diagnostic.Range.End.Line);

            // replace the error template with right template
            templates.UpdateTemplate("newtemplate#$%", "newtemplateName", null, "- new hi");
            Assert.Equal(0, templates.Diagnostics.Count);

            // reference the other exist template
            templates.UpdateTemplate("newtemplateName", "newtemplateName", null, "- ${template1()}");
            Assert.Equal(0, templates.Diagnostics.Count);

            // wrong reference, throw by static checker
            templates.UpdateTemplate("newtemplateName", "newtemplateName", null, "- ${NoTemplate()}");
            Assert.Equal(1, templates.Diagnostics.Count);
            diagnostic = templates.Diagnostics[0];
            Assert.True(diagnostic.Message.Contains("it's not a built-in function or a custom function"));
            Assert.Equal(15, diagnostic.Range.Start.Line);
            Assert.Equal(15, diagnostic.Range.End.Line);

            // delete the error template
            templates.DeleteTemplate("newtemplateName");
            Assert.Equal(0, templates.Diagnostics.Count);
        }

        [Fact]
        public void TestMemoryScope()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MemoryScope.lg"));
            var evaled = templates.Evaluate("T1", new { turn = new { name = "Dong", count = 3 } });
            Assert.Equal(evaled, "Hi Dong, welcome to Seattle, Seattle is a beautiful place, how many burgers do you want, 3?");

            var scope = new SimpleObjectMemory(new
            {
                schema = new Dictionary<string, object>()
                {
                    {
                        "Bread", new Dictionary<string, object>()
                        {
                            {
                                "enum", new List<string>() { "A", "B" }
                            }
                        }
                    }
                }
            });

            evaled = templates.Evaluate("AskBread", scope);

            Assert.Equal(evaled, "Which Bread, A or B do you want?");
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }

        [Fact]
        public void TestStructuredTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("StructuredTemplate.lg"));

            var evaled = templates.Evaluate("AskForAge.prompt");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"how old are you?\"}"), evaled as JObject)
                || JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"what's your age?\"}"), evaled as JObject));

            evaled = templates.Evaluate("AskForAge.prompt2");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"suggestedactions\":[\"10\",\"20\",\"30\"]}"), evaled as JObject)
                || JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"suggestedactions\":[\"10\",\"20\",\"30\"]}"), evaled as JObject));

            evaled = templates.Evaluate("AskForAge.prompt3");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"${GetAge()}\",\"suggestions\":[\"10 | cards\",\"20 | cards\"]}"), evaled as JObject));

            evaled = templates.Evaluate("T1");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"This is awesome\",\"speak\":\"foo bar I can also speak!\"}"), evaled as JObject));

            evaled = templates.Evaluate("ST1");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"text\":\"foo\",\"speak\":\"bar\"}"), evaled as JObject));

            evaled = templates.Evaluate("AskForColor");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"suggestedactions\":[{\"lgType\":\"MyStruct\",\"speak\":\"bar\",\"text\":\"zoo\"},{\"lgType\":\"Activity\",\"speak\":\"I can also speak!\"}]}"), evaled as JObject));

            evaled = templates.Evaluate("MultiExpression");
            var options = new string[]
            {
                "{\r\n  \"lgType\": \"Activity\",\r\n  \"speak\": \"I can also speak!\"\r\n} {\r\n  \"lgType\": \"MyStruct\",\r\n  \"text\": \"hi\"\r\n}",
                "{\n  \"lgType\": \"Activity\",\n  \"speak\": \"I can also speak!\"\n} {\n  \"lgType\": \"MyStruct\",\n  \"text\": \"hi\"\n}"
            };
            Assert.True(options.Contains(evaled.ToString()));

            evaled = templates.Evaluate("StructuredTemplateRef");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"text\":\"hi\"}"), evaled as JObject));

            evaled = templates.Evaluate("MultiStructuredRef");

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"list\":[{\"lgType\":\"SubStruct\",\"text\":\"hello\"},{\"lgType\":\"SubStruct\",\"text\":\"world\"}]}"), evaled as JObject));

            evaled = templates.Evaluate("templateWithSquareBrackets", new { manufacturer = new { Name = "Acme Co" } });

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Struct\",\"text\":\"Acme Co\"}"), evaled as JObject));

            evaled = templates.Evaluate("ValueWithEqualsMark", new { name = "Jack" });

            Assert.True(
                JToken.DeepEquals(JObject.Parse("{\"lgType\": \"Activity\",\"text\": \"Hello! welcome back. I have your name = Jack\"}"), evaled as JObject));
        }

        [Fact]
        public void TestEvaluateOnce()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EvaluateOnce.lg"));

            var evaled = templates.Evaluate("templateWithSameParams", new { param = "ms" });
            Assert.NotNull(evaled);

            var resultList = evaled.ToString().Split(" ");
            Assert.True(resultList.Length == 2);
            Assert.True(resultList[0] == resultList[1]);

            // may be has different values
            evaled = templates.Evaluate("templateWithDifferentParams", new { param1 = "ms", param2 = "newms" });
        }

        [Fact]
        public void TestReExecute()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ReExecute.lg"));

            // may be has different values
            var evaled = templates.Evaluate("templateWithSameParams", new { param1 = "ms", param2 = "newms" });
        }

        [Fact]
        public void TestConditionExpression()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ConditionExpression.lg"));

            var evaled = templates.Evaluate("conditionTemplate", new { num = 1 });

            Assert.Equal(evaled, "Your input is one");

            evaled = templates.Evaluate("conditionTemplate", new { num = 2 });

            Assert.Equal(evaled, "Your input is two");

            evaled = templates.Evaluate("conditionTemplate", new { num = 3 });

            Assert.Equal(evaled, "Your input is three");

            evaled = templates.Evaluate("conditionTemplate", new { num = 4 });

            Assert.Equal(evaled, "Your input is not one, two or three");
        }

        [Fact]
        public void TestLoopScope()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("LoopScope.lg"));

            var loopClass1 = new LoopClass();
            loopClass1.Name = "jack";

            var loopClass2 = new LoopClass();
            loopClass2.Name = "jones";

            loopClass1.LoopObj = loopClass2;
            loopClass2.LoopObj = loopClass1;

            templates.Evaluate("template1", new { scope = loopClass1 });
        }

        [Fact]
        public void TestExpandTemplateWithStructuredLG()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("StructuredTemplate.lg"));

            // without scope
            var evaled = templates.ExpandTemplate("AskForAge.prompt");
            Assert.Equal(4, evaled.Count);
            var expectedResults = new List<string>()
            {
                "{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"what's your age?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"what's your age?\"}"
            };

            for (var i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(JToken.DeepEquals(JObject.Parse(expectedResults[i]), JObject.Parse(evaled[i].ToString())));
            }

            evaled = templates.ExpandTemplate("ExpanderT1");
            Assert.Equal(4, evaled.Count);
            expectedResults = new List<string>()
            {
                "{\"lgType\":\"MyStruct\",\"text\":\"Hi\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hello\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hi\",\"speak\":\"what's your age?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hello\",\"speak\":\"what's your age?\"}"
            };

            for (var i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(JToken.DeepEquals(JObject.Parse(expectedResults[i]), JObject.Parse(evaled[i].ToString())));
            }
        }

        [Fact]
        public void TestExpressionextract()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ExpressionExtract.lg"));

            var evaled1 = templates.Evaluate("templateWithBrackets");
            var evaled2 = templates.Evaluate("templateWithBrackets2");
            var evaled3 = templates.Evaluate("templateWithBrackets3").ToString().Trim();
            var espectedResult = "don't mix {} and '{}'";
            Assert.Equal(evaled1, espectedResult);
            Assert.Equal(evaled2, espectedResult);
            Assert.Equal(evaled3, espectedResult);

            evaled1 = templates.Evaluate("templateWithQuotationMarks");
            evaled2 = templates.Evaluate("templateWithQuotationMarks2");
            evaled3 = templates.Evaluate("templateWithQuotationMarks3").ToString().Trim();
            espectedResult = "don't mix {\"} and \"\"'\"";
            Assert.Equal(evaled1, espectedResult);
            Assert.Equal(evaled2, espectedResult);
            Assert.Equal(evaled3, espectedResult);

            evaled1 = templates.Evaluate("templateWithUnpairedBrackets1");
            evaled2 = templates.Evaluate("templateWithUnpairedBrackets12");
            evaled3 = templates.Evaluate("templateWithUnpairedBrackets13").ToString().Trim();
            espectedResult = "{prefix 5 sufix";
            Assert.Equal(evaled1, espectedResult);
            Assert.Equal(evaled2, espectedResult);
            Assert.Equal(evaled3, espectedResult);

            evaled1 = templates.Evaluate("templateWithUnpairedBrackets2");
            evaled2 = templates.Evaluate("templateWithUnpairedBrackets22");
            evaled3 = templates.Evaluate("templateWithUnpairedBrackets23").ToString().Trim();
            espectedResult = "prefix 5 sufix}";
            Assert.Equal(evaled1, espectedResult);
            Assert.Equal(evaled2, espectedResult);
            Assert.Equal(evaled3, espectedResult);
        }

        [Fact]
        public void TestStringInterpolation()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("StringInterpolation.lg"));

            var evaled = templates.Evaluate("simpleStringTemplate");
            Assert.Equal("say hi", evaled);

            evaled = templates.Evaluate("StringTemplateWithVariable", new { w = "world" });
            Assert.Equal("hello world", evaled);

            evaled = templates.Evaluate("StringTemplateWithMixing", new { name = "jack" });
            Assert.Equal("I know your name is jack", evaled);

            evaled = templates.Evaluate("StringTemplateWithJson", new { h = "hello", w = "world" });
            Assert.Equal("get 'h' value : hello", evaled);

            evaled = templates.Evaluate("StringTemplateWithEscape");
            Assert.Equal("just want to output ${bala`bala}", evaled);

            evaled = templates.Evaluate("StringTemplateWithTemplateRef");
            Assert.Equal("hello jack , welcome. nice weather!", evaled);
        }

        public void TestMemoryAccessPath()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MemoryAccess.lg"));

            var memory = new
            {
                myProperty = new
                {
                    name = "p1"
                },

                turn = new
                {
                    properties = new Dictionary<string, object>
                    {
                        {
                            "p1", new Dictionary<string, object>() { { "enum", "p1enum" } }
                        }
                    }
                }
            };

            // this evaulate will hit memory access twice
            // first for "property", and get "p1", from local
            // sencond for "turn.property[p1].enum" and get "p1enum" from global
            var result = templates.Evaluate("T1", memory);
            Assert.Equal(result, "p1enum");

            // this evaulate will hit memory access twice
            // first for "myProperty.name", and get "p1", from global
            // sencond for "turn.property[p1].enum" and get "p1enum" from global 
            result = templates.Evaluate("T3", memory);
            Assert.Equal(result, "p1enum");
        }

        [Fact]
        public void TestIsTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("IsTemplate.lg"));

            var evaled = templates.Evaluate("template2", new { templateName = "template1" });
            Assert.Equal("template template1 exists", evaled);

            evaled = templates.Evaluate("template2", new { templateName = "wPhrase" });
            Assert.Equal("template wPhrase exists", evaled);

            evaled = templates.Evaluate("template2", new { templateName = "xxx" });
            Assert.Equal("template xxx does not exist", evaled);
        }

        [Fact]
        public void TestEmptyArratAndObject()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EmptyArrayAndObject.lg"));

            var evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new { } });
            Assert.Equal("list and obj are both empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new Dictionary<string, object>() });
            Assert.Equal("list and obj are both empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { "hi" }, obj = new { } });
            Assert.Equal("obj is empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new { a = "a" } });
            Assert.Equal("list is empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new Dictionary<string, object> { { "a", "b" } } });
            Assert.Equal("list is empty", evaled);

            evaled = templates.Evaluate("template", new { list = new JArray() { new JObject() }, obj = new JObject { ["a"] = "b" } });
            Assert.Equal("list and obj are both not empty.", evaled);
        }

        [Fact]
        public void TestNullTolerant()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("NullTolerant.lg"));

            var evaled = templates.Evaluate("template1");

            Assert.Equal("null", evaled);

            evaled = templates.Evaluate("template2");

            Assert.Equal("result is 'null'", evaled);

            var jObjEvaled = templates.Evaluate("template3") as JObject;

            Assert.Equal("null", jObjEvaled["key1"]);
        }

        [Fact]
        public void TestLGOptions()
        {
            //LGOptionTest has no import files.
            var templates = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/LGOptionTest.lg"));

            var evaled = templates.Evaluate("SayHello");

            Assert.Equal("hi The user.name is undefined", evaled);

            evaled = templates.Evaluate("testInlineString");

            Assert.Equal($"m\n\ns\n\nf\n\nt\n\n", evaled.ToString().Replace("\r\n", "\n"));

            //a1.lg imports b1.lg. 
            //a1's option is strictMode is false, replaceNull = ${path} is undefined, and defalut lineBreakStyle.
            //b1's option is strictMode is true, replaceNull = The ${path} is undefined, and markdown lineBreakStyle.
            var templates2 = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/a1.lg"));

            var evaled2 = templates2.Evaluate("SayHello");

            Assert.Equal("hi user.name is undefined", evaled2);

            Assert.Equal(templates2.LgOptions.LineBreakStyle, LGLineBreakStyle.Default);

            //a2.lg imports b2.lg and c2.lg. 
            //a2.lg: replaceNull = The ${path} is undefined  
            //b2.lg: strict = true, replaceNull = ${path} is evaluated to null, please check!
            //c2: lineBreakStyle = markdown
            var templates3 = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/a2.lg"));

            var evaled3 = templates3.Evaluate("SayHello");

            Assert.Equal("hi The user.name is undefined", evaled3);

            Assert.Equal(templates3.LgOptions.LineBreakStyle, null);

            //a3.lg imports b3.lg and c3.lg in sequence. 
            //b3.lg imports d3.lg 
            //a3.lg: lineBreakStyle = markdown, replaceNull = the ${path} is undefined a3!
            //b3.lg: lineBreakStyle = default
            //d3: replaceNull = ${path} is evaluated to null in d3!
            //c3: replaceNull = ${path} is evaluated to null in c3!
            var templates4 = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/a3.lg"));

            var evaled4 = templates4.Evaluate("SayHello");

            Assert.Equal("hi the user.name is undefined a3!", evaled4);

            Assert.Equal(templates4.LgOptions.LineBreakStyle, LGLineBreakStyle.Markdown);

            //Test use an defined option in Evaluate method, which will override all options in LG files.
            var optionStrList = new string[] { "@strictMode = false", "@replaceNull = ${ path } is undefined", "@lineBreakStyle = defalut" };
            var newOpt = new EvaluationOptions(optionStrList);
            evaled4 = templates4.Evaluate("SayHello", null, newOpt);

            Assert.Equal("hi user.name is undefined", evaled4);

            evaled4 = templates4.Evaluate("testInlineString", null, newOpt);

            Assert.Equal($"m\ns\nf\nt\n", evaled4.ToString().Replace("\r\n", "\n"));

            //a4.lg imports b4.lg and c4.lg in sequence. 
            //b4.lg imports d3.lg, c4.lg imports f4.lg.
            //a4.lg: replaceNull = the ${path} is undefined a4!.
            //b4.lg, c4.lg: nothing but import statement.
            //d4: only have template definition.
            //f4: only options, strictMode = true, replaceNull = The ${path} is undefined, lineBreaStyle = markdown.
            var templates5 = Templates.ParseFile(GetExampleFilePath("EvaluationOptions/a4.lg"));

            var evaled5 = templates5.Evaluate("SayHello");

            Assert.Equal("hi the user.name is undefined a4!", evaled5);

            Assert.Equal(templates5.LgOptions.StrictMode, null);

            Assert.Equal(templates5.LgOptions.LineBreakStyle, null);
        }

        [Fact]
        public void TestInlineEvaluate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("2.lg"));
            var evaled = templates.EvaluateText("hello");
            Assert.Equal("hello", evaled);

            evaled = templates.EvaluateText("${``}");
            Assert.Equal(string.Empty, evaled);

            // test template reference
            evaled = templates.EvaluateText("${wPhrase()}");
            var options = new List<string> { "Hi", "Hello", "Hiya" };
            Assert.True(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");

            var exception = Assert.Throws<Exception>(() => templates.EvaluateText("${ErrrorTemplate()}"));
            Assert.True(exception.Message.Contains("it's not a built-in function or a custom function"));
        }

        [Fact]
        public void TestCustomFunction()
        {
            var parser = new ExpressionParser((string func) =>
            { 
                if (func == "custom")
                {
                    return new NumericEvaluator("custom", (args) => (int)args[0] + (int)args[1]);
                }
                else
                {
                    return Expression.Lookup(func);
                }
            });
            var templates = Templates.ParseFile(GetExampleFilePath("CustomFunction.lg"), null, parser);
            var evaled = templates.Evaluate("template");
            Assert.Equal(3, evaled);
            evaled = templates.Evaluate("callRef");
            Assert.Equal(12, evaled);
        }

        [Fact]
        public void TestCustomFunction2()
        {
            Expression.Functions.Add("contoso.sqrt", (args) =>
            {
                object retValue = null;
                if (args[0] != null)
                {
                    double dblValue;
                    if (double.TryParse(args[0], out dblValue))
                    {
                        retValue = Math.Sqrt(dblValue);
                    }
                }

                return retValue;
            });
            var templates = Templates.ParseFile(GetExampleFilePath("CustomFunction2.lg"), null);
            var evaled = templates.Evaluate("custom");
            Assert.Equal(6.0, evaled);
        }

        [Fact]
        public void TestInjectLG()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("./InjectionTest/inject.lg"));
            
            var (evaled, error) = Expression.Parse("foo.bar()").TryEvaluate(null);
            
            Assert.Equal("3", evaled.ToString());

            (evaled, error) = Expression.Parse("foo.cool(2)").TryEvaluate(null);
            Assert.Equal("3", evaled.ToString());

            (evaled, error) = Expression.Parse("common.looking()").TryEvaluate(null);
            Assert.Equal("John", evaled);

            var scope1 = new { a = new List<string> { "cat", "dog" }, b = 12.10, c = new List<string> { "lion" } };
            (evaled, error) = Expression.Parse("string(common.countTotal(a, c))").TryEvaluate(scope1);
            Assert.Null(error);
            Assert.Equal("3", evaled);

            (evaled, error) = Expression.Parse("common.countTotal()").TryEvaluate(scope1);
            Assert.NotNull(error);

            (evaled, error) = Expression.Parse("common.countTotal(a, b, c)").TryEvaluate(scope1);
            Assert.NotNull(error);

            var scope2 = new { i = 1, j = 2, k = 3, l = 4 };
            (evaled, error) = Expression.Parse("common.sumFourNumbers(i, j, k, l)").TryEvaluate(scope2);
            Assert.Equal("10", evaled.ToString());
        }

        [Fact]
        public void TestFileOperation()
        {
            Templates.EnableFromFile = true;

            var templates = Templates.ParseFile(GetExampleFilePath("FileOperation.lg"));
            var evaluated = templates.Evaluate("FromFileWithEvaluation1", new { name = "Lucy" });
            Assert.Equal("hi Lucy", evaluated);

            evaluated = templates.Evaluate("FromFileWithEvaluation2", new { name = "Lucy" });
            Assert.Equal("hi Lucy", evaluated);
        }

        [Fact]
        public void TestFileOperationDisabled()
        {
            Templates.EnableFromFile = false;

            Assert.Throws<Exception>(() =>
            {
                var templates = Templates.ParseFile(GetExampleFilePath("FileOperation.lg"));
                var evaluated = templates.Evaluate("FromFileWithoutEvaluation");
            });
        }

        public class LoopClass
        {
            public string Name { get; set; }

            public object LoopObj { get; set; }
        }
    }
}
