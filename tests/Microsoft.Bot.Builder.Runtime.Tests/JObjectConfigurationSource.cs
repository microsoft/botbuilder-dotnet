// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Runtime.Tests
{
    public class JObjectConfigurationSource : IConfigurationSource
    {
        private readonly JObject jObject;

        public JObjectConfigurationSource(JObject jObject)
        {
            this.jObject = jObject ?? throw new ArgumentNullException(nameof(jObject));
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return new JObjectConfigurationProvider(this.jObject);
        }
    }
}
