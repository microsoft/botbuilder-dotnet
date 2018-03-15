using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Classic.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class GetTokenRefreshTests
    {
        public GetTokenRefreshTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public async Task TokenTests_GetCredentialsWorks()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("10c55330-7945-4008-b2c5-9e91cb5e5d34", "cPVCp1|l!8T=>-Fz");
            var result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task TokenTests_RefreshTokenWorks()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("10c55330-7945-4008-b2c5-9e91cb5e5d34", "cPVCp1|l!8T=>-Fz");
            var result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
            var result2 = await credentials.GetTokenAsync();
            Assert.AreEqual(result, result2);
            var result3 = await credentials.GetTokenAsync(true);
            Assert.IsNotNull(result3);
            Assert.AreNotEqual(result2, result3);
        }

        [TestMethod]
        public async Task TokenTests_RefreshTestLoad()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("10c55330-7945-4008-b2c5-9e91cb5e5d34", "cPVCp1|l!8T=>-Fz");
            List<Task<string>> tasks = new List<Task<string>>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(credentials.GetTokenAsync());
            }

            string prevResult = null;
            foreach (var item in tasks)
            {
                string result = await item;
                Assert.IsNotNull(result);
                if (prevResult != null)
                    Assert.AreEqual(prevResult, result);
                prevResult = result;
            }

            tasks.Clear();
            for (int i = 0; i < 1000; i++)
            {
                if (i % 100 == 50)
                    tasks.Add(credentials.GetTokenAsync(true));
                else
                    tasks.Add(credentials.GetTokenAsync());
            }

            HashSet<string> results = new HashSet<string>();
            for(int i=0; i < 1000; i++)
            {
                string result = await tasks[i];
                if (i == 0)
                    results.Add(result);
                Assert.IsNotNull(result);
                if (prevResult != null)
                {
                    if (i % 100 == 50)
                    {
                        Assert.IsTrue(!results.Contains(result));
                        results.Add(result);
                    }
                    else
                        Assert.IsTrue(results.Contains(result));
                }
            }

        }
    }
}
