// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    [TestCategory("Intent Recognizers")]
    [TestCategory("Middleware")]
    public class Intent_MiddlewareTests
    {
        [TestMethod]
        public async Task Intents_RecognizeZeroIntents()
        {
            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return new List<Intent>();
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(resultingIntents.Count == 0, "Expected zero intents");
        }

        [TestMethod]
        public async Task Intents_RecognizeZeroIntentsViaNullReturn()
        {
            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return null;
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(resultingIntents.Count == 0, "Expected zero intents");
        }

        [TestMethod]
        public async Task Intents_RecognizeSingleIntent()
        {
            string targetName = Guid.NewGuid().ToString();

            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                IList<Intent> result = new List<Intent>
                {
                    new Intent() { Name = targetName }
                };
                return result;
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(resultingIntents.Count == 1, "Expected a sigle intent");
            Assert.IsTrue(resultingIntents.First().Name == targetName, "Unexpected Intent Name");
        }

        [TestMethod]
        public async Task Intents_MergeTwoRecognizerResults()
        {
            string targetName1 = Guid.NewGuid().ToString();
            string targetName2 = Guid.NewGuid().ToString();

            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return new List<Intent>
                {
                    new Intent() { Name = targetName1 }
                };
            });

            m.OnRecognize(async (context) =>
            {
                return new List<Intent>
                {
                    new Intent() { Name = targetName2 }
                };
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(resultingIntents.Count == 2, "Expected a two intents");
            Assert.IsTrue(resultingIntents[0].Name == targetName1, $"Unexpected Intent Name. Expected {targetName1}");
            Assert.IsTrue(resultingIntents[1].Name == targetName2, $"Unexpected Intent Name. Expected {targetName2}");
        }

        [TestMethod]
        public async Task Intents_RegognizeTwoIntents()
        {
            string targetName1 = Guid.NewGuid().ToString();
            string targetName2 = Guid.NewGuid().ToString();

            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return new List<Intent>
                {
                    new Intent() { Name = targetName1 },
                    new Intent() { Name = targetName2 }
                };
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(resultingIntents.Count == 2, "Expected exactly 2 intents");
            Assert.IsTrue(resultingIntents[0].Name == targetName1, $"Unexpected Intent Name. Expected {targetName1}");
            Assert.IsTrue(resultingIntents[1].Name == targetName2, $"Unexpected Intent Name. Expected {targetName2}");
        }

        [TestMethod]
        public async Task Intents_DisableIntent()
        {
            string targetName = Guid.NewGuid().ToString();

            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return new List<Intent>
                {
                    new Intent() { Name = targetName },
                };
            });
            bool enabled = false;
            m.OnEnabled(async (context) =>
            {
                return enabled;
            });

            BotContext bc = TestUtilities.CreateEmptyContext();

            // Test that the Intent comes back when the OnEnabled method returns true
            enabled = true;
            var resultingIntents = await m.Recognize(bc);

            Assert.IsTrue(resultingIntents.Count == 1, "Expected exactly 1 intent");
            Assert.IsTrue(resultingIntents.First().Name == targetName, $"Unexpected Intent Name. Expected {targetName}");

            // Test that NO Intent comes back when the OnEnabled method returns false
            enabled = false;
            var resultingIntents2 = await m.Recognize(bc);
            Assert.IsTrue(resultingIntents2.Count == 0, "Expected exactly 0 intent");
        }

        [TestMethod]
        public async Task Intents_MutateIntentResult()
        {
            string targetName = Guid.NewGuid().ToString();
            string replacedName = Guid.NewGuid().ToString();

            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return new List<Intent>
                {
                    new Intent() { Name = targetName },
                };
            });

            m.OnFilter(async (context, intentList) =>
            {
                // When this code is called, the intent should already have been recognized. This code, as "filter code"
                // has the oppertunity to manipulate that intent. 

                Assert.IsTrue(intentList.Count == 1, "Expecting exactly 1 intent");
                Assert.IsTrue(intentList.First().Name == targetName, $"Unexpected Intent Name. Expected {targetName}");

                // replace the name of the intent. Do this via the Context to vette paremeter passing
                intentList[0].Name = replacedName;
            });

            BotContext bc = TestUtilities.CreateEmptyContext();

            // Test that the Intent comes back has been "filtered" to have the revised name

            var resultingIntents = await m.Recognize(bc);

            Assert.IsTrue(resultingIntents.Count == 1, "Expected exactly 1 intent");
            Assert.IsTrue(resultingIntents.First().Name == replacedName, $"Unexpected Intent Name. Expected {replacedName}");
        }

        [TestMethod]
        public async Task Intents_RemoveIntentViaFilter()
        {
            string intentToKeep = Guid.NewGuid().ToString();
            string intentToRemove = Guid.NewGuid().ToString();

            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            m.OnRecognize(async (context) =>
            {
                return new List<Intent>
                {
                    new Intent() { Name = intentToKeep },
                    new Intent() { Name = intentToRemove }
                };
            });

            m.OnFilter(async (context, intentList) =>
            {
                Assert.IsTrue(intentList.Count == 2, "Expecting exactly 2 intents");
                Assert.IsTrue(intentList[0].Name == intentToKeep, $"Unexpected Intent Name. Expected {intentToKeep}");
                Assert.IsTrue(intentList[1].Name == intentToRemove, $"Unexpected Intent Name. Expected {intentToRemove}");

                intentList.RemoveAt(1);
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            var resultingIntents = await m.Recognize(bc);

            Assert.IsTrue(resultingIntents.Count == 1, "Expected exactly 1 intent");
            Assert.IsTrue(resultingIntents.First().Name == intentToKeep, $"Unexpected Intent Name. Expected {intentToKeep}");
        }

        [TestMethod]
        public async Task Intents_ValidateFilterOrder()
        {
            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            string shouldRun = null;
            /*
             *  Filters are required to run in reverse order. This code validates that by registering 3 filters and
             *  running a simple state machine across them. 
             */
            m.OnFilter(async (context, intentList) =>
            {
                Assert.IsTrue(shouldRun == "third", "1st filter did not run last");
                shouldRun = "done";
            });

            m.OnFilter(async (context, intentList) =>
            {
                Assert.IsTrue(shouldRun == "second", "2nd filter did not run second");
                shouldRun = "third";
            });

            m.OnFilter(async (context, intentList) =>
            {
                Assert.IsTrue(shouldRun == "first", "last filter did not run first");
                shouldRun = "second";
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            shouldRun = "first";

            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(shouldRun == "done", "Final filter did not run");
        }

        [TestMethod]
        public async Task Intents_ValidateRecognizerOrder()
        {
            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            string shouldRun = null;

            /*
             *  Filters are required to run in reverse order. This code validates that by registering 3 filters and
             *  running a simple state machine across them. 
             */
            m.OnRecognize(async (context) =>
            {
                Assert.IsTrue(shouldRun == "first", "1st recognizer did not run first");
                shouldRun = "second";

                return new List<Intent>();
            });

            m.OnRecognize(async (context) =>
            {
                Assert.IsTrue(shouldRun == "second", "2st recognizer did not run second");
                shouldRun = "third";

                return new List<Intent>();
            });

            m.OnRecognize(async (context) =>
            {
                Assert.IsTrue(shouldRun == "third", "3rd recognizer did not run last");
                shouldRun = "done";

                return new List<Intent>();
            });


            BotContext bc = TestUtilities.CreateEmptyContext();
            shouldRun = "first";

            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(shouldRun == "done", "Final recognizer did not run");
        }

        [TestMethod]
        public async Task Intents_ValidateEnablerOrder()
        {
            IntentRecognizerMiddleware m = new IntentRecognizerMiddleware();
            string shouldRun = "first" ;

            /*
             *  Filters are required to run in reverse order. This code validates that by registering 3 filters and
             *  running a simple state machine across them. 
             */
            m.OnEnabled(async (context) =>
            {
                Assert.IsTrue(shouldRun == "first", "1st enabler did not run first.");
                shouldRun  = "second";
                return true;
            });

            m.OnEnabled(async (context) =>
            {
                Assert.IsTrue(shouldRun == "second", "2nd enabler did not run second");
                shouldRun = "third";

                return true;
            });

            m.OnEnabled(async (context) =>
            {
                Assert.IsTrue(shouldRun == "third", "3rd enabler did not run last");
                shouldRun = "done";

                return true;
            });

            BotContext bc = TestUtilities.CreateEmptyContext();
            shouldRun = "first";

            var resultingIntents = await m.Recognize(bc);
            Assert.IsTrue(shouldRun == "done", "Final recognizer did not run");
        }
       
        [TestMethod]
        public void Intents_TopIntentsOrdering()
        {
            string small = "small";
            string medium = "medium";
            string large = "large";

            List<Intent> intents = new List<Intent>
            {
                new Intent { Name = small, Score = 0.0 },
                new Intent { Name = medium, Score = 0.5 },
                new Intent { Name = large, Score = 1.0 }
            };

            for (int i = 0; i < 100; i++)
            {
                Shuffle(intents);
                Assert.IsTrue(IntentRecognizerMiddleware.FindTopIntent(intents).Name == large, "Not the top intent");
            }
        }
    
        private static Random rng = new Random();

        public void Shuffle<T>(IList<T> list)
        {
            // Note: Code taken from Stack Overflow: 
            // https://stackoverflow.com/questions/273313/randomize-a-listt/1262619#1262619
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
