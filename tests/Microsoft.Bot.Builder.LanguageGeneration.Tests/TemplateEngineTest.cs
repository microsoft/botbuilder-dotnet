using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplateEngineTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestBasic()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("2.lg"));

            var evaled = engine.EvaluateTemplate("wPhrase");
            var options = new List<string> { "Hi", "Hello", "Hiya" };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("3.lg"));

            var evaled = engine.EvaluateTemplate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)" };

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = engine.EvaluateTemplate("welcome-user", new { userName = userName });
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName), $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestIfElseTemplate()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("5.lg"));

            string evaled = engine.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = engine.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "evening" });
            Assert.IsTrue(evaled == "Good evening" || evaled == "Evening! ", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicConditionalTemplateWithoutDefault()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("5.lg"));

            string evaled = engine.EvaluateTemplate("time-of-day-readout-without-default", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = engine.EvaluateTemplate("time-of-day-readout-without-default2", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = engine.EvaluateTemplate("time-of-day-readout-without-default2", new { timeOfDay = "evening" });
            Assert.IsNull(evaled, "Evaled is not null");
        }

        [TestMethod]
        public void TestBasicSwitchCaseTemplate()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("switchcase.lg"));

            string evaled = engine.EvaluateTemplate("greetInAWeek", new { day = "Saturday" });
            Assert.IsTrue(evaled == "Happy Saturday!");

            evaled = engine.EvaluateTemplate("greetInAWeek", new { day = "Monday" });
            Assert.IsTrue(evaled == "Work Hard!");
        }

        [TestMethod]
        public void TestBasicTemplateRefWithParameters()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("6.lg"));

            string evaled = engine.EvaluateTemplate("welcome", null);
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = engine.EvaluateTemplate("welcome", new { userName = "DL" });
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [TestMethod]
        public void TestBasicListSupport()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("BasicList.lg"));
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1" } }), "1");
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2" } }), "1, 2");
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2", "3" } }), "1, 2 and 3");
        }

        [TestMethod]
        public void TestBasicExtendedFunctions()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("6.lg"));
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

            var evaled = engine.EvaluateTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);
        }

        [TestMethod]
        public void TestCaseInsensitive()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("CaseInsensitive.lg"));
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

            var evaled = engine.EvaluateTemplate("ShowAlarms", new { alarms = alarms });
            Assert.AreEqual("You have two alarms", evaled);

            evaled = engine.EvaluateTemplate("greetInAWeek", new { day = "Saturday" });
            Assert.AreEqual("Happy Saturday!", evaled);
        }

        [TestMethod]
        public void TestListWithOnlyOneElement()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("8.lg"));
            var evaled = engine.EvaluateTemplate("ShowTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual("Your most recent task is Task1. You can let me know if you want to add or complete a task.", evaled);
        }

        [TestMethod]
        public void TestTemplateNameWithDotIn()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("TemplateNameWithDot.lg"));
            Assert.AreEqual(engine.EvaluateTemplate("Hello.World", null), "Hello World");
            Assert.AreEqual(engine.EvaluateTemplate("Hello", null), "Hello World");
        }

        [TestMethod]
        public void TestBasicInlineTemplate()
        {
            var emptyEngine = new TemplateEngine();
            Assert.AreEqual(emptyEngine.Evaluate("Hi"), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi", null), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name}", new { name = "DL" }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName}", new { name = new { FirstName = "D", LastName = "L" } }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi \n Hello", null), "Hi \n Hello");
            Assert.AreEqual(emptyEngine.Evaluate("Hi \r\n Hello", null), "Hi \r\n Hello");
            Assert.AreEqual(emptyEngine.Evaluate("Hi \r\n @{name}", new { name = "DL" }), "Hi \r\n DL");
        }

        [TestMethod]
        public void TestInlineTemplateWithTemplateFile()
        {
            var emptyEngine = new TemplateEngine().AddFile(GetExampleFilePath("8.lg"));
            Assert.AreEqual(emptyEngine.Evaluate("Hi"), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi", null), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name}", new { name = "DL" }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName}", new { name = new { FirstName = "D", LastName = "L" } }), "Hi DL");
            Assert.AreEqual(
                emptyEngine.Evaluate(
                "Hi {name.FirstName}{name.LastName} [RecentTasks]",
                new
                {
                    name = new
                    {
                        FirstName = "D",
                        LastName = "L"
                    }
                }), "Hi DL You don't have any tasks.");
            Assert.AreEqual(
                emptyEngine.Evaluate(
                "Hi {name.FirstName}{name.LastName} [RecentTasks]",
                new
                {
                    name = new
                    {
                        FirstName = "D",
                        LastName = "L"
                    },
                    recentTasks = new[] { "task1" }
                }), "Hi DL Your most recent task is task1. You can let me know if you want to add or complete a task.");
        }

        [TestMethod]
        public void TestMultiLine()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("MultilineTextForAdaptiveCard.lg"));
            var evaled1 = engine.EvaluateTemplate("wPhrase", string.Empty);
            var options1 = new List<string> { "\r\ncardContent\r\n", "hello", "\ncardContent\n" };
            Assert.IsTrue(options1.Contains(evaled1), $"Evaled is {evaled1}");

            var evaled2 = engine.EvaluateTemplate("nameTemplate", new { name = "N" });
            var options2 = new List<string> { "\r\nN\r\n", "N", "\nN\n" };
            Assert.IsTrue(options2.Contains(evaled2), $"Evaled is {evaled2}");

            var evaled3 = engine.EvaluateTemplate("adaptivecardsTemplate", string.Empty);

            var evaled4 = engine.EvaluateTemplate("refTemplate", string.Empty);
            var options4 = new List<string> { "\r\nhi\r\n", "\nhi\n" };
            Assert.IsTrue(options4.Contains(evaled4), $"Evaled is {evaled4}");
        }

        [TestMethod]
        public void TestTemplateRef()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("TemplateRef.lg"));

            var scope = new
            {
                time = "morning",
                name = "Dong Lei"
            };
            Assert.AreEqual(engine.EvaluateTemplate("Hello", scope), "Good morning Dong Lei");
            Assert.AreEqual(engine.EvaluateTemplate("Hello2", scope), "Good morning Dong Lei");
            Assert.AreEqual(engine.EvaluateTemplate("Hello3", scope), "Good morning Dong Lei");
        }

        [TestMethod]
        public void TestEscapeCharacter()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("EscapeCharacter.lg"));
            var evaled = engine.EvaluateTemplate("wPhrase", null);
            Assert.AreEqual(evaled, "Hi \r\n\t[]{}\\");

            evaled = engine.EvaluateTemplate("otherEscape", null);
            Assert.AreEqual(evaled, @"Hi \y \");

            evaled = engine.EvaluateTemplate("escapeInExpression", null);
            Assert.AreEqual(evaled, "Hi hello\\\\");

            evaled = engine.EvaluateTemplate("escapeInExpression2", null);
            Assert.AreEqual(evaled, "Hi hello'");

            evaled = engine.EvaluateTemplate("escapeInExpression3", null);
            Assert.AreEqual(evaled, "Hi hello\"");

            evaled = engine.EvaluateTemplate("escapeInExpression4", null);
            Assert.AreEqual(evaled, "Hi hello\"");

            evaled = engine.EvaluateTemplate("escapeInExpression5", null);
            Assert.AreEqual(evaled, "Hi hello\n");

            evaled = engine.EvaluateTemplate("escapeInExpression6", null);
            Assert.AreEqual(evaled, "Hi hello\n");

            var todos = new[] { "A", "B", "C" };
            evaled = engine.EvaluateTemplate("showTodo", new { todos });
            Assert.AreEqual(evaled.Replace("\r\n", "\n"), "\n    Your most recent 3 tasks are\n    * A\n* B\n* C\n    ");
            
            evaled = engine.EvaluateTemplate("showTodo", null);
            Assert.AreEqual(evaled.Replace("\r\n", "\n"), "\n    You don't have any \"t\\\\odo'\".\n    ");
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
                }
            };

            foreach (var testItem in testData)
            {
                var engine = new TemplateEngine().AddFile(GetExampleFilePath("analyzer.lg"));
                var evaled1 = engine.AnalyzeTemplate(testItem.GetType().GetProperty("name").GetValue(testItem).ToString());
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
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = engine.EvaluateTemplate("TemplateC", string.Empty);
            var options = new List<string> { "Hi", "Hello" };
            Assert.AreEqual(options.Contains(evaled), true);

            evaled = engine.EvaluateTemplate("TemplateD", new { b = "morning" });
            options = new List<string> { "Hi morning", "Hello morning" };
            Assert.AreEqual(options.Contains(evaled), true);
        }

        [TestMethod]
        public void TestTemplateAsFunction()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("TemplateAsFunction.lg"));
            string evaled = engine.EvaluateTemplate("Test2", string.Empty);

            Assert.AreEqual(evaled, "hello world");

            evaled = engine.EvaluateTemplate("Test3", string.Empty);
            Assert.AreEqual(evaled, "hello world");

            evaled = engine.EvaluateTemplate("Test4", string.Empty);
            Assert.AreEqual(evaled.Trim(), "hello world");

            evaled = engine.EvaluateTemplate("dupNameWithTemplate");
            Assert.AreEqual(evaled, "calculate length of ms by user's template");

            evaled = engine.EvaluateTemplate("dupNameWithBuiltinFunc");
            Assert.AreEqual(evaled, "2");
        }

        [TestMethod]
        public void TestAnalyzelgTemplateFunction()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("lgTemplate.lg"));
            var evaled = engine.AnalyzeTemplate("TemplateD");
            var variableEvaled = evaled.Variables;
            var options = new List<string>() { "b" };
            Assert.AreEqual(variableEvaled.Count, options.Count);
            options.ForEach(e => Assert.AreEqual(variableEvaled.Contains(e), true));
        }

        [TestMethod]
        public void TestExceptionCatch()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("ExceptionCatch.lg"));
            try
            {
                engine.EvaluateTemplate("NoVariableMatch", null);
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.Message);
            }
        }

        [TestMethod]
        public void TestMultipleLgFiles()
        {
            var file123 = new string[]
            {
                GetExampleFilePath("MultiFile-Part1.lg"),
                GetExampleFilePath("MultiFile-Part2.lg"),
                GetExampleFilePath("MultiFile-Part3.lg"),
            };

            var ex = Assert.ThrowsException<Exception>(() => new TemplateEngine().AddFile(file123[0]));
            TestContext.WriteLine(ex.Message);

            ex = Assert.ThrowsException<Exception>(() => new TemplateEngine().AddFiles(new List<string> { file123[0], file123[1] }));
            TestContext.WriteLine(ex.Message);

            ex = Assert.ThrowsException<Exception>(() => new TemplateEngine().AddFiles(new List<string> { file123[0], file123[2] }));
            TestContext.WriteLine(ex.Message);

            var msg = "hello from t1, ref template2: 'hello from t2, ref template3: hello from t3' and ref template3: 'hello from t3'";

            var engine = new TemplateEngine().AddFiles(new List<string> { file123[0], file123[1], file123[2] });
            Assert.AreEqual(msg, engine.EvaluateTemplate("template1", null));

            engine = new TemplateEngine().AddFiles(new List<string> { file123[1], file123[0], file123[2] });
            Assert.AreEqual(msg, engine.EvaluateTemplate("template1", null));

            engine = new TemplateEngine().AddFiles(new List<string> { file123[2], file123[1], file123[0] });
            Assert.AreEqual(msg, engine.EvaluateTemplate("template1", null));
        }

        [TestMethod]
        public void TestImportLgFiles()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("import.lg"));

            // Assert 6.lg is imported only once when there are several relative paths which point to the same file.
            // Assert import cycle loop is handled well as expected when a file imports itself.
            Assert.AreEqual(14, engine.Templates.Count());

            string evaled = engine.EvaluateTemplate("basicTemplate", null);
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = engine.EvaluateTemplate("welcome", null);
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = engine.EvaluateTemplate("template3", null);
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            evaled = engine.EvaluateTemplate("welcome", new { userName = "DL" });
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");

            evaled = engine.EvaluateTemplate("basicTemplate2", null);
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            // Assert 6.lg of relative path is imported from text.
            engine = new TemplateEngine().AddText(content: "# basicTemplate\r\n- Hi\r\n- Hello\r\n[import](./6.lg)", id: GetExampleFilePath("xx.lg"));

            Assert.AreEqual(8, engine.Templates.Count());

            evaled = engine.EvaluateTemplate("basicTemplate", null);
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = engine.EvaluateTemplate("welcome", null);
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = engine.EvaluateTemplate("welcome", new { userName = "DL" });
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");
        }

        [TestMethod]
        public void TestRegex()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("Regex.lg"));
            var evaled = engine.EvaluateTemplate("wPhrase", string.Empty);
            Assert.AreEqual(evaled, "Hi");

            evaled = engine.EvaluateTemplate("wPhrase", new { name = "jack" });
            Assert.AreEqual(evaled, "Hi jack");

            evaled = engine.EvaluateTemplate("wPhrase", new { name = "morethanfive" });
            Assert.AreEqual(evaled, "Hi");
        }

        [TestMethod]
        public void TestLgFileImportMultipleTimes()
        {
            var engine = new TemplateEngine().AddFiles(new List<string>() { GetExampleFilePath("import.lg"), GetExampleFilePath("import2.lg") });

            // Assert 6.lg is imported only once and no exceptions are thrown when it is imported from multiple files.
            Assert.AreEqual(14, engine.Templates.Count());

            string evaled = engine.EvaluateTemplate("basicTemplate", null);
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = engine.EvaluateTemplate("welcome", null);
            Assert.IsTrue(evaled == "Hi DongLei :)" ||
                evaled == "Hey DongLei :)" ||
                evaled == "Hello DongLei :)");

            evaled = engine.EvaluateTemplate("welcome", new { userName = "DL" });
            Assert.IsTrue(evaled == "Hi DL :)" ||
                evaled == "Hey DL :)" ||
                evaled == "Hello DL :)");

            evaled = engine.EvaluateTemplate("basicTemplate2", null);
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            evaled = engine.EvaluateTemplate("basicTemplate3", null);
            Assert.IsTrue(evaled == "Hi" || evaled == "Hello");

            evaled = engine.EvaluateTemplate("basicTemplate4", null);
            Assert.IsTrue(evaled == "Hi 2" || evaled == "Hello 2");

            engine = new TemplateEngine().AddFile(GetExampleFilePath("import.lg"));
            var ex = Assert.ThrowsException<Exception>(() => engine.AddFile(GetExampleFilePath("import2.lg")));
            Assert.IsTrue(ex.Message.Contains("Duplicated definitions found for template: wPhrase"));

            engine = new TemplateEngine().AddFiles(new List<string>() { GetExampleFilePath("import.lg") });
            ex = Assert.ThrowsException<Exception>(() => engine.AddFiles(new List<string>() { GetExampleFilePath("import2.lg") }));
            Assert.IsTrue(ex.Message.Contains("Duplicated definitions found for template: wPhrase"));
        }

        [TestMethod]
        public void TestExpandTemplate()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("Expand.lg"));

            // without scope
            var evaled = engine.ExpandTemplate("FinalGreeting");
            Assert.AreEqual(4, evaled.Count);
            var expectedResults = new List<string>() { "Hi Morning", "Hi Evening", "Hello Morning", "Hello Evening" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            // with scope
            evaled = engine.ExpandTemplate("TimeOfDayWithCondition", new { time = "evening" });
            Assert.AreEqual(2, evaled.Count);
            expectedResults = new List<string>() { "Hi Evening", "Hello Evening" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));

            // with scope
            evaled = engine.ExpandTemplate("greetInAWeek", new { day = "Sunday" });
            Assert.AreEqual(2, evaled.Count);
            expectedResults = new List<string>() { "Nice Sunday!", "Happy Sunday!" };
            expectedResults.ForEach(x => Assert.AreEqual(true, evaled.Contains(x)));
        }

        [TestMethod]
        public void TestExpandTemplateWithRef()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("Expand.lg"));

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

            var evaled = engine.ExpandTemplate("ShowAlarmsWithLgTemplate", new { alarms = alarms });
            Assert.AreEqual(2, evaled.Count);
            Assert.AreEqual("You have 2 alarms, they are 8 pm at tomorrow", evaled[0]);
            Assert.AreEqual("You have 2 alarms, they are 8 pm of tomorrow", evaled[1]);
        }

        [TestMethod]
        public void TestExpandTemplateWithRefInMultiLine()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("Expand.lg"));

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

            var evaled = engine.ExpandTemplate("ShowAlarmsWithMultiLine", new { alarms = alarms });
            Assert.AreEqual(2, evaled.Count);
            var eval1Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm at tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm at tomorrow\n" };
            var eval2Options = new List<string>() { "\r\nYou have 2 alarms.\r\nThey are 8 pm of tomorrow\r\n", "\nYou have 2 alarms.\nThey are 8 pm of tomorrow\n" };
            Assert.AreEqual(true, eval1Options.Contains(evaled[0]));
            Assert.AreEqual(true, eval2Options.Contains(evaled[1]));
        }

        [TestMethod]
        public void TestExpandTemplateWithFunction()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("Expand.lg"));

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

            var evaled = engine.ExpandTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            var evalOptions = new List<string>()
            {
                "You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow",
                "You have 2 alarms, 7 am at tomorrow and 8 pm of tomorrow",
                "You have 2 alarms, 7 am of tomorrow and 8 pm at tomorrow",
                "You have 2 alarms, 7 am of tomorrow and 8 pm of tomorrow"
            };

            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evalOptions.Contains(evaled[0]));

            evaled = engine.ExpandTemplate("T2");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "3" || evaled[0] == "5");

            evaled = engine.ExpandTemplate("T3");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "3" || evaled[0] == "5");

            evaled = engine.ExpandTemplate("T4");
            Assert.AreEqual(1, evaled.Count);
            Assert.AreEqual(true, evaled[0] == "ey" || evaled[0] == "el");
        }

        [TestMethod]
        public void TestEvalExpression()
        {
            var engine = new TemplateEngine().AddFile(GetExampleFilePath("EvalExpression.lg"));

            var userName = "MS";
            var evaled = engine.EvaluateTemplate("template1", new { userName });
            Assert.AreEqual(evaled, "Hi MS");

            evaled = engine.EvaluateTemplate("template2", new { userName });
            Assert.AreEqual(evaled, "Hi MS");

            evaled = engine.EvaluateTemplate("template3", new { userName });
            Assert.AreEqual(evaled, "HiMS");

            evaled = engine.EvaluateTemplate("template4", new { userName });
            var eval1Options = new List<string>() { "\r\nHi MS\r\n", "\nHi MS\n" };
            Assert.IsTrue(eval1Options.Contains(evaled));

            evaled = engine.EvaluateTemplate("template5", new { userName });
            var eval2Options = new List<string>() { "\r\nHiMS\r\n", "\nHiMS\n" };
            Assert.IsTrue(eval2Options.Contains(evaled));

            evaled = engine.EvaluateTemplate("template6", new { userName });
            Assert.AreEqual(evaled, "goodmorning");
        }

        [TestMethod]
        public void TestLGResource()
        {
            var lgResource = LGParser.Parse(File.ReadAllText(GetExampleFilePath("2.lg")));

            Assert.AreEqual(lgResource.Templates.Count, 1);
            Assert.AreEqual(lgResource.Imports.Count, 0);
            Assert.AreEqual(lgResource.Templates[0].Name, "wPhrase");
            Assert.AreEqual(lgResource.Templates[0].Body.Replace("\r\n", "\n"), "- Hi\n- Hello\n- Hiya\n- Hi");

            lgResource = lgResource.AddTemplate("newtemplate", new List<string> { "age", "name" }, "- hi ");
            Assert.AreEqual(lgResource.Templates.Count, 2);
            Assert.AreEqual(lgResource.Imports.Count, 0);
            Assert.AreEqual(lgResource.Templates[1].Name, "newtemplate");
            Assert.AreEqual(lgResource.Templates[1].Parameters.Count, 2);
            Assert.AreEqual(lgResource.Templates[1].Parameters[0], "age");
            Assert.AreEqual(lgResource.Templates[1].Parameters[1], "name");
            Assert.AreEqual(lgResource.Templates[1].Body, "- hi ");

            lgResource = lgResource.UpdateTemplate("newtemplate", new List<string> { "newage", "newname" }, "- new hi\r\n#hi");
            Assert.AreEqual(lgResource.Templates.Count, 2);
            Assert.AreEqual(lgResource.Imports.Count, 0);
            Assert.AreEqual(lgResource.Templates[1].Name, "newtemplate");
            Assert.AreEqual(lgResource.Templates[1].Parameters.Count, 2);
            Assert.AreEqual(lgResource.Templates[1].Parameters[0], "newage");
            Assert.AreEqual(lgResource.Templates[1].Parameters[1], "newname");
            Assert.AreEqual(lgResource.Templates[1].Body, "- new hi\r\n- #hi");

            lgResource = lgResource.DeleteTemplate("newtemplate");
            Assert.AreEqual(lgResource.Templates.Count, 1);
            Assert.AreEqual(lgResource.Imports.Count, 0);
            Assert.AreEqual(lgResource.Templates[0].Name, "wPhrase");
            Assert.AreEqual(lgResource.Templates[0].Body.Replace("\r\n", "\n"), "- Hi\n- Hello\n- Hiya\n- Hi");
        }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Examples", fileName);
        }
    }
}
