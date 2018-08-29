using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Bot.Connector.Authentication;

namespace Microsoft.Bot.Builder.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    public class GetTokenRefreshTests
    {
        public GetTokenRefreshTests()
        {
        }

        [Fact]
        public async Task TokenTests_GetCredentialsWorks()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("10c55330-7945-4008-b2c5-9e91cb5e5d34", "cPVCp1|l!8T=>-Fz");
            var result = await credentials.GetTokenAsync();
            Assert.NotNull(result);
        }


        [Fact]
        public async Task TokenTests_RefreshTokenWorks()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("10c55330-7945-4008-b2c5-9e91cb5e5d34", "cPVCp1|l!8T=>-Fz");
            var result = await credentials.GetTokenAsync();
            Assert.NotNull(result);
            var result2 = await credentials.GetTokenAsync();
            Assert.Equal(result, result2);
            var result3 = await credentials.GetTokenAsync(true);
            Assert.NotNull(result3);
            Assert.NotEqual(result2, result3);
        }

        [Fact]
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
                Assert.NotNull(result);
                if (prevResult != null)
                    Assert.Equal(prevResult, result);
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
                Assert.NotNull(result);
                if (prevResult != null)
                {
                    if (i % 100 == 50)
                    {
                        Assert.True(!results.Contains(result));
                        results.Add(result);
                    }
                    else
                        Assert.Contains(result, results);
                }
            }

        }
    }
}
