using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Configuration.Tests
{
    public class BotConfigurationEndpointServiceCredentialProviderTests
    {
        [TestClass]
        public class ConstructorTests
        {
            [TestMethod]
            public void NullEndpointServiceThrowsExpectedException()
            {
                try
                {
                    new BotConfigurationEndpointServiceCredentialProvider(null);

                    Assert.Fail("Should have thrown when endpoint service was null.");
                }
                catch (ArgumentNullException exception)
                {
                    Assert.AreEqual<string>("endpointService", exception.ParamName);
                }
            }

            [TestMethod]
            public void SucceedsWithNonNullEndpointService() => new BotConfigurationEndpointServiceCredentialProvider(new EndpointService());
        }

        public class LoadingTests
        {
            [TestClass]
            public class LoadTests
            {
                [TestMethod]
                public void WhenNoBotFilesExistThrows()
                {
                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.Load();

                        Assert.Fail("Should have thrown an exception.");
                    }
                    catch (Exception exception)
                    {
                        Assert.IsInstanceOfType(exception.InnerException, typeof(FileNotFoundException));
                    }
                }

                [TestMethod]
                public void WhenBotFileDoesExistLoads()
                {
                    var provider = BotConfigurationEndpointServiceCredentialProvider.Load(endpointName: "testEndpoint");

                    Assert.IsNotNull(provider);
                }
            }

            [TestClass]
            public class LoadFromTests
            {
                [TestMethod]
                public void NullBotFilePathThrows()
                {
                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.LoadFrom(null);

                        Assert.Fail("Should have thrown an exception.");
                    }
                    catch (ArgumentException exception)
                    {
                        Assert.AreEqual("botConfigurationFilePath", exception.ParamName);
                    }
                }

                [TestMethod]
                public void EmptyBotFilePathThrows()
                {
                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.LoadFrom(string.Empty);

                        Assert.Fail("Should have thrown an exception.");
                    }
                    catch (ArgumentException exception)
                    {
                        Assert.AreEqual("botConfigurationFilePath", exception.ParamName);
                    }
                }

                [TestMethod]
                public void BotFilePathDoesNotExistThrows()
                {
                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.LoadFrom("no-such-file.bot");
                    }
                    catch (Exception exception)
                    {
                        Assert.IsNotNull(exception.InnerException);
                    }
                }

                [TestMethod]
                public void ValidBotFilePathWithValidEndpointNameLoads()
                {
                    var provider = BotConfigurationEndpointServiceCredentialProvider.LoadFrom("test.bot", endpointName: "testEndpoint");

                    Assert.IsNotNull(provider);
                }

                [TestMethod]
                public void ValidBotFilePathWithMissingEndpointNameThrows()
                {
                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.LoadFrom("test.bot", endpointName: "no-such-endpoint");

                        Assert.Fail("Expected an exception to be thrown.");
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }

            [TestClass]
            public class FromConfigurationTests
            {
                [TestMethod]
                public void NullBotConfigurationThrows()
                {
                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.FromConfiguration(null);

                        Assert.Fail("Expected an exception to be thrown.");
                    }
                    catch (ArgumentNullException exception)
                    {
                        Assert.AreEqual<string>("botConfiguration", exception.ParamName);
                    }
                }

                [TestMethod]
                public void MissingEndpointNameThrows()
                {
                    var configuration = new BotConfiguration();

                    try
                    {
                        BotConfigurationEndpointServiceCredentialProvider.FromConfiguration(configuration, endpointName: "no-such-endpoint");

                        Assert.Fail("Expected an exception to be thrown.");
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

                [TestMethod]
                public void ValidEndpointNameResolves()
                {
                    var configuration = new BotConfiguration();
                    configuration.Services.Add(new EndpointService
                    {
                        Name = "test-endpoint"
                    });

                    BotConfigurationEndpointServiceCredentialProvider.FromConfiguration(configuration, endpointName: "test-endpoint");
                }
            }
        }

        public class ICredentialProviderTests
        {
            private static readonly EndpointService TestEndpointService = new EndpointService
            {
                Name = "unit-test",
                AppId = "test-appid",
                AppPassword = "test-apppassword"
            };

            private readonly BotConfigurationEndpointServiceCredentialProvider _provider = new BotConfigurationEndpointServiceCredentialProvider(TestEndpointService);


            [TestClass]
            public class IsValidAppIdAsync : ICredentialProviderTests
            {
                [TestMethod]
                public async Task ReturnsTrueForMatchingAppId()
                {
                    Assert.IsTrue(await _provider.IsValidAppIdAsync(TestEndpointService.AppId));
                }

                [TestMethod]
                public async Task ReturnsFalseForNonMatchingAppId()
                {

                    Assert.IsFalse(await _provider.IsValidAppIdAsync("not-the-right-appid"));
                }

                [TestMethod]
                public async Task ReturnsFalseForNullAppId()
                {
                    Assert.IsFalse(await _provider.IsValidAppIdAsync(null));
                }
            }

            [TestClass]
            public class GetAppPasswordAsync : ICredentialProviderTests
            {
                [TestMethod]
                public async Task ReturnsExpectedPasswordForValidAppId()
                {
                    Assert.AreEqual<string>(TestEndpointService.AppPassword, await _provider.GetAppPasswordAsync(TestEndpointService.AppId));
                }

                [TestMethod]
                public async Task ReturnsNullPasswordForUnknownAppId()
                {
                    var password = await _provider.GetAppPasswordAsync("unknown-appid");

                    Assert.IsNull(password);
                }

                [TestMethod]
                public async Task ReturnsNullPasswordForNullAppId()
                {
                    var password = await _provider.GetAppPasswordAsync(null);

                    Assert.IsNull(password);
                }
            }

            [TestClass]
            public class IsAuthenticationDisabledAsync : ICredentialProviderTests
            {
                [TestMethod]
                public async Task IsFalseForEndpointWithAppIdSet()
                {
                    Assert.IsFalse(await _provider.IsAuthenticationDisabledAsync());
                }

                [TestMethod]
                public async Task IsFalseForEndpointWithNoAppIdSetAndNotNamedDevelopment()
                {
                    var providerWithEndpointWithNoAppId = new BotConfigurationEndpointServiceCredentialProvider(
                        new EndpointService
                        {
                        });

                    Assert.IsFalse(await providerWithEndpointWithNoAppId.IsAuthenticationDisabledAsync());
                }

                [TestMethod]
                public async Task IsTrueForEndpointWithNoAppIdSetAndNamedDevelopment()
                {
                    var providerWithEndpointWithNoAppId = new BotConfigurationEndpointServiceCredentialProvider(
                        new EndpointService
                        {
                            Name = "development"
                        });

                    Assert.IsTrue(await providerWithEndpointWithNoAppId.IsAuthenticationDisabledAsync());
                }
            }
        }

    }
}
