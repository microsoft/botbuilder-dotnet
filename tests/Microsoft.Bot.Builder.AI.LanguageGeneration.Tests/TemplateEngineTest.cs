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

            var evaled = engine.Evaluate("wPhrase", null);
            var options = new List<string> { "Hi", "Hello", "Hiya " };

            Assert.IsTrue(options.Contains(evaled), $"The result `{evaled}` is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateReference()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("3.lg"));

            var evaled = engine.Evaluate("welcome-user", null);
            var options = new List<string> { "Hi", "Hello", "Hiya", "Hi :)", "Hello :)", "Hiya :)"};

            Assert.IsTrue(options.Contains(evaled), $"The result {evaled} is not in those options [{string.Join(",", options)}]");
        }

        [TestMethod]
        public void TestBasicTemplateRefAndEntityRef()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("4.lg"));

            var userName = "DL";
            var evaled = engine.Evaluate("welcome-user", new { userName = userName});
            var options = new List<string> { "Hi", "Hello", "Hiya ", "Hi :)", "Hello :)", "Hiya  :)" };

            Assert.IsTrue(evaled.Contains(userName),  $"The result {evaled} does not contiain `{userName}`");
        }

        [TestMethod]
        public void TestBasicConditionalTemplate()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("5.lg"));

            string evaled = engine.Evaluate("time-of-day-readout", new { timeOfDay = "morning" });
            Assert.IsTrue(evaled == "Good morning" || evaled == "Morning! ", $"Evaled is {evaled}");
        }

        [TestMethod]
        public void TestBasicTemplateRefWithParameters()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("6.lg"));

            string evaled = engine.Evaluate("welcome", null);
            Assert.AreEqual("Hi DongLei :)", evaled);

            evaled = engine.Evaluate("welcome", new { userName = "DL" });
            Assert.AreEqual("Hi DL :)", evaled);
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


            //var alarmStrs = alarms.Select(x => engine.Evaluate("ShowAlarm", new { alarm = x })).ToList() ;
            //var evaled = engine.Evaluate("ShowAlarms", new { alarms = alarmStrs });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);


            //var evaled = engine.Evaluate("ShowAlarmsWithForeach", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.Evaluate("ShowAlarmsWithMemberForeach", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            //var evaled = engine.Evaluate("ShowAlarmsWithHumanize", new { alarms = alarms });
            //Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

            var evaled = engine.Evaluate("ShowAlarmsWithMemberHumanize", new { alarms = alarms });
            Assert.AreEqual("You have 2 alarms, 7 am at tomorrow and 8 pm at tomorrow", evaled);

        }

        [TestMethod]
        public void TestBasicLoopRef()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("7.lg"));
            var evaled = engine.Evaluate("wPhrase", "");
            Assert.AreEqual(evaled, "你好");
        }

        [TestMethod]
        public void TestListWithOnlyOneElement()
        {
            var engine = TemplateEngine.FromFile(GetExampleFilePath("8.lg"));
            var evaled = engine.Evaluate("RecentTasks", new { recentTasks = new[] { "Task1" } });
            Assert.AreEqual(evaled, "你好");
        }
    }
}
