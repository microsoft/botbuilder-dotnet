using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Tests.RaceCondition;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Diag = System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public class RaceConditionTests
    {
        private const string Choice1 = "C1";
        private const string Choice2 = "C2";
        private const string Lang1 = "en";
        private const string Lang2 = "es";
        private const string Lang3 = "it";

        private void PromptRecognizersChoice(string choicesKey, string text, string expected, string locale = null)
        {
            var activity = new Activity { Text = text, Locale = locale };
            var results = new PromptRecognizer().RecognizeLocalizedChoices(activity, choicesKey, Resources.ResourceManager, null);
            var top = results.MaxBy(x => x.Score);
            Assert.AreEqual(expected, top.Entity);
        }

        [TestMethod]
        public void RaceSingleLanguageSingleChoices()
        {
            List<Task> taskPool = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a1", "a", Lang1)));
            }
            Task.WaitAll(taskPool.ToArray());
        }

        [TestMethod]
        public void RaceMultiLanguageSingleChoices()
        {
            List<Task> taskPool = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a1", "a", Lang1)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a3", "a", Lang2)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a5", "a", Lang3)));
            }
            Task.WaitAll(taskPool.ToArray());
        }

        [TestMethod]
        public void RaceSingleLanguageMultiChoices()
        {
            List<Task> taskPool = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a1", "a", Lang1)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice2, "1b", "1", Lang1)));
            }
            Task.WaitAll(taskPool.ToArray());
        }

        [TestMethod]
        public void RaceMultiLanguageMultiChoices()
        {
            List<Task> taskPool = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a1", "a", Lang1)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a3", "a", Lang2)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice1, "a5", "a", Lang3)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice2, "1b", "1", Lang1)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice2, "1d", "1", Lang2)));
                taskPool.Add(Task.Run(() => PromptRecognizersChoice(Choice2, "1f", "1", Lang3)));
            }
            Task.WaitAll(taskPool.ToArray());
        }
    }
}
