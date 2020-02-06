// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1202 // Elements should be ordered by access
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions.Memory;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class LGFileTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestBasic()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("2.lg"));

            var evaled = lgFile.EvaluateTemplate("wPhrase");
            var options = new List<string> { "Hi", "Hello", "Hiya" };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("3.lg"));

            var evaled = lgFile.EvaluateTemplate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)" };

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = lgFile.EvaluateTemplate("welcome-user", new { userName = userName }).ToString();
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName), $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestIfElseTemplate()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("5.lg"));

            string evaled = lgFile.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "morning" }).ToString();
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = lgFile.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "evening" }).ToString();
            Assert.IsTrue(evaled == "Good evening" || evaled == "Evening! ", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicConditionalTemplateWithoutDefault()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("5.lg"));

            string evaled = lgFile.EvaluateTemplate("time-of-day-readout-without-default", new { timeOfDay = "morning" }).ToString();
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = lgFile.EvaluateTemplate("time-of-day-readout-without-default2", new { timeOfDay = "morning" }).ToString();
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            object evaledNull = lgFile.EvaluateTemplate("time-of-day-readout-without-default2", new { timeOfDay = "evening" });
            Assert.IsNull(evaledNull, "Evaled is not null");
        }

        [TestMethod]
        public void TestBasicSwitchCaseTemplate()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("switchcase.lg"));

            string evaled = lgFile.EvaluateTemplate("greetInAWeek", new { day = "Saturday" }).ToString();
            Assert.IsTrue(evaled == "Happy Saturday!");

            evaled = lgFile.EvaluateTemplate("greetInAWeek", new { day = "Monday" }).ToString();
            Assert.IsTrue(evaled == "Work Hard!");
        }

        [TestMethod]
        public void TestBasicTemplateRefWithParameters()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("6.lg"));
            string evaled = lgFile.EvaluateTemplate("welcome", null).ToString();
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = lgFile.EvaluateTemplate("welcome", new { userName = "DL" }).ToString();
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [TestMethod]
        public void TestBasicListSupport()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("BasicList.lg"));
            Assert.AreEqual(lgFile.EvaluateTemplate("BasicJoin", new { items = new[] { "1" } }), "1");
            Assert.AreEqual(lgFile.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2" } }), "1, 2");
            Assert.AreEqual(lgFile.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2", "3" } }), "1, 2 and 3");
        }

        [TestMethod]
        public void TestBasicExtendedFunctions()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("6.lg"));
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

            var evaled = lgFile.EvaluateTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            evaled = lgFile.EvaluateTemplate("ShowAlarmsWithLgTemplate", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            evaled = lgFile.EvaluateTemplate("ShowAlarmsWithDynamicLgTemplate", new { alarms = alarms, templateName = "ShowAlarm" });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = lgFile.EvaluateTemplate("ShowAlarmsWithMemberForeach", new { alarms = alarms });
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = lgFile.EvaluateTemplate("ShowAlarmsWithHumanize", new { alarms = alarms });
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            // var evaled = lgFile.EvaluateTemplate("ShowAlarmsWithMemberHumanize", new { alarms = alarms });
            // Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);
        }

        [TestMethod]
        public void TestCaseInsensitive()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("CaseInsensitive.lg"));
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

            var evaled = lgFile.EvaluateTemplate("ShowAlarms", new { alarms = alarms });
            Assert.AreEqual("You have two alarms", evaled);

            evaled = lgFile.EvaluateTemplate("greetInAWeek", new { day = "Saturday" });
            Assert.AreEqual("Happy Saturday!", evaled);
        }

        [TestMethod]
        public void TestListWithOnlyOneElement()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("8.lg"));
            var evaled = lgFile.EvaluateTemplate("ShowTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual("Your most recent task is Task1. You can let me know if you want to add or complete a task.", evaled);
        }

        [TestMethod]
        public void TestTemplateNameWithDotIn()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("TemplateNameWithDot.lg"));
            Assert.AreEqual(lgFile.EvaluateTemplate("Hello.World", null), "Hello World");
            Assert.AreEqual(lgFile.EvaluateTemplate("Hello", null), "Hello World");
        }

        [TestMethod]
        public void TestMultiLine()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("MultilineTextForAdaptiveCard.lg"));
            var evaled1 = lgFile.EvaluateTemplate("wPhrase", string.Empty);
            var options1 = new List<string> { "\r\ncardContent\r\n", "hello", "\ncardContent\n" };
            Assert.IsTrue(options1.Contains(evaled1), $"Evaled is {evaled1}");

            var evaled2 = lgFile.EvaluateTemplate("nameTemplate", new { name = "N" });
            var options2 = new List<string> { "\r\nN\r\n", "N", "\nN\n" };
            Assert.IsTrue(options2.Contains(evaled2), $"Evaled is {evaled2}");

            var evaled3 = lgFile.EvaluateTemplate("adaptivecardsTemplate", string.Empty);

            var evaled4 = lgFile.EvaluateTemplate("refTemplate", string.Empty);
            var options4 = new List<string> { "\r\nhi\r\n", "\nhi\n" };
            Assert.IsTrue(options4.Contains(evaled4), $"Evaled is {evaled4}");
        }

        [TestMethod]
        public void TestTemplateRef()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("TemplateRef.lg"));

            var scope = new
            {
                time = "morning",
                name = "Dong Lei"
            };
            Assert.AreEqual(lgFile.EvaluateTemplate("Hello", scope), "Good morning Dong Lei");
        }

        [TestMethod]
        public void TestEscapeCharacter()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("EscapeCharacter.lg"));
            var evaled = lgFile.EvaluateTemplate("wPhrase", null);
            Assert.AreEqual(evaled, "Hi \r\n\t[]{}\\");

            evaled = lgFile.EvaluateTemplate("AtEscapeChar", null);
            Assert.AreEqual(evaled, "Hi{1+1}[wPhrase]{wPhrase()}@{wPhrase()}2@{1+1} ");

            evaled = lgFile.EvaluateTemplate("otherEscape", null);
            Assert.AreEqual(evaled, "Hi y ");

            evaled = lgFile.EvaluateTemplate("escapeInExpression", null);
            Assert.AreEqual(evaled, "Hi hello\\\\");

            evaled = lgFile.EvaluateTemplate("escapeInExpression2", null);
            Assert.AreEqual(evaled, "Hi hello'");

            evaled = lgFile.EvaluateTemplate("escapeInExpression3", null);
            Assert.AreEqual(evaled, "Hi hello\"");

            evaled = lgFile.EvaluateTemplate("escapeInExpression4", null);
            Assert.AreEqual(evaled, "Hi hello\"");

            evaled = lgFile.EvaluateTemplate("escapeInExpression5", null);
            Assert.AreEqual(evaled, "Hi hello\n");

            evaled = lgFile.EvaluateTemplate("escapeInExpression6", null);
            Assert.AreEqual(evaled, "Hi hello\n");

            var todos = new[] { "A", "B", "C" };
            evaled = lgFile.EvaluateTemplate("showTodo", new { todos });
            Assert.AreEqual(((string)evaled).Replace("\r\n", "\n"), "\n    Your most recent 3 tasks are\n    * A\n* B\n* C\n    ");
            
            evaled = lgFile.EvaluateTemplate("showTodo", null);
            Assert.AreEqual(((string)evaled).Replace("\r\n", "\n"), "\n    You don't have any \"t\\\\odo'\".\n    ");
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
                var lgFile = LGParser.ParseFile(GetExampleFilePath("analyzer.lg"));
                var evaled1 = lgFile.AnalyzeTemplate(testItem.GetType().GetProperty("name").GetValue(testItem).ToString());
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
            var lgFile = LGParser.ParseFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = lgFile.EvaluateTemplate("TemplateC", string.Empty);
            var options = new List<string> { "Hi", "Hello" };
            Assert.AreEqual(options.Contains(evaled), true);

            evaled = lgFile.EvaluateTemplate("TemplateD", new { b = "morning" });
            options = new List<string> { "Hi morning", "Hello morning" };
            Assert.AreEqual(options.Contains(evaled), true);
        }

        [TestMethod]
        public void TestTemplateAsFunction()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("TemplateAsFunction.lg"));
            string evaled = lgFile.EvaluateTemplate("Test2", string.Empty).ToString();

            Assert.AreEqual(evaled, "hello world");

            evaled = lgFile.EvaluateTemplate("Test3", string.Empty).ToString();
            Assert.AreEqual(evaled, "hello world");

            evaled = lgFile.EvaluateTemplate("Test4", string.Empty).ToString();

            Assert.AreEqual(evaled.Trim(), "hello world");

            evaled = lgFile.EvaluateTemplate("dupNameWithTemplate").ToString();
            Assert.AreEqual(evaled, "calculate length of ms by user's template");

            evaled = lgFile.EvaluateTemplate("dupNameWithBuiltinFunc").ToString();
            Assert.AreEqual(evaled, "2");
        }

        [TestMethod]
        public void TestAnalyzelgTemplateFunction()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = lgFile.AnalyzeTemplate("TemplateD");
            var variableEvaled = evaled.Variables;
            var options = new List<string>() { "b" };
            Assert.AreEqual(variableEvaled.Count, options.Count);
            options.ForEach(e => Assert.AreEqual(variableEvaled.Contains(e), true));
        }

        [TestMethod]
        public void TestImportLgFiles()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("import.lg"));

            // Assert 6.lg is imported only once when there are several relative paths which point to the same file.
            // Assert import cycle loop is handled well as expected when a file imports itself.
            Assert.AreEqual(14, lgFile.AllTemplates.Count());

            string evaled = lgFile.EvaluateTemplate("basicTemplate", null).ToString();
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = lgFile.EvaluateTemplate("welcome", null).ToString();
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = lgFile.EvaluateTemplate("template3", null).ToString();
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            evaled = lgFile.EvaluateTemplate("welcome", new { userName = "DL" }).ToString();
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");

            evaled = lgFile.EvaluateTemplate("basicTemplate2", null).ToString();
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            // Assert 6.lg of relative path is imported from text.
            lgFile = LGParser.ParseText("# basicTemplate\r\n- Hi\r\n- Hello\r\n[import](./6.lg)", GetExampleFilePath("xx.lg"));

            Assert.AreEqual(8, lgFile.AllTemplates.Count());
            evaled = lgFile.EvaluateTemplate("basicTemplate", null).ToString();
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = lgFile.EvaluateTemplate("welcome", null).ToString();
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = lgFile.EvaluateTemplate("welcome", new { userName = "DL" }).ToString();
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [TestMethod]
        public void TestRegex()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("Regex.lg"));
            var evaled = lgFile.EvaluateTemplate("wPhrase", string.Empty);
            Assert.AreEqual(evaled, "Hi");

            evaled = lgFile.EvaluateTemplate("wPhrase", new { name = "jack" });
            Assert.AreEqual(evaled, "Hi jack");

            evaled = lgFile.EvaluateTemplate("wPhrase", new { name = "morethanfive" });
            Assert.AreEqual(evaled, "Hi");
        }

        [TestMethod]
        public void TestExpandTemplate()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("Expand.lg"));

            // without scope
            var evaled = lgFile.ExpandTemplate("FinalGreeting");
            Assert.AreEqual(4, evaled.Count);
            var expectedResults = new List<string>() { "Hi Morning", "Hi Evening", "Hello Morning", "Hello Evening" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            // with scope
            evaled = lgFile.ExpandTemplate("TimeOfDayWithCondition", new { time = "evening" });
            Assert.AreEqual(2, evaled.Count);
            expectedResults = new List<string>() { "Hi Evening", "Hello Evening" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            // with scope
            evaled = lgFile.ExpandTemplate("greetInAWeek", new { day = "Sunday" });
            Assert.AreEqual(2, evaled.Count);
            expectedResults = new List<string>() { "Nice Sunday!", "Happy Sunday!" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));
        }

        [TestMethod]
        public void TestExpandTemplateWithRef()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("Expand.lg"));

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

            var evaled = lgFile.ExpandTemplate("ShowAlarmsWithLgTemplate", new { alarms = alarms });
            Assert.AreEqual(2, evaled.Count);
            Assert.AreEqual("You have 2 alarms, they are 8 pm at tomorrow", evaled[0]);
            Assert.AreEqual("You have 2 alarms, they are 8 pm of tomorrow", evaled[1]);
        }

        [TestMethod]
        public void TestExpandTemplateWithRefInMultiLine()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("Expand.lg"));

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

            var evaled = lgFile.ExpandTemplate("ShowAlarmsWithMultiLine", new { alarms = alarms });
            Assert.AreEqual(2, evaled.Count);
            var eval1Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm at tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm at tomorrow\n" };
            var eval2Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm of tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm of tomorrow\n" };
            Assert.AreEqual(true, eval1Options.Contains(evaled[0]));
            Assert.AreEqual(true, eval2Options.Contains(evaled[1]));
        }

        [TestMethod]
        public void TestExpandTemplateWithFunction()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("Expand.lg"));

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

            var evaled = lgFile.ExpandTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            var evalOptions = new List<string>()
            {
                "You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow",
                "You have 2 alarms, 7 am at tomorrow and 8 pm of tomorrow",
                "You have 2 alarms, 7 am of tomorrow and 8 pm at tomorrow",
                "You have 2 alarms, 7 am of tomorrow and 8 pm of tomorrow"
            };

            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evalOptions.Contains(evaled[0]));

            evaled = lgFile.ExpandTemplate("T2");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "3" || evaled[0] == "5");

            evaled = lgFile.ExpandTemplate("T3");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "3" || evaled[0] == "5");

            evaled = lgFile.ExpandTemplate("T4");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "ey" || evaled[0] == "el");
        }

        [TestMethod]
        public void TestEvalExpression()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("EvalExpression.lg"));

            var userName = "MS";
            var evaled = lgFile.EvaluateTemplate("template1", new { userName });
            Assert.AreEqual(evaled, "Hi MS");

            evaled = lgFile.EvaluateTemplate("template2", new { userName });
            Assert.AreEqual(evaled, "Hi MS");

            evaled = lgFile.EvaluateTemplate("template3", new { userName });
            Assert.AreEqual(evaled, "HiMS");

            evaled = lgFile.EvaluateTemplate("template4", new { userName });
            var eval1Options = new List<string>() { "\r\nHi MS\r\n", "\nHi MS\n" };
            Assert.IsTrue(eval1Options.Contains(evaled));

            evaled = lgFile.EvaluateTemplate("template5", new { userName });
            var eval2Options = new List<string>() { "\r\nHiMS\r\n", "\nHiMS\n" };
            Assert.IsTrue(eval2Options.Contains(evaled));

            evaled = lgFile.EvaluateTemplate("template6", new { userName });
            Assert.AreEqual(evaled, "goodmorning");
        }

        [TestMethod]
        public void TestLGResource()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("2.lg"));

            Assert.AreEqual(lgFile.Templates.Count, 1);
            Assert.AreEqual(lgFile.Imports.Count, 0);
            Assert.AreEqual(lgFile.Templates[0].Name, "wPhrase");
            Assert.AreEqual(lgFile.Templates[0].Body.Replace("\r\n", "\n"), "- Hi\n- Hello\n- Hiya\n- Hi");

            lgFile.AddTemplate("newtemplate", new List<string> { "age", "name" }, "- hi ");
            Assert.AreEqual(lgFile.Templates.Count, 2);
            Assert.AreEqual(lgFile.Imports.Count, 0);
            Assert.AreEqual(lgFile.Templates[1].Name, "newtemplate");
            Assert.AreEqual(lgFile.Templates[1].Parameters.Count, 2);
            Assert.AreEqual(lgFile.Templates[1].Parameters[0], "age");
            Assert.AreEqual(lgFile.Templates[1].Parameters[1], "name");
            Assert.AreEqual(lgFile.Templates[1].Body, "- hi ");

            lgFile.AddTemplate("newtemplate2", null, "- hi2 ");
            Assert.AreEqual(lgFile.Templates.Count, 3);
            Assert.AreEqual(lgFile.Templates[2].Name, "newtemplate2");
            Assert.AreEqual(lgFile.Templates[2].Body, "- hi2 ");

            lgFile.UpdateTemplate("newtemplate", "newtemplateName", new List<string> { "newage", "newname" }, "- new hi\r\n#hi");
            Assert.AreEqual(lgFile.Templates.Count, 3);
            Assert.AreEqual(lgFile.Imports.Count, 0);
            Assert.AreEqual(lgFile.Templates[1].Name, "newtemplateName");
            Assert.AreEqual(lgFile.Templates[1].Parameters.Count, 2);
            Assert.AreEqual(lgFile.Templates[1].Parameters[0], "newage");
            Assert.AreEqual(lgFile.Templates[1].Parameters[1], "newname");
            Assert.AreEqual(lgFile.Templates[1].Body, "- new hi\r\n- #hi");

            lgFile.UpdateTemplate("newtemplate2", "newtemplateName2", new List<string> { "newage2", "newname2" }, "- new hi\r\n#hi2");
            Assert.AreEqual(lgFile.Templates.Count, 3);
            Assert.AreEqual(lgFile.Imports.Count, 0);
            Assert.AreEqual(lgFile.Templates[2].Name, "newtemplateName2");
            Assert.AreEqual(lgFile.Templates[2].Body, "- new hi\r\n- #hi2");

            lgFile.DeleteTemplate("newtemplateName");
            Assert.AreEqual(lgFile.Templates.Count, 2);

            lgFile.DeleteTemplate("newtemplateName2");
            Assert.AreEqual(lgFile.Templates.Count, 1);
        }

        [TestMethod]
        public void TestMemoryScope()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("MemoryScope.lg"));
            var evaled = lgFile.EvaluateTemplate("T1", new { turn = new { name = "Dong", count = 3 } });
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

            evaled = lgFile.EvaluateTemplate("AskBread", scope);

            Assert.AreEqual(evaled, "Which Bread, A or B do you want?");
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }

        [TestMethod]
        public void TestStructuredTemplate()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("StructuredTemplate.lg"));

            var evaled = lgFile.EvaluateTemplate("AskForAge.prompt");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"how old are you?\"}"), evaled as JObject)
                || JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"what's your age?\"}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("AskForAge.prompt2");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"suggestedactions\":[\"10\",\"20\",\"30\"]}"), evaled as JObject)
                || JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"suggestedactions\":[\"10\",\"20\",\"30\"]}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("AskForAge.prompt3");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"@{GetAge()}\",\"suggestions\":[\"10 | cards\",\"20 | cards\"]}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("T1");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"text\":\"This is awesome\",\"speak\":\"foo bar I can also speak!\"}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("ST1");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"text\":\"foo\",\"speak\":\"bar\"}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("AskForColor");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Activity\",\"suggestedactions\":[{\"lgType\":\"MyStruct\",\"speak\":\"bar\",\"text\":\"zoo\"},{\"lgType\":\"Activity\",\"speak\":\"I can also speak!\"}]}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("MultiExpression");
            var options = new string[]
            {
                "{\r\n  \"lgType\": \"Activity\",\r\n  \"speak\": \"I can also speak!\"\r\n} {\r\n  \"lgType\": \"MyStruct\",\r\n  \"text\": \"hi\"\r\n}",
                "{\n  \"lgType\": \"Activity\",\n  \"speak\": \"I can also speak!\"\n} {\n  \"lgType\": \"MyStruct\",\n  \"text\": \"hi\"\n}"
            };
            Assert.IsTrue(options.Contains(evaled.ToString()));

            evaled = lgFile.EvaluateTemplate("StructuredTemplateRef");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"text\":\"hi\"}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("MultiStructuredRef");

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"MyStruct\",\"list\":[{\"lgType\":\"SubStruct\",\"text\":\"hello\"},{\"lgType\":\"SubStruct\",\"text\":\"world\"}]}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("templateWithSquareBrackets", new { manufacturer = new { Name = "Acme Co" } });

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\":\"Struct\",\"text\":\"Acme Co\"}"), evaled as JObject));

            evaled = lgFile.EvaluateTemplate("ValueWithEqualsMark", new { name = "Jack" });

            Assert.IsTrue(
                JToken.DeepEquals(JObject.Parse("{\"lgType\": \"Activity\",\"text\": \"Hello! welcome back. I have your name = Jack\"}"), evaled as JObject));
        }

        [TestMethod]
        public void TestEvaluateOnce()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("EvaluateOnce.lg"));

            var evaled = lgFile.EvaluateTemplate("templateWithSameParams", new { param = "ms" });
            Assert.IsNotNull(evaled);

            var resultList = evaled.ToString().Split(" ");
            Assert.IsTrue(resultList.Length == 2);
            Assert.IsTrue(resultList[0] == resultList[1]);

            // may be has different values
            evaled = lgFile.EvaluateTemplate("templateWithDifferentParams", new { param1 = "ms", param2 = "newms" });
        }

        [TestMethod]
        public void TestReExecute()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("ReExecute.lg"));

            // may be has different values
            var evaled = lgFile.EvaluateTemplate("templateWithSameParams", new { param1 = "ms", param2 = "newms" });
        }

        [TestMethod]
        public void TestConditionExpression()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("ConditionExpression.lg"));

            var evaled = lgFile.EvaluateTemplate("conditionTemplate", new { num = 1 });

            Assert.AreEqual(evaled, "Your input is one");

            evaled = lgFile.EvaluateTemplate("conditionTemplate", new { num = 2 });

            Assert.AreEqual(evaled, "Your input is two");

            evaled = lgFile.EvaluateTemplate("conditionTemplate", new { num = 3 });

            Assert.AreEqual(evaled, "Your input is three");

            evaled = lgFile.EvaluateTemplate("conditionTemplate", new { num = 4 });

            Assert.AreEqual(evaled, "Your input is not one, two or three");
        }

        [TestMethod]
        public void TestLoopScope()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("LoopScope.lg"));

            var loopClass1 = new LoopClass();
            loopClass1.Name = "jack";

            var loopClass2 = new LoopClass();
            loopClass2.Name = "jones";

            loopClass1.LoopObj = loopClass2;
            loopClass2.LoopObj = loopClass1;

            lgFile.EvaluateTemplate("template1", new { scope = loopClass1 });
        }

        [TestMethod]
        public void TestExpandTemplateWithStructuredLG()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("StructuredTemplate.lg"));

            // without scope
            var evaled = lgFile.ExpandTemplate("AskForAge.prompt");
            Assert.AreEqual(4, evaled.Count);
            var expectedResults = new List<string>()
            {
                "{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"how old are you?\",\"speak\":\"what's your age?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"how old are you?\"}",
                "{\"lgType\":\"Activity\",\"text\":\"what's your age?\",\"speak\":\"what's your age?\"}"
            };

            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            evaled = lgFile.ExpandTemplate("ExpanderT1");
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
            var lgFile = LGParser.ParseFile(GetExampleFilePath("ExpressionExtract.lg"));

            var evaled1 = lgFile.EvaluateTemplate("templateWithBrackets");
            var evaled2 = lgFile.EvaluateTemplate("templateWithBrackets2");
            var evaled3 = lgFile.EvaluateTemplate("templateWithBrackets3").ToString().Trim();
            var espectedResult = "don't mix {} and '{}'";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);

            evaled1 = lgFile.EvaluateTemplate("templateWithQuotationMarks");
            evaled2 = lgFile.EvaluateTemplate("templateWithQuotationMarks2");
            evaled3 = lgFile.EvaluateTemplate("templateWithQuotationMarks3").ToString().Trim();
            espectedResult = "don't mix {\"} and \"\"'\"";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);

            evaled1 = lgFile.EvaluateTemplate("templateWithUnpairedBrackets1");
            evaled2 = lgFile.EvaluateTemplate("templateWithUnpairedBrackets12");
            evaled3 = lgFile.EvaluateTemplate("templateWithUnpairedBrackets13").ToString().Trim();
            espectedResult = "{prefix 5 sufix";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);

            evaled1 = lgFile.EvaluateTemplate("templateWithUnpairedBrackets2");
            evaled2 = lgFile.EvaluateTemplate("templateWithUnpairedBrackets22");
            evaled3 = lgFile.EvaluateTemplate("templateWithUnpairedBrackets23").ToString().Trim();
            espectedResult = "prefix 5 sufix}";
            Assert.AreEqual(evaled1, espectedResult);
            Assert.AreEqual(evaled2, espectedResult);
            Assert.AreEqual(evaled3, espectedResult);
        }

        [TestMethod]
        public void TestStringInterpolation()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("StringInterpolation.lg"));

            var evaled = lgFile.EvaluateTemplate("simpleStringTemplate");
            Assert.AreEqual("say hi", evaled);

            evaled = lgFile.EvaluateTemplate("StringTemplateWithVariable", new { w = "world" });
            Assert.AreEqual("hello world", evaled);

            evaled = lgFile.EvaluateTemplate("StringTemplateWithMixing", new { name = "jack" });
            Assert.AreEqual("I know your name is jack", evaled);

            evaled = lgFile.EvaluateTemplate("StringTemplateWithJson", new { h = "hello", w = "world" });
            Assert.AreEqual("get 'h' value : hello", evaled);

            evaled = lgFile.EvaluateTemplate("StringTemplateWithEscape");
            Assert.AreEqual("just want to output @{bala`bala}", evaled);

            evaled = lgFile.EvaluateTemplate("StringTemplateWithTemplateRef");
            Assert.AreEqual("hello jack , welcome. nice weather!", evaled);
        }

        public void TestMemoryAccessPath()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("MemoryAccess.lg"));

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
            var result = lgFile.EvaluateTemplate("T1", memory);
            Assert.AreEqual(result, "p1enum");

            // this evaulate will hit memory access twice
            // first for "myProperty.name", and get "p1", from global
            // sencond for "turn.property[p1].enum" and get "p1enum" from global 
            result = lgFile.EvaluateTemplate("T3", memory);
            Assert.AreEqual(result, "p1enum");
        }

        [TestMethod]
        public void TestIsTemplateFunction()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("IsTemplate.lg"));

            var evaled = lgFile.EvaluateTemplate("template2", new { templateName = "template1" });
            Assert.AreEqual("template template1 exists", evaled);

            evaled = lgFile.EvaluateTemplate("template2", new { templateName = "wPhrase" });
            Assert.AreEqual("template wPhrase exists", evaled);

            evaled = lgFile.EvaluateTemplate("template2", new { templateName = "xxx" });
            Assert.AreEqual("template xxx does not exist", evaled);
        }

        [TestMethod]
        public void TestEmptyArratAndObject()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("EmptyArrayAndObject.lg"));

            var evaled = lgFile.EvaluateTemplate("template", new { list = new List<string> { }, obj = new { } });
            Assert.AreEqual("list and obj are both empty", evaled);

            evaled = lgFile.EvaluateTemplate("template", new { list = new List<string> { }, obj = new Dictionary<string, object>() });
            Assert.AreEqual("list and obj are both empty", evaled);

            evaled = lgFile.EvaluateTemplate("template", new { list = new List<string> { "hi" }, obj = new { } });
            Assert.AreEqual("obj is empty", evaled);

            evaled = lgFile.EvaluateTemplate("template", new { list = new List<string> { }, obj = new { a = "a" } });
            Assert.AreEqual("list is empty", evaled);

            evaled = lgFile.EvaluateTemplate("template", new { list = new List<string> { }, obj = new Dictionary<string, object> { { "a", "b" } } });
            Assert.AreEqual("list is empty", evaled);

            evaled = lgFile.EvaluateTemplate("template", new { list = new JArray() { new JObject() }, obj = new JObject { ["a"] = "b" } });
            Assert.AreEqual("list and obj are both not empty.", evaled);
        }

        [TestMethod]
        public void TestNullTolerant()
        {
            var lgFile = LGParser.ParseFile(GetExampleFilePath("NullTolerant.lg"));

            var evaled = lgFile.EvaluateTemplate("template1");

            Assert.AreEqual("null", evaled);

            evaled = lgFile.EvaluateTemplate("template2");

            Assert.AreEqual("result is 'null'", evaled);

            var jObjEvaled = lgFile.EvaluateTemplate("template3") as JObject;

            Assert.AreEqual("null", jObjEvaled["key1"]);
        }

        public class LoopClass
        {
            public string Name { get; set; }

            public object LoopObj { get; set; }
        }
    }
}
