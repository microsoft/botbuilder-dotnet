// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Builder.Runtime.Tests.Extensions
{
    public class FeatureTests
    {
        public static IEnumerable<object[]> FeatureTestData()
        {
            // params: ResourcesSettings settings, Type registeredType

            // no settings object
            yield return new object[]
            {
                null,
                null
            };

            // showtyping false
            yield return new object[]
            {
                new RuntimeSettings()
                {
                    Features = new FeatureSettings()
                    {
                        ShowTyping = false
                    }
                },
                null
            };

            // showtyping true
            yield return new object[]
            {
                new RuntimeSettings()
                {
                    Features = new FeatureSettings()
                    {
                        ShowTyping = true
                    }
                },
                typeof(ShowTypingMiddleware)
            };

            // RemoveRecipientMentions false
            yield return new object[]
            {
                new RuntimeSettings()
                {
                    Features = new FeatureSettings()
                    {
                        RemoveRecipientMentions = false
                    }
                },
                null
            };

            // RemoveRecipientMentions true
            yield return new object[]
            {
                new RuntimeSettings()
                {
                    Features = new FeatureSettings()
                    {
                        RemoveRecipientMentions = true
                    }
                },
                typeof(NormalizeMentionsMiddleware)
            };

            // SetSpeak enabled
            yield return new object[]
            {
                new RuntimeSettings()
                {
                    Features = new FeatureSettings()
                    {
                        SetSpeak = new SpeakSettings
                        {
                            VoiceFontName = "en-US-AriaNeural",
                            FallbackToTextForSpeechIfEmpty = true
                        }
                    }
                },
                typeof(SetSpeakMiddleware)
            };
        }

        [Theory]
        [MemberData(nameof(FeatureTestData))]
        internal void FeatureFlags(RuntimeSettings settings, Type registeredType)
        {
            // Setup
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddRuntimeSettings(settings)
                .Build();

            services.AddSingleton(configuration);

            // Test
            services.AddBotRuntimeFeatures(configuration);

            // Assert
            var provider = services.BuildServiceProvider();

            if (registeredType == null)
            {
                Assert.Null(provider.GetService<IEnumerable<IMiddleware>>().FirstOrDefault());
            }
            else
            {
                Assert.IsType(registeredType, provider.GetService<IEnumerable<IMiddleware>>().First());
            }
        }
    }
}
