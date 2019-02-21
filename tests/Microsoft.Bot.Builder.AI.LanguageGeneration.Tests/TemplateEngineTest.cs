using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Builder.AI.LanguageGeneration;
using System.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Tests
{
    [TestClass]
    public class TemplateEngineTest
    {
        private string GetExampleFilePath(string fileName)
        {
            return AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")) + "Examples\\" + fileName;
        }
        

        [TestMethod]
        public void TestBasic()
        {   
            var engine = TemplateEngine.FromFile(GetExampleFilePath("2.lg"));

            var evaled = engine.EvaluateTemplate("wPhrase", null);
            var options = new List<string> { "Hi", "Hello", "Hiya " };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("3.lg"));

            var evaled = engine.EvaluateTemplate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)"};

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = engine.EvaluateTemplate("welcome-user", new { userName = userName});
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName),  $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestBasicConditionalTemplate()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("5.lg"));

            string evaled = engine.EvaluateTemplate("time-of-day-readout", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicTemplateRefWithParameters()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("6.lg"));

            string evaled = engine.EvaluateTemplate("welcome", null);
            Assert.IsTrue("Hi DongLei :)" == evaled || 
                "Hey DongLei :)" == evaled ||
                "Hello DongLei :)" == evaled );

            evaled = engine.EvaluateTemplate("welcome", new { userName = "DL" });
            Assert.IsTrue("Hi DL :)" == evaled ||
                "Hey DL :)" == evaled ||
                "Hello DL :)" == evaled);
        }

        [TestMethod]
        public void TestBasicListSupport()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("BasicList.lg"));
            Assert.AreEqual(engine.EvaluateTemplate("BasicJoin", new { items = new[] { "1", "2" } }), "1, 2");
        }

        [TestMethod]
        public void TestBasicExtendedFunctions()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("6.lg"));
            var alarms = new []
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


            //var evaled = engine.EvaluateTemplate("ShowAlarmsWithForeach", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.EvaluateTemplate("ShowAlarmsWithMemberForeach", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.EvaluateTemplate("ShowAlarmsWithHumanize", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            var evaled = engine.EvaluateTemplate("ShowAlarmsWithMemberHumanize", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

        }

        [TestMethod]
        public void TestBasicLoopRef()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("7.lg"));
            var evaled = engine.EvaluateTemplate("wPhrase", "");
            Assert.AreEqual(evaled, "你好");
        }

        [TestMethod]
        public void TestListWithOnlyOneElement()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("8.lg"));
            var evaled = engine.EvaluateTemplate("RecentTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual(evaled, "你好");
        }

        [TestMethod]
        public void TestTemplateNameWithDotIn()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("TemplateNameWithDot.lg"));
            Assert.AreEqual(engine.EvaluateTemplate("Hello.World", null), "Hello World");
            Assert.AreEqual(engine.EvaluateTemplate("Hello", null), "Hello World");
        }

        [TestMethod]
        public void TestBasicInlineTemplate()
        {
            var emptyEngine = TemplateEngine.FromText("");
            Assert.AreEqual(emptyEngine.Evaluate("Hi", null), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name}", new { name = "DL" } ), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName}", new { name = new { FirstName = "D", LastName = "L" }} ), "Hi DL");
            Assert.AreEqual(TemplateEngine.EmptyEngine().Evaluate("Hi", null), "Hi");
        }

        [TestMethod]
        public void TestInlineTemplateWithTemplateFile()
        {
            var emptyEngine = TemplateEngine.FromFile(GetExampleFilePath("8.lg"));
            Assert.AreEqual(emptyEngine.Evaluate("Hi", null), "Hi");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name}", new { name = "DL" }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName}", new { name = new { FirstName = "D", LastName = "L" } }), "Hi DL");
            Assert.AreEqual(emptyEngine.Evaluate("Hi {name.FirstName}{name.LastName} [RecentTasks]", 
                                                  new {
                                                       name = new {
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
                                                      recentTasks = new [] {"task1"}
                                                      

                                                  }), "Hi DL Your most recent task is task1. You can let me know if you want to add or complete a task.");

        }

        [TestMethod]
        public void TestMultiLine()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("MultilineTextForAdaptiveCard.lg"));
            var evaled1 = engine.EvaluateTemplate("wPhrase", "");
            var options1 = new List<string> { "\r\ncardContent\r\n", "hello" };
            Assert.IsTrue(options1.Contains(evaled1), $"Evaled is {evaled1}");

            var evaled2 = engine.EvaluateTemplate("nameTemplate", new { name = "N" });
            var options2 = new List<string> { "\r\nN\r\n", "N" };
            Assert.IsTrue(options2.Contains(evaled2), $"Evaled is {evaled2}");

            var evaled3 = engine.EvaluateTemplate("adaptivecardsTemplate", "");

            var evaled4 = engine.EvaluateTemplate("refTemplate", "");
            var options4 = new List<string> { "\r\nhi\r\n" };
            Assert.IsTrue(options4.Contains(evaled4), $"Evaled is {evaled4}");
        }

       
    }
}
