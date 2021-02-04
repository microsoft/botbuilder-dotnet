//// Copyright (c) Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License.

//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using Microsoft.Bot.Builder.Azure.Blobs;
//using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
//using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
//using Microsoft.Bot.Builder.Runtime.Extensions;
//using Microsoft.Bot.Builder.Runtime.Settings;
//using Microsoft.Bot.Builder.Runtime.Skills;
//using Microsoft.Bot.Builder.Skills;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Serialization;
//using Xunit;

//namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
//{
//    public class AdapterRegistrationTests
//    {
//        public static IEnumerable<object[]> GetAddBotRuntimeThrowsArgumentNullExceptionData()
//        {
//            IServiceCollection services = new ServiceCollection();
//            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();
//            Func<IServiceProvider, ResourceExplorer> resourceExplorerImplementationFactory =
//                (serviceProvider) => TestDataGenerator.BuildMemoryResourceExplorer();

//            yield return new object[]
//            {
//                "services",
//                (IServiceCollection)null,
//                configuration
//            };

//            yield return new object[]
//            {
//                "configuration",
//                services,
//                (IConfiguration)null
//            };
//        }

//        public static IEnumerable<object[]> GetAddBotRuntimeTranscriptLoggerData()
//        {
//            var settings = new Dictionary<string, string>
//            {
//                { "blobTranscript:connectionString", "connectionString" },
//                { "blobTranscript:containerName", "containerName" },
//            };

//            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = true, BlobTranscript = true },
//                2
//            };
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = true, BlobTranscript = false },
//                1
//            };
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = false, BlobTranscript = true },
//                1
//            };
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = false, BlobTranscript = false },
//                0
//            };
//        }

//        public static IEnumerable<object[]> GetAddBotRuntimeThrowsArgumentNullExceptionData()
//        {
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = true, BlobTranscript = true },
//                2
//            };
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = true, BlobTranscript = false },
//                1
//            };
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = false, BlobTranscript = true },
//                1
//            };
//            yield return new object[]
//            {
//                configuration,
//                new FeatureSettings() { TraceTranscript = false, BlobTranscript = false },
//                0
//            };
//        }

//        [Fact]
//        public void AddBotRuntime_Succeeds()
//        {
//            IServiceCollection services = new ServiceCollection();
//            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

//            services.AddSingleton(TestDataGenerator.BuildMemoryResourceExplorer());
//            services.AddBotRuntime(configuration);
//        }

//        [Theory]
//        [MemberData(nameof(GetAddBotRuntimeThrowsArgumentNullExceptionData))]
//        public void AddBotRuntime_Throws_ArgumentNullException(string paramName, IServiceCollection services, IConfiguration configuration)
//        {
//            Assert.Throws<ArgumentNullException>(
//                paramName,
//                () => services.AddBotRuntime(configuration));
//        }

//        [Theory]
//        [MemberData(nameof(GetAddBotRuntimeTranscriptLoggerData))]
//        public void AddBotRuntimeSkills(object settings)
//        {
//            IServiceCollection services = new ServiceCollection();
//            var skillSettings = settings as SkillSettings;

//            services.AddBotRuntimeSkills(skillSettings);

//            var serviceProvider = services.BuildServiceProvider();

//            AssertConditionalRegistration<SkillConversationIdFactoryBase, SkillConversationIdFactory>(serviceProvider);
//            AssertConditionalRegistration<BotFrameworkClient, SkillHttpClient>(serviceProvider);
//            AssertConditionalRegistration<ChannelServiceHandler, SkillHandler>(serviceProvider);
//            AssertConditionalRegistration<AuthenticationConfiguration, AuthenticationConfiguration>(
//                serviceProvider,
//                config => config.);
//        }

//        [Theory]
//        [MemberData(nameof(GetAddBotRuntimeTranscriptLoggerErrorData))]
//        public void AddBotRuntimeTranscriptLogger_ErrorCases(Dictionary<string, string> settings, Type exceptionType)
//        {
//            IServiceCollection services = new ServiceCollection();
//            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
//            var featureSettings = new FeatureSettings() { BlobTranscript = true };
//            Assert.Throws(exceptionType, () => services.AddBotRuntimeTranscriptLogging(configuration, featureSettings));
//        }

//        [Fact]
//        public void AddBotRuntimeTranscriptLogger_IncorrectConfiguration_Throws()
//        {
//            var settings = new Dictionary<string, string>
//            {
//                { "blobTranscript:connectionStringWRONG", "connectionString" },
//                { "blobTranscript:containerName", "containerName" },
//            };

//            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
//            IServiceCollection services = new ServiceCollection();
//            var featureSettings = new FeatureSettings() { BlobTranscript = true };

//            Assert.Throws<ConfigurationException>(() => services.AddBotRuntimeTranscriptLogging(configuration, featureSettings));
//        }

//        [Theory]
//        [MemberData(nameof(GetAddBotRuntimeTranscriptLoggerData))]
//        public void AddBotRuntimeTranscriptLogger(IConfiguration configuration, object settings, int middlewareCount)
//        {
//            IServiceCollection services = new ServiceCollection();
//            var featureSettings = settings as FeatureSettings;

//            services.AddBotRuntimeTranscriptLogging(configuration, featureSettings);

//            var serviceProvider = services.BuildServiceProvider();

//            var registeredServices = serviceProvider.GetServices<IMiddleware>();
//            Assert.Equal(middlewareCount, registeredServices.Count());

//            foreach (var service in registeredServices)
//            {
//                Assert.IsType<TranscriptLoggerMiddleware>(service);
//            }
//        }

//        private static void AssertConditionalRegistration<TRegistration, TInstance>(
//            ServiceProvider serviceProvider,
//            Action<TInstance> customValidation = null,
//            bool shouldBeRegistered = true)
//        {
//            var registeredServices = serviceProvider.GetServices<TRegistration>();
//            var registeredInstance = registeredServices.OfType<TInstance>().SingleOrDefault();

//            if (shouldBeRegistered)
//            {
//                Assert.NotNull(registeredInstance);
//            }
//            else
//            {
//                Assert.Null(registeredInstance);
//            }

//            customValidation(registeredInstance);
//        }
//    }
//}
