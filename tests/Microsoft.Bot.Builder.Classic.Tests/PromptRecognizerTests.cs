using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.Tests.Resource;
using Microsoft.Bot.Builder.Classic.Resource;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    [TestClass]
    public class PromptRecognizerTests
    {
        private void PromptRecognizersChoice(string choicesKey, string text, string expected, string locale = null)
        {
            var activity = new Activity { Text = text, Locale = locale };
            var results = new PromptRecognizer().RecognizeLocalizedChoices(activity, choicesKey, Resources.ResourceManager, null);
            var top = results.MaxBy(x => x.Score);
            Assert.AreEqual(expected, top.Entity);
        }

        private void PromptRecognizersOrdinals(string text, long expected, string locale = null)
        {
            var activity = new Activity { Text = text, Locale = locale };
            var results = new PromptRecognizer().RecognizeOrdinals(activity);
            var top = results.MaxBy(x => x.Score);
            Assert.AreEqual(expected, top.Entity);
        }

        [TestMethod]
        public void RecognizeChoices()
        {
            PromptRecognizersChoice("NumberReverseOrdinals", "the last one", "-1");
            PromptRecognizersChoice("NumberReverseOrdinals", "the second to last", "-2");
            PromptRecognizersChoice("NumberOrdinals", "just the first option", "1");
            PromptRecognizersChoice("NumberOrdinals", "I pick the 3rd", "3");
            PromptRecognizersChoice("NumberReverseOrdinals", "maybe the last one", "-1");
        }

        [TestMethod]
        public void RecognizeOrdinals()
        {
            PromptRecognizersOrdinals("the last one", -1);
            PromptRecognizersOrdinals("the second to last", -2);
            PromptRecognizersOrdinals("just the first option", 1);
            PromptRecognizersOrdinals("I pick the 3rd", 3);
            PromptRecognizersOrdinals("maybe the last one", -1);
        }

        [TestMethod]
        public void RecognizeOrdinalsSpanish()
        {
            PromptRecognizersOrdinals("el último", -1, "es-ES");
            PromptRecognizersOrdinals("la penúltima", -2, "es-ES");
            PromptRecognizersOrdinals("elijo la primera opción", 1, "es-ES");
            PromptRecognizersOrdinals("quiero la 3ra", 3, "es-ES");
            PromptRecognizersOrdinals("quizás el último", -1, "es-ES");
        }

        [TestMethod]
        public void RecognizeNumbers()
        {
            var activity = new Activity()
            {
                Text = "the value is -12"
            };
            var result = new PromptRecognizer().RecognizeNumbers(activity, null);
            Assert.AreEqual(-12, result.FirstOrDefault().Entity);
        }

        [TestMethod]
        public void RecognizeNumbersWithLimits()
        {
            var activity = new Activity()
            {
                Text = "the value is 12"
            };
            var result = new PromptRecognizer().RecognizeNumbers(activity, new PromptRecognizeNumbersOptions { IntegerOnly = true, MinValue = 50, MaxValue = 100 });
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void RecognizeTerm()
        {
            var activity = new Activity()
            {
                Text = "the value is twelve."
            };
            var result = new PromptRecognizer().RecognizeNumbers(activity, null);
            Assert.AreEqual(12, result.FirstOrDefault().Entity);
        }

        [TestMethod]
        public void RecognizeTerm_Spanish()
        {
            var activity = new Activity()
            {
                Text = "El valor es doce.",
                Locale = "es"
            };
            var result = new PromptRecognizer().RecognizeNumbers(activity, null);
            Assert.AreEqual(12, result.FirstOrDefault().Entity);
        }

        [TestMethod]
        public void RecognizeBoolean_TrueValue()
        {
            TestRecognizeBooleanValue("The value is y", true);
            TestRecognizeBooleanValue("The value is yes", true);
            TestRecognizeBooleanValue("The value is yep", true);
            TestRecognizeBooleanValue("The value is sure", true);
            TestRecognizeBooleanValue("The value is ok", true);
            TestRecognizeBooleanValue("The value is true", true);
            TestRecognizeBooleanValue("The value is \\u1F44d", true);
            TestRecognizeBooleanValue("The value is \\u1F44C", true);
        }

        [TestMethod]
        public void RecognizeBoolean_FalseValue()
        {
            TestRecognizeBooleanValue("The value is n", false);
            TestRecognizeBooleanValue("The value is no", false);
            TestRecognizeBooleanValue("The value is nope", false);
            TestRecognizeBooleanValue("The value is false", false);
            TestRecognizeBooleanValue("The value is \\u1f44e", false);
            TestRecognizeBooleanValue("The value is \\u270b", false);
            TestRecognizeBooleanValue("The value is \\u1f590", false);
        }

        private void TestRecognizeBooleanValue(string text, bool expectedResult)
        {
            var activity = new Activity()
            {
                Text = text
            };
            var result = new PromptRecognizer().RecognizeBooleans(activity);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.AreEqual(expectedResult, result.FirstOrDefault().Entity);
        }

        [TestMethod]
        public void RecognizeTime()
        {
            TestRecognizeTimeValue("The value is 1 pm.", 13);
            TestRecognizeTimeValue("The value is 17:17.", 17, 17);
            TestRecognizeTimeValue("The value is 5:17 pm.", 17, 17);
            TestRecognizeTimeValue("The value is noon.", 12, 00);
            TestRecognizeTimeValue("The value is midnight.", 0, 00);
        }

        private void TestRecognizeTimeValue(string text, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
        {
            var activity = new Activity()
            {
                Text = text
            };

            var result = new PromptRecognizer().RecognizeTimes(activity);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());

            DateTime now = DateTime.Now;
            DateTime expectedDate = new DateTime(now.Year, now.Month, now.Day, hour, minute, second, millisecond);

            string expectedResult = expectedDate.TimeOfDay.ToString();
            Assert.AreEqual(expectedResult, result.FirstOrDefault().Entity);
        }

        [TestMethod]
        public void TestRecognizeRegex()
        {
            var expectedValue = "help";
            var activity = new Activity { Text = expectedValue };
            var result = new PromptRecognizer().RecognizeLocalizedRegExp(activity, "Exp1", TestResources.ResourceManager);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.FirstOrDefault().Entity, expectedValue);
        }

        [TestMethod]
        public void TestRecognizeRegexEs()
        {
            var expectedValue = "ayuda";
            var activity = new Activity { Text = expectedValue, Locale = "es-AR" };
            var result = new PromptRecognizer().RecognizeLocalizedRegExp(activity, "Exp1", TestResources.ResourceManager);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.FirstOrDefault().Entity, expectedValue);
        }

        [TestMethod]
        public void TestRecognizeUsingDefaultCulture()
        {
            var expectedValue = "help";
            var activity = new Activity { Text = expectedValue, Locale = "fr-FR" };
            var result = new PromptRecognizer().RecognizeLocalizedRegExp(activity, "Exp1", TestResources.ResourceManager);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.FirstOrDefault().Entity, expectedValue);
        }
        
        [TestMethod]
        public void TestRecognizeUsingDefaultCultureWhenLocaleNotFound()
        {
            var activity = new Activity { Text = "aider", Locale = "fr-FR" };
            var result = new PromptRecognizer().RecognizeLocalizedRegExp(activity, "Exp1", TestResources.ResourceManager);
            Assert.AreEqual(result.Count(), 0);
        }

        [TestMethod]
        public void TestNotRecognizeRegex()
        {
            var activity = new Activity { Text = "foo" };
            var result = new PromptRecognizer().RecognizeLocalizedRegExp(activity, "Exp1", TestResources.ResourceManager);
            Assert.AreEqual(result.Count(), 0);
        }
        
        [TestMethod]
        public void TestRecognizeChoice()
        {
            var expectedValue = "a";
            var activity = new Activity { Text = "a" };
            var result = new PromptRecognizer().RecognizeLocalizedChoices(activity, "Choices1", TestResources.ResourceManager, null);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.FirstOrDefault().Entity, expectedValue);
        }

        [TestMethod]
        public void TestRecognizeChoiceSynonym()
        {
            var expectedValue = "b";
            var activity = new Activity { Text = "b1" };
            var result = new PromptRecognizer().RecognizeLocalizedChoices(activity, "Choices1", TestResources.ResourceManager, null);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.FirstOrDefault().Entity, expectedValue);
        }

        [TestMethod]
        public void TestRecognizeChoiceWithoutSynonym()
        {
            var expectedValue = "c";
            var activity = new Activity { Text = "c" };
            var result = new PromptRecognizer().RecognizeLocalizedChoices(activity, "Choices1", TestResources.ResourceManager, null);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.FirstOrDefault().Entity, expectedValue);
        }

        [TestMethod]
        public void TestNotRecognizeChoiceValueIgnored()
        {
            var activity = new Activity { Text = "a" };
            var options = new PromptRecognizeChoicesOptions { ExcludeValue = true };
            var result = new PromptRecognizer().RecognizeLocalizedChoices(activity, "Choices1", TestResources.ResourceManager, options);
            Assert.AreEqual(result.Count(), 0);
        }

        [TestMethod]
        public void TestRecognizeBooleanTrue()
        {
            var activity = new Activity { Text = "yes" };
            var result = new PromptRecognizer().RecognizeBooleans(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizeBooleanFalse()
        {
            var activity = new Activity { Text = "no" };
            var result = new PromptRecognizer().RecognizeBooleans(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(false, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizeMultipleBoolean()
        {
            var activity = new Activity { Text = "yes and no" };
            var result = new PromptRecognizer().RecognizeBooleans(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(true, result.First().Entity);
            Assert.AreEqual(false, result.Last().Entity);
        }

        [TestMethod]
        public void TestRecognizeCardinal()
        {
            var activity = new Activity { Text = "1.23" };
            var result = new PromptRecognizer().RecognizeNumbers(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(1.23, result.First().Entity); // Can change the decimal point?
        }

        [TestMethod]
        public void TestRecognizeCardinalWords()
        {
            var activity = new Activity { Text = "seven" };
            var result = new PromptRecognizer().RecognizeNumbers(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(7, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizeNegativeNumber()
        {
            var activity = new Activity { Text = "-13" };
            var result = new PromptRecognizer().RecognizeNumbers(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(-13, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizePositiveNumber()
        {
            var activity = new Activity { Text = "I will take +12" };
            var result = new PromptRecognizer().RecognizeNumbers(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(12, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizeMultipleNumber()
        {
            var activity = new Activity { Text = "1.7 and seven" };
            var result = new PromptRecognizer().RecognizeNumbers(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1.7, result.First().Entity);
            Assert.AreEqual(7, result.Last().Entity);
        }

        [TestMethod]
        public void TestRecognizeMultipleIntegerOnly()
        {
            var activity = new Activity { Text = "1, 2.3, and seven" };
            var result = new PromptRecognizer().RecognizeNumbers(activity, new PromptRecognizeNumbersOptions { IntegerOnly = true });

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1, result.First().Entity);
            Assert.AreEqual(7, result.Last().Entity);
        }

        [TestMethod]
        public void TestRecognizeRangeNumbers()
        {
            var activity = new Activity { Text = "1, 2.3, and seven" };
            var result = new PromptRecognizer().RecognizeNumbers(activity, new PromptRecognizeNumbersOptions { MinValue = 2, MaxValue = 5 });
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2.3, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizeOrdinal()
        {
            var activity = new Activity { Text = "i'd like the second one" };
            var result = new PromptRecognizer().RecognizeOrdinals(activity);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.First().Entity);
        }

        [TestMethod]
        public void TestRecognizeReverseOrdinal()
        {
            var activity = new Activity { Text = "i'd like the second to last one" };
            var results = new PromptRecognizer().RecognizeOrdinals(activity);

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count() >= 2);
            var top = results.MaxBy(x => x.Score);
            Assert.AreEqual(-2, top.Entity);
        }
    }
}