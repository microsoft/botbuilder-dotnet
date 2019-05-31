using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.LanguageGeneration;
using System.Linq;
using System.IO;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplateEngineTest
    {
        public TestContext TestContext { get; set; }

        private string GetExampleFilePath(string fileName)
        {
            return Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), "Examples" , fileName);
        }


        [TestMethod]
        public void TestBasic()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("2.lg"));

            var evaled = engine.EvaluateTemplate("wPhrase", null);
            var options = new List<string> { "Hi", "Hello", "Hiya " };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("3.lg"));

            var evaled = engine.EvaluateTemplate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)" };

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = engine.EvaluateTemplate("welcome-user", new { userName = userName });
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName), $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestIfElseTemplate()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("5.lg"));

            string evaled = engine.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");

            evaled = engine.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "evening" });
            Assert.IsTrue(evaled == "Good evening" || evaled == "Evening! ", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicConditionalTemplateWithoutDefault()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("5.lg"));

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
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("switchcase.lg"));

            string evaled = engine.EvaluateTemplate("greetInAWeek", new { day = "Saturday" });
            Assert.IsTrue(evaled == "Happy Saturday!");

            evaled = engine.EvaluateTemplate("greetInAWeek", new { day = "Monday" });
            Assert.IsTrue(evaled == "Work Hard!");
        }

        [TestMethod]
        public void TestBasicTemplateRefWithParameters()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("6.lg"));

            string evaled = engine.EvaluateTemplate("welcome", null);
            Assert.IsTrue("Hi DongLei :)" == evaled ||
                "Hey DongLei :)" == evaled ||
                "Hello DongLei :)" == evaled);

            evaled = engine.EvaluateTemplate("welcome", new { userName = "DL" });
            Assert.IsTrue("Hi DL :)" == evaled ||
                "Hey DL :)" == evaled ||
                "Hello DL :)" == evaled);
        }

        [TestMethod]
        public void TestBasicListSupport()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("BasicList.lg"));
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1"} }), "1");
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2" } }), "1, 2");
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2", "3" } }), "1, 2 and 3");
        }

        [TestMethod]
        public void TestBasicExtendedFunctions()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("6.lg"));
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


            //var alarmStrs = alarms.Select(x => engine.EvaluateTemplate("ShowAlarm", new { alarm = x })).ToList() ;
            //var evaled = engine.EvaluateTemplate("ShowAlarms", new { alarms = alarmStrs });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);


            var evaled = engine.EvaluateTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.EvaluateTemplate("ShowAlarmsWithMemberForeach", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.EvaluateTemplate("ShowAlarmsWithHumanize", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.EvaluateTemplate("ShowAlarmsWithMemberHumanize", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

        }

        [TestMethod]
        public void TestCaseInsensitive()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("CaseInsensitive.lg"));
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
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("8.lg"));
            var evaled = engine.EvaluateTemplate("ShowTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual("Your most recent task is Task1. You can let me know if you want to add or complete a task.", evaled);
        }

        [TestMethod]
        public void TestTemplateNameWithDotIn()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("TemplateNameWithDot.lg"));
            Assert.AreEqual(engine.EvaluateTemplate("Hello.World", null), "Hello World");
            Assert.AreEqual(engine.EvaluateTemplate("Hello", null), "Hello World");
        }

        [TestMethod]
        public void TestBasicInlineTemplate()
        {
            var emptyEngine = TemplateEngine.FromText("");
            Assert.AreEqual(emptyEngine.Evaluate("Hi", null), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name}", new { name = "DL" }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName}", new { name = new { FirstName = "D", LastName = "L" } }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi \n Hello", null), "Hi \n Hello");
            Assert.AreEqual(emptyEngine.Evaluate("Hi \r\n Hello", null), "Hi \r\n Hello");
            Assert.AreEqual(emptyEngine.Evaluate("Hi \r\n @{name}", new { name = "DL" }), "Hi \r\n DL");
            Assert.AreEqual(new TemplateEngine().Evaluate("Hi", null), "Hi");
        }

        [TestMethod]
        public void TestInlineTemplateWithTemplateFile()
        {
            var emptyEngine = TemplateEngine.FromFiles(GetExampleFilePath("8.lg"));
            Assert.AreEqual(emptyEngine.Evaluate("Hi", null), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name}", new { name = "DL" }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName}", new { name = new { FirstName = "D", LastName = "L" } }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName} [RecentTasks]",
                                                  new
                                                  {
                                                      name = new
                                                      {
                                                          FirstName = "D",
                                                          LastName = "L"
                                                      }

                                                  }), "Hi DL You don't have any tasks.");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName} [RecentTasks]",
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
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("MultilineTextForAdaptiveCard.lg"));
            var evaled1 = engine.EvaluateTemplate("wPhrase", "");
            var options1 = new List<string> { "\r\ncardContent\r\n", "hello", "\ncardContent\n" };
            Assert.IsTrue(options1.Contains(evaled1), $"Evaled is {evaled1}");

            var evaled2 = engine.EvaluateTemplate("nameTemplate", new { name = "N" });
            var options2 = new List<string> { "\r\nN\r\n", "N", "\nN\n" };
            Assert.IsTrue(options2.Contains(evaled2), $"Evaled is {evaled2}");

            var evaled3 = engine.EvaluateTemplate("adaptivecardsTemplate", "");

            var evaled4 = engine.EvaluateTemplate("refTemplate", "");
            var options4 = new List<string> { "\r\nhi\r\n", "\nhi\n" };
            Assert.IsTrue(options4.Contains(evaled4), $"Evaled is {evaled4}");
        }

        [TestMethod]
        public void TestTemplateRef()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("TemplateRef.lg"));

            var scope = new
            {
                time = "morning",
                name = "Dong Lei"
            };
            Assert.AreEqual(engine.EvaluateTemplate("Hello", scope), "Good morning Dong Lei");

        }




        [TestMethod]
        public void TestEscapeCharacter()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("EscapeCharacter.lg"));
            var evaled1 = engine.EvaluateTemplate("wPhrase", null);
            Assert.AreEqual(evaled1, "Hi \r\n\t[]{}\\");
        }


        [TestMethod]
        public void TestAnalyzer()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("analyzer.lg"));
            var evaled1 = engine.AnalyzeTemplate("orderReadOut");
            var evaled1Options = new List<string> { "orderType", "userName", "base", "topping", "bread", "meat" };
            Assert.IsTrue(evaled1.All(evaled1Options.Contains) && evaled1.Count == evaled1Options.Count);

            var evaled2 = engine.AnalyzeTemplate("sandwichOrderConfirmation");
            var evaled2Options = new List<string> { "bread", "meat" };
            Assert.IsTrue(evaled2.All(evaled2Options.Contains) && evaled2.Count == evaled2Options.Count);
            
            var evaled3 = engine.AnalyzeTemplate("template1");
            var evaled3Options = new List<string> { "alarms", "customer", "tasks[0]", "age", "city" };
            Assert.IsTrue(evaled3.All(evaled3Options.Contains) && evaled3.Count == evaled3Options.Count);
        }

        [TestMethod]
        public void TestExceptionCatch()
        {
            var engine = TemplateEngine.FromFiles(GetExampleFilePath("ExceptionCatch.lg"));
            try
            {
                engine.EvaluateTemplate("NoVariableMatch", null );
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

            var ex = Assert.ThrowsException<Exception>(() => TemplateEngine.FromFiles(file123[0]));
            TestContext.WriteLine(ex.Message);

            ex = Assert.ThrowsException<Exception>(() => TemplateEngine.FromFiles(file123[0], file123[1]));
            TestContext.WriteLine(ex.Message);

            ex = Assert.ThrowsException<Exception>(() => TemplateEngine.FromFiles(file123[0], file123[2]));
            TestContext.WriteLine(ex.Message);

            var engine = TemplateEngine.FromFiles(file123);

            var msg = "hello from t1, ref template2: 'hello from t2, ref template3: hello from t3' and ref template3: 'hello from t3'";
            Assert.AreEqual(msg, engine.EvaluateTemplate("template1", null));
        }
    }
}
