using Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.GrammarChecker.Tests
{
    [TestClass]
    public class GrammarCheckerTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestSingularNoun()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("She wants one apples");
            var expected = "She wants one apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants 1 apples");
            expected = "She wants 1 apple";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestPluralNoun()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("She wants two apple");
            var expected = "She wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants 2 apple");
            expected = "She wants 2 apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("It's about 12 mile away");
            expected = "It's about 12 miles away";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("there is 54 cheap restaurants that allows childs");
            expected = "there are 54 cheap restaurants that allow childs";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("I found a few places matching Hilton near you that have 5 star and have 2 or more star");
            expected = "I found a few places matching Hilton near you that have 5 stars and have 2 or more stars";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void Test1stVerb()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("The apples is delicious");
            var expected = "The apples are delicious";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("The apples looks like delicious");
            expected = "The apples look like delicious";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("125 of them is within one mile");
            expected = "125 of them are within one mile";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void Test3sgVerb()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("The apple are delicious");
            var expected = "The apple is delicious";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("The apple look like delicious");
            expected = "The apple looks like delicious";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestSingularPronoun()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("She want two apples");
            var expected = "She wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("He want two apple");
            expected = "He wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("It want two apple");
            expected = "It wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Any of them want two apple");
            expected = "Any of them wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Each of them want two apple");
            expected = "Each of them wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Everyone want two apple");
            expected = "Everyone wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Every one want two apple");
            expected = "Every one wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Each one want two apple");
            expected = "Each one wants two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Any one want two apple");
            expected = "Any one wants two apples";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestPluralPronoun()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("I wants two apple");
            var expected = "I want two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("We wants two apples");
            expected = "We want two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("They wants two apples");
            expected = "They want two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("You wants two apples");
            expected = "You want two apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Many of them wants two apples");
            expected = "Many of them want two apples";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestElision()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("She wants a apple");
            var expected = "She wants an apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants a orange");
            expected = "She wants an orange";
            Assert.AreEqual(expected, evaled);

            // Exception case
            evaled = checker.CheckText("She goes into an universality");
            expected = "She goes into a universality";
            Assert.AreEqual(expected, evaled);
            
            // Exception case
            evaled = checker.CheckText("an useful tool");
            expected = "a useful tool";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestEnglishNumber()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("She wants one hundred apple");
            var expected = "She wants one hundred apples";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants twenty-one apple");
            expected = "She wants twenty-one apples";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestEnglishOrdinal()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("She wants the fifth apples");
            var expected = "She wants the fifth apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants the twenty-first apples");
            expected = "She wants the twenty-first apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants the 1st apples");
            expected = "She wants the 1st apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants the 2nd apples");
            expected = "She wants the 2nd apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants the 3rd apples");
            expected = "She wants the 3rd apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("She wants the 4th apples");
            expected = "She wants the 4th apple";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestEnglishName()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText("Mary want the apple");
            var expected = "Mary wants the apple";
            Assert.AreEqual(expected, evaled);

            evaled = checker.CheckText("Tom want the twenty-first apple");
            expected = "Tom wants the twenty-first apple";
            Assert.AreEqual(expected, evaled);
        }

        [TestMethod]
        public void TestCheckParagraph()
        {
            var checker = new MockGrammarChecker();
            var evaled = checker.CheckText(
                "I found a few places matching  Hilton near you that have 5 star and have 2 or more star, " +
                "125 of them is within one mile, but none have 3 or more stars. " +
                "That is really cool!" + 
                "Do you wants to have a try ? ");
            var expected = 
                "I found a few places matching  Hilton near you that have 5 stars and have 2 or more stars, " +
                "125 of them are within one mile, but none have 3 or more stars. " +
                "That is really cool!" +
                "Do you want to have a try ? ";
            Assert.AreEqual(expected, evaled);
        }
    }
}
