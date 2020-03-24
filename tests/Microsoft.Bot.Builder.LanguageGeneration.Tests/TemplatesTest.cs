// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1202 // Elements should be ordered by access
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplatesTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestBasic()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("2.lg"));

            var evaled = templates.Evaluate("wPhrase");
            var options = new List<string> { "Hi", "Hello", "Hiya" };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("3.lg"));

            var evaled = templates.Evaluate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)" };

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = templates.Evaluate("welcome-user", new { userName = userName }).ToString();
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName), $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestIfElseTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("5.lg"));

            string evaled = templates.Evaluate("time-of-day-readout", new { timeOfDay = "morning" }).ToString();
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = templates.Evaluate("time-of-day-readout", new { timeOfDay = "evening" }).ToString();
            Assert.IsTrue(evaled == "Good evening" || evaled == "Evening! ", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicConditionalTemplateWithoutDefault()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("5.lg"));

            string evaled = templates.Evaluate("time-of-day-readout-without-default", new { timeOfDay = "morning" }).ToString();
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = templates.Evaluate("time-of-day-readout-without-default2", new { timeOfDay = "morning" }).ToString();
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            object evaledNull = templates.Evaluate("time-of-day-readout-without-default2", new { timeOfDay = "evening" });
            Assert.IsNull(evaledNull, "Evaled is not null");
        }

        [TestMethod]
        public void TestMultiLineExprInLG()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MultiLineExpr.lg"));

            string evaled = templates.Evaluate("ExprInCondition", new { userName = "Henry", day = "Monday" }).ToString();
            Assert.IsTrue(evaled == "Not today", $"Evaled is {evaled}");

            evaled = templates.Evaluate("definition").ToString();
            Assert.IsTrue(evaled == "10", $"Evaled is {evaled}");

            evaled = templates.Evaluate("template").ToString();
            Assert.IsTrue(evaled == "15", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicSwitchCaseTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("switchcase.lg"));

            string evaled = templates.Evaluate("greetInAWeek", new { day = "Saturday" }).ToString();
            Assert.IsTrue(evaled == "Happy Saturday!");

            evaled = templates.Evaluate("greetInAWeek", new { day = "Monday" }).ToString();
            Assert.IsTrue(evaled == "Work Hard!");
        }

        [TestMethod]
        public void TestBasicTemplateRefWithParameters()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("6.lg"));
            string evaled = templates.Evaluate("welcome", null).ToString();
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = templates.Evaluate("welcome", new { userName = "DL" }).ToString();
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [TestMethod]
        public void TestBasicListSupport()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("BasicList.lg"));
            Assert.AreEqual(templates.Evaluate("BasicJoin", new { items = new[] { "1" } }), "1");
            Assert.AreEqual(templates.Evaluate("BasicJoin", new { items = new[] { "1", "2" } }), "1, 2");
            Assert.AreEqual(templates.Evaluate("BasicJoin", new { items = new[] { "1", "2", "3" } }), "1, 2 and 3");
        }

        [TestMethod]
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
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            var evaled = templates.Evaluate("ShowAlarmsWithForeach", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            evaled = templates.Evaluate("ShowAlarmsWithLgTemplate", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            evaled = templates.Evaluate("ShowAlarmsWithDynamicLgTemplate", new { alarms = alarms, templateName = "ShowAlarm" });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = templates.EvaluateTemplate("ShowAlarmsWithMemberForeach", new { alarms = alarms });
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = templates.EvaluateTemplate("ShowAlarmsWithHumanize", new { alarms = alarms });
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = templates.EvaluateTemplate("ShowAlarmsWithMemberHumanize", new { alarms = alarms });
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);
        }

        [TestMethod]
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
            Assert.AreEqual("You have two alarms", evaled);

            evaled = templates.Evaluate("greetInAWeek", new { day = "Saturday" });
            Assert.AreEqual("Happy Saturday!", evaled);
        }

        [TestMethod]
        public void TestListWithOnlyOneElement()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("8.lg"));
            var evaled = templates.Evaluate("ShowTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual("Your most recent task is Task1. You can let me know if you want to add or complete a task.", evaled);
        }

        [TestMethod]
        public void TestTemplateNameWithDotIn()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("TemplateNameWithDot.lg"));
            Assert.AreEqual(templates.Evaluate("Hello.World", null), "Hello World");
            Assert.AreEqual(templates.Evaluate("Hello", null), "Hello World");
        }

        [TestMethod]
        public void TestMultiLine()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MultilineTextForAdaptiveCard.lg"));
            var evaled1 = templates.Evaluate("wPhrase", string.Empty);
            var options1 = new List<string> { "\r\ncardContent\r\n", "hello", "\ncardContent\n" };
            Assert.IsTrue(options1.Contains(evaled1), $"Evaled is {evaled1}");

            var evaled2 = templates.Evaluate("nameTemplate", new { name = "N" });
            var options2 = new List<string> { "\r\nN\r\n", "N", "\nN\n" };
            Assert.IsTrue(options2.Contains(evaled2), $"Evaled is {evaled2}");

            var evaled3 = templates.Evaluate("adaptivecardsTemplate", string.Empty);

            var evaled4 = templates.Evaluate("refTemplate", string.Empty);
            var options4 = new List<string> { "\r\nhi\r\n", "\nhi\n" };
            Assert.IsTrue(options4.Contains(evaled4), $"Evaled is {evaled4}");
        }

        [TestMethod]
        public void TestTemplateRef()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("TemplateRef.lg"));

            var scope = new
            {
                time = "morning",
                name = "Dong Lei"
            };
            Assert.AreEqual(templates.Evaluate("Hello", scope), "Good morning Dong Lei");
        }

        [TestMethod]
        public void TestEscapeCharacter()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EscapeCharacter.lg"));
            var evaled = templates.Evaluate("wPhrase", null);
            Assert.AreEqual(evaled, "Hi \r\n\t\\");

            evaled = templates.Evaluate("AtEscapeChar", null);
            Assert.AreEqual(evaled, "Hi{1+1}[wPhrase]{wPhrase()}${wPhrase()}2${1+1}");

            evaled = templates.Evaluate("otherEscape", null);
            Assert.AreEqual(evaled, @"Hi \y \");

            evaled = templates.Evaluate("escapeInExpression", null);
            Assert.AreEqual(evaled, "Hi hello\\\\");

            evaled = templates.Evaluate("escapeInExpression2", null);
            Assert.AreEqual(evaled, "Hi hello'");

            evaled = templates.Evaluate("escapeInExpression3", null);
            Assert.AreEqual(evaled, "Hi hello\"");

            evaled = templates.Evaluate("escapeInExpression4", null);
            Assert.AreEqual(evaled, "Hi hello\"");

            evaled = templates.Evaluate("escapeInExpression5", null);
            Assert.AreEqual(evaled, "Hi hello\n");

            evaled = templates.Evaluate("escapeInExpression6", null);
            Assert.AreEqual(evaled, "Hi hello\n");

            var todos = new[] { "A", "B", "C" };
            evaled = templates.Evaluate("showTodo", new { todos });
            Assert.AreEqual(((string)evaled).Replace("\r\n", "\n"), "\n    Your most recent 3 tasks are\n    * A\n* B\n* C\n    ");
            
            evaled = templates.Evaluate("showTodo", null);
            Assert.AreEqual(((string)evaled).Replace("\r\n", "\n"), "\n    You don't have any \"t\\\\odo'\".\n    ");

            evaled = templates.Evaluate("getUserName", null);
            Assert.AreEqual(evaled, "super \"x man\"");

            evaled = templates.Evaluate("structure1", null);
            Assert.AreEqual(evaled.ToString().Replace("\r\n", "\n").Replace("\n", string.Empty), "{  \"lgType\": \"struct\",  \"list\": [    \"a\",    \"b|c\"  ]}");

            evaled = templates.Evaluate("nestedSample", null);
            Assert.AreEqual(evaled.ToString(), "i like three movies, they are \"\\\"name1\", \"name2\" and \"{name3\"");

            evaled = templates.Evaluate("dollarsymbol");
            Assert.AreEqual("$ $ ${'hi'} hi", evaled);
        }

        [TestMethod]
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
                    name = "coffee-to-go-order",
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
                Assert.AreEqual(variableEvaledOptions.Length, variableEvaled.Count);
                variableEvaledOptions.ToList().ForEach(element => Assert.AreEqual(variableEvaled.Contains(element), true));
                var templateEvaled = evaled1.TemplateReferences;
                var templateEvaledOptions = testItem.GetType().GetProperty("templateRefOptions").GetValue(testItem) as string[];
                Assert.AreEqual(templateEvaledOptions.Length, templateEvaled.Count);
                templateEvaledOptions.ToList().ForEach(element => Assert.AreEqual(templateEvaled.Contains(element), true));
            }
        }

        [TestMethod]
        public void TestlgTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = templates.Evaluate("TemplateC", string.Empty);
            var options = new List<string> { "Hi", "Hello" };
            Assert.AreEqual(options.Contains(evaled), true);

            evaled = templates.Evaluate("TemplateD", new { b = "morning" });
            options = new List<string> { "Hi morning", "Hello morning" };
            Assert.AreEqual(options.Contains(evaled), true);
        }

        [TestMethod]
        public void TestTemplateAsFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("TemplateAsFunction.lg"));
            string evaled = templates.Evaluate("Test2", string.Empty).ToString();

            Assert.AreEqual(evaled, "hello world");

            evaled = templates.Evaluate("Test3", string.Empty).ToString();
            Assert.AreEqual(evaled, "hello world");

            evaled = templates.Evaluate("Test4", string.Empty).ToString();

            Assert.AreEqual(evaled.Trim(), "hello world");

            evaled = templates.Evaluate("dupNameWithTemplate").ToString();
            Assert.AreEqual(evaled, "2");
        }

        [TestMethod]
        public void TestAnalyzelgTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = templates.AnalyzeTemplate("TemplateD");
            var variableEvaled = evaled.Variables;
            var options = new List<string>() { "b" };
            Assert.AreEqual(variableEvaled.Count, options.Count);
            options.ForEach(e => Assert.AreEqual(variableEvaled.Contains(e), true));
        }

        [TestMethod]
        public void TestImportLgFiles()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("import.lg"));

            // Assert 6.lg is imported only once when there are several relative paths which point to the same file.
            // Assert import cycle loop is handled well as expected when a file imports itself.
            Assert.AreEqual(14, templates.AllTemplates.Count());

            string evaled = templates.Evaluate("basicTemplate", null).ToString();
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = templates.Evaluate("welcome", null).ToString();
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = templates.Evaluate("template3", null).ToString();
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            evaled = templates.Evaluate("welcome", new { userName = "DL" }).ToString();
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");

            evaled = templates.Evaluate("basicTemplate2", null).ToString();
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            // Assert 6.lg of relative path is imported from text.
            templates = Templates.ParseText("# basicTemplate\r\n- Hi\r\n- Hello\r\n[import](./6.lg)", GetExampleFilePath("xx.lg"));

            Assert.AreEqual(8, templates.AllTemplates.Count());
            evaled = templates.Evaluate("basicTemplate", null).ToString();
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = templates.Evaluate("welcome", null).ToString();
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = templates.Evaluate("welcome", new { userName = "DL" }).ToString();
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [TestMethod]
        public void TestRegex()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Regex.lg"));
            var evaled = templates.Evaluate("wPhrase", string.Empty);
            Assert.AreEqual(evaled, "Hi");

            evaled = templates.Evaluate("wPhrase", new { name = "jack" });
            Assert.AreEqual(evaled, "Hi jack");

            evaled = templates.Evaluate("wPhrase", new { name = "morethanfive" });
            Assert.AreEqual(evaled, "Hi");
        }

        [TestMethod]
        public void TestExpandTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("Expand.lg"));

            // without scope
            var evaled = templates.ExpandTemplate("FinalGreeting");
            Assert.AreEqual(4, evaled.Count);
            var expectedResults = new List<string>() { "Hi Morning", "Hi Evening", "Hello Morning", "Hello Evening" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            // with scope
            evaled = templates.ExpandTemplate("TimeOfDayWithCondition", new { time = "evening" });
            Assert.AreEqual(2, evaled.Count);
            expectedResults = new List<string>() { "Hi Evening", "Hello Evening" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            // with scope
            evaled = templates.ExpandTemplate("greetInAWeek", new { day = "Sunday" });
            Assert.AreEqual(2, evaled.Count);
            expectedResults = new List<string>() { "Nice Sunday!", "Happy Sunday!" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));
        }

        [TestMethod]
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
            Assert.AreEqual(2, evaled.Count);
            Assert.AreEqual("You have 2 alarms, they are 8 pm at tomorrow", evaled[0]);
            Assert.AreEqual("You have 2 alarms, they are 8 pm of tomorrow", evaled[1]);
        }

        [TestMethod]
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
            Assert.AreEqual(2, evaled.Count);
            var eval1Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm at tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm at tomorrow\n" };
            var eval2Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm of tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm of tomorrow\n" };
            Assert.AreEqual(true, eval1Options.Contains(evaled[0]));
            Assert.AreEqual(true, eval2Options.Contains(evaled[1]));
        }

        [TestMethod]
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

            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evalOptions.Contains(evaled[0]));

            evaled = templates.ExpandTemplate("T2");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "3" || evaled[0] == "5");

            evaled = templates.ExpandTemplate("T3");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "3" || evaled[0] == "5");

            evaled = templates.ExpandTemplate("T4");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "ey" || evaled[0] == "el");
        }

        [TestMethod]
        public void TestEvalExpression()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EvalExpression.lg"));

            var userName = "MS";
            var evaled = templates.Evaluate("template1", new { userName });
            Assert.AreEqual(evaled, "Hi MS");

            evaled = templates.Evaluate("template2", new { userName });
            Assert.AreEqual(evaled, "Hi MS");

            evaled = templates.Evaluate("template3", new { userName });
            Assert.AreEqual(evaled, "HiMS");

            evaled = templates.Evaluate("template4", new { userName });
            var eval1Options = new List<string>() { "\r\nHi MS\r\n", "\nHi MS\n" };
            Assert.IsTrue(eval1Options.Contains(evaled));

            evaled = templates.Evaluate("template5", new { userName });
            var eval2Options = new List<string>() { "\r\nHiMS\r\n", "\nHiMS\n" };
            Assert.IsTrue(eval2Options.Contains(evaled));

            evaled = templates.Evaluate("template6", new { userName });
            Assert.AreEqual(evaled, "goodmorning");
        }

        [TestMethod]
        public void TestLGResource()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("2.lg"));

            Assert.AreEqual(templates.Count, 1);
            Assert.AreEqual(templates.Imports.Count, 0);
            Assert.AreEqual(templates[0].Name, "wPhrase");
            Assert.AreEqual(templates[0].Body.Replace("\r\n", "\n"), "> this is an in-template comment\n- Hi\n- Hello\n- Hiya\n- Hi");

            templates.AddTemplate("newtemplate", new List<string> { "age", "name" }, "- hi ");
            Assert.AreEqual(templates.Count, 2);
            Assert.AreEqual(templates.Imports.Count, 0);
            Assert.AreEqual(templates[1].Name, "newtemplate");
            Assert.AreEqual(templates[1].Parameters.Count, 2);
            Assert.AreEqual(templates[1].Parameters[0], "age");
            Assert.AreEqual(templates[1].Parameters[1], "name");
            Assert.AreEqual(templates[1].Body.Replace("\r\n", "\n"), "- hi \n");

            templates.AddTemplate("newtemplate2", null, "- hi2 ");
            Assert.AreEqual(templates.Count, 3);
            Assert.AreEqual(templates[2].Name, "newtemplate2");
            Assert.AreEqual(templates[2].Body.Replace("\r\n", "\n"), "- hi2 \n");

            templates.UpdateTemplate("newtemplate", "newtemplateName", new List<string> { "newage", "newname" }, "- new hi\r\n#hi");
            Assert.AreEqual(templates.Count, 3);
            Assert.AreEqual(templates.Imports.Count, 0);
            Assert.AreEqual(templates[1].Name, "newtemplateName");
            Assert.AreEqual(templates[1].Parameters.Count, 2);
            Assert.AreEqual(templates[1].Parameters[0], "newage");
            Assert.AreEqual(templates[1].Parameters[1], "newname");
            Assert.AreEqual(templates[1].Body.Replace("\r\n", "\n"), "- new hi\n- #hi\n");

            templates.UpdateTemplate("newtemplate2", "newtemplateName2", new List<string> { "newage2", "newname2" }, "- new hi\r\n#hi2");
            Assert.AreEqual(templates.Count, 3);
            Assert.AreEqual(templates.Imports.Count, 0);
            Assert.AreEqual(templates[2].Name, "newtemplateName2");
            Assert.AreEqual(templates[2].Body.Replace("\r\n", "\n"), "- new hi\n- #hi2\n");

            templates.DeleteTemplate("newtemplateName");
            Assert.AreEqual(templates.Count, 2);

            templates.DeleteTemplate("newtemplateName2");
            Assert.AreEqual(templates.Count, 1);
        }

        [TestMethod]
        public void TestMemoryScope()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("MemoryScope.lg"));
            var evaled = templates.Evaluate("T1", new { turn = new { name = "Dong", count = 3 } });
            Assert.AreEqual(evaled, "Hi Dong, welcome to Seattle, Seattle is a beautiful place, how many burgers do you want, 3?");

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

            Assert.AreEqual(evaled, "Which Bread, A or B do you want?");
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }

        [TestMethod]
        public void TestStructuredTemplate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("StructuredTemplate.lg"));

            var evaled = templates.Evaluate("AskForAge.prompt");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"how old are you?\"}"), evaled as JObject)
                || JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"what's your age?\"}"), evaled as JObject));

            evaled = templates.Evaluate("AskForAge.prompt2");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"suggestedactions\":[\"10\",\"20\",\"30\"]}"), evaled as JObject)
                || JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"suggestedactions\":[\"10\",\"20\",\"30\"]}"), evaled as JObject));

            evaled = templates.Evaluate("AskForAge.prompt3");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"${GetAge()}\",\"suggestions\":[\"10 | cards\",\"20 | cards\"]}"), evaled as JObject));

            evaled = templates.Evaluate("T1");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"This is awesome\",\"speak\":\"foo bar I can also speak!\"}"), evaled as JObject));

            evaled = templates.Evaluate("ST1");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"text\":\"foo\",\"speak\":\"bar\"}"), evaled as JObject));

            evaled = templates.Evaluate("AskForColor");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"suggestedactions\":[{\"lgType\":\"MyStruct\",\"speak\":\"bar\",\"text\":\"zoo\"},{\"lgType\":\"Activity\",\"speak\":\"I can also speak!\"}]}"), evaled as JObject));

            evaled = templates.Evaluate("MultiExpression");
            var options = new string[]
            {
                "{\r\n  \"lgType\": \"Activity\",\r\n  \"speak\": \"I can also speak!\"\r\n} {\r\n  \"lgType\": \"MyStruct\",\r\n  \"text\": \"hi\"\r\n}",
                "{\n  \"lgType\": \"Activity\",\n  \"speak\": \"I can also speak!\"\n} {\n  \"lgType\": \"MyStruct\",\n  \"text\": \"hi\"\n}"
            };
            Assert.IsTrue(options.Contains(evaled.ToString()));

            evaled = templates.Evaluate("StructuredTemplateRef");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"text\":\"hi\"}"), evaled as JObject));

            evaled = templates.Evaluate("MultiStructuredRef");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"list\":[{\"lgType\":\"SubStruct\",\"text\":\"hello\"},{\"lgType\":\"SubStruct\",\"text\":\"world\"}]}"), evaled as JObject));

            evaled = templates.Evaluate("templateWithSquareBrackets", new { manufacturer = new { Name = "Acme Co" } });

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Struct\",\"text\":\"Acme Co\"}"), evaled as JObject));

            evaled = templates.Evaluate("ValueWithEqualsMark", new { name = "Jack" });

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\": \"Activity\",\"text\": \"Hello! welcome back. I have your name = Jack\"}"), evaled as JObject));
        }

        [TestMethod]
        public void TestEvaluateOnce()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EvaluateOnce.lg"));

            var evaled = templates.Evaluate("templateWithSameParams", new { param = "ms" });
            Assert.IsNotNull(evaled);

            var resultList = evaled.ToString().Split(" ");
            Assert.IsTrue(resultList.Length == 2);
            Assert.IsTrue(resultList[0] == resultList[1]);

            // may be has different values
            evaled = templates.Evaluate("templateWithDifferentParams", new { param1 = "ms", param2 = "newms" });
        }

        [TestMethod]
        public void TestReExecute()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ReExecute.lg"));

            // may be has different values
            var evaled = templates.Evaluate("templateWithSameParams", new { param1 = "ms", param2 = "newms" });
        }

        [TestMethod]
        public void TestConditionExpression()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ConditionExpression.lg"));

            var evaled = templates.Evaluate("conditionTemplate", new { num = 1 });

            Assert.AreEqual(evaled, "Your input is one");

            evaled = templates.Evaluate("conditionTemplate", new { num = 2 });

            Assert.AreEqual(evaled, "Your input is two");

            evaled = templates.Evaluate("conditionTemplate", new { num = 3 });

            Assert.AreEqual(evaled, "Your input is three");

            evaled = templates.Evaluate("conditionTemplate", new { num = 4 });

            Assert.AreEqual(evaled, "Your input is not one, two or three");
        }

        [TestMethod]
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

        [TestMethod]
        public void TestExpandTemplateWithStructuredLG()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("StructuredTemplate.lg"));

            // without scope
            var evaled = templates.ExpandTemplate("AskForAge.prompt");
            Assert.AreEqual(4, evaled.Count);
            var expectedResults = new List<string>()
            {
                "{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"what's your age?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"what's your age?\"}"
            };

            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            evaled = templates.ExpandTemplate("ExpanderT1");
            Assert.AreEqual(4, evaled.Count);
            expectedResults = new List<string>()
            {
                "{\"lgType\":\"MyStruct\",\"text\":\"Hi\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hi\",\"speak\":\"what's your age?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hello\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"MyStruct\",\"text\":\"Hello\",\"speak\":\"what's your age?\"}"
            };

            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));
        }

        [TestMethod]
        public void TestExpressionextract()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("ExpressionExtract.lg"));

            var evaled1 = templates.Evaluate("templateWithBrackets");
            var evaled2 = templates.Evaluate("templateWithBrackets2");
            var evaled3 = templates.Evaluate("templateWithBrackets3").ToString().Trim();
            var espectedResult = "don't mix {} and '{}'";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);

            evaled1 = templates.Evaluate("templateWithQuotationMarks");
            evaled2 = templates.Evaluate("templateWithQuotationMarks2");
            evaled3 = templates.Evaluate("templateWithQuotationMarks3").ToString().Trim();
            espectedResult = "don't mix {\"} and \"\"'\"";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);

            evaled1 = templates.Evaluate("templateWithUnpairedBrackets1");
            evaled2 = templates.Evaluate("templateWithUnpairedBrackets12");
            evaled3 = templates.Evaluate("templateWithUnpairedBrackets13").ToString().Trim();
            espectedResult = "{prefix 5 sufix";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);

            evaled1 = templates.Evaluate("templateWithUnpairedBrackets2");
            evaled2 = templates.Evaluate("templateWithUnpairedBrackets22");
            evaled3 = templates.Evaluate("templateWithUnpairedBrackets23").ToString().Trim();
            espectedResult = "prefix 5 sufix}";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);
        }

        [TestMethod]
        public void TestStringInterpolation()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("StringInterpolation.lg"));

            var evaled = templates.Evaluate("simpleStringTemplate");
            Assert.AreEqual("say hi", evaled);

            evaled = templates.Evaluate("StringTemplateWithVariable", new { w = "world" });
            Assert.AreEqual("hello world", evaled);

            evaled = templates.Evaluate("StringTemplateWithMixing", new { name = "jack" });
            Assert.AreEqual("I know your name is jack", evaled);

            evaled = templates.Evaluate("StringTemplateWithJson", new { h = "hello", w = "world" });
            Assert.AreEqual("get 'h' value : hello", evaled);

            evaled = templates.Evaluate("StringTemplateWithEscape");
            Assert.AreEqual("just want to output ${bala`bala}", evaled);

            evaled = templates.Evaluate("StringTemplateWithTemplateRef");
            Assert.AreEqual("hello jack , welcome. nice weather!", evaled);
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
            Assert.AreEqual(result, "p1enum");

            // this evaulate will hit memory access twice
            // first for "myProperty.name", and get "p1", from global
            // sencond for "turn.property[p1].enum" and get "p1enum" from global 
            result = templates.Evaluate("T3", memory);
            Assert.AreEqual(result, "p1enum");
        }

        [TestMethod]
        public void TestIsTemplateFunction()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("IsTemplate.lg"));

            var evaled = templates.Evaluate("template2", new { templateName = "template1" });
            Assert.AreEqual("template template1 exists", evaled);

            evaled = templates.Evaluate("template2", new { templateName = "wPhrase" });
            Assert.AreEqual("template wPhrase exists", evaled);

            evaled = templates.Evaluate("template2", new { templateName = "xxx" });
            Assert.AreEqual("template xxx does not exist", evaled);
        }

        [TestMethod]
        public void TestEmptyArratAndObject()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("EmptyArrayAndObject.lg"));

            var evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new { } });
            Assert.AreEqual("list and obj are both empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new Dictionary<string, object>() });
            Assert.AreEqual("list and obj are both empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { "hi" }, obj = new { } });
            Assert.AreEqual("obj is empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new { a = "a" } });
            Assert.AreEqual("list is empty", evaled);

            evaled = templates.Evaluate("template", new { list = new List<string> { }, obj = new Dictionary<string, object> { { "a", "b" } } });
            Assert.AreEqual("list is empty", evaled);

            evaled = templates.Evaluate("template", new { list = new JArray() { new JObject() }, obj = new JObject { ["a"] = "b" } });
            Assert.AreEqual("list and obj are both not empty.", evaled);
        }

        [TestMethod]
        public void TestNullTolerant()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("NullTolerant.lg"));

            var evaled = templates.Evaluate("template1");

            Assert.AreEqual("null", evaled);

            evaled = templates.Evaluate("template2");

            Assert.AreEqual("result is 'null'", evaled);

            var jObjEvaled = templates.Evaluate("template3") as JObject;

            Assert.AreEqual("null", jObjEvaled["key1"]);
        }

        [TestMethod]
        public void TestInlineEvaluate()
        {
            var templates = Templates.ParseFile(GetExampleFilePath("2.lg"));
            var evaled = templates.EvaluateText("hello");
            Assert.AreEqual("hello", evaled);

            // test template reference
            evaled = templates.EvaluateText("${wPhrase()}");
            var options = new List<string> { "Hi", "Hello", "Hiya" };
            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");

            var exception = Assert.ThrowsException<Exception>(() => templates.EvaluateText("${ErrrorTemplate()}"));
            Assert.IsTrue(exception.Message.Contains("it's not a built-in function or a custom function"));
        }

        [TestMethod]
        public void TestCustomFunction()
        {
            var parser = new ExpressionParser((string func) =>
            { 
                if (func == "custom")
                {
                    return ExpressionFunctions.Numeric("custom", (args) => (int)args[0] + (int)args[1]);
                }
                else
                {
                    return Expression.Lookup(func);
                }
            });
            var templates = Templates.ParseFile(GetExampleFilePath("CustomFunction.lg"), null, parser);
            var evaled = templates.Evaluate("template");
            Assert.AreEqual(3, evaled);
            evaled = templates.Evaluate("callRef");
            Assert.AreEqual(12, evaled);
        }

        [TestMethod]
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
            Assert.AreEqual(6.0, evaled);
        }

        public class LoopClass
        {
            public string Name { get; set; }

            public object LoopObj { get; set; }
        }
    }
}
