// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.BotBuilderSamples.Tests.Framework.XUnit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class JsonLuisDataAttribute : DataAttribute
    {
        private readonly string _jsonFileName;
        private readonly string _relativePath;

        public JsonLuisDataAttribute(string jsonFileName, string relativePath)
        {
            _jsonFileName = jsonFileName;
            _relativePath = relativePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return new JsonLuisDataGenerator(_jsonFileName, _relativePath);
        }
    }
}
