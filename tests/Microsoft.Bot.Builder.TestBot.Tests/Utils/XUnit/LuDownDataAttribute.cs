// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.BotBuilderSamples.Tests.Utils.XUnit
{
    /// <summary>
    /// Represents an implementation of <see cref="DataAttribute"/> which uses an
    /// instance of <see cref="IDataAdapter"/> to get the data for a <see cref="TheoryAttribute"/>
    /// decorated test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class LuDownDataAttribute : DataAttribute
    {
        private readonly string _luDownFileName;
        private readonly string _relativePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuDownDataAttribute"/> class.
        /// </summary>
        /// <param name="class">The class that provides the data.</param>
        public LuDownDataAttribute(string luDownFileName, string relativePath)
        {
            _luDownFileName = luDownFileName;
            _relativePath = relativePath;
        }

        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return new LuDownDataGenerator(_luDownFileName, _relativePath);
            //if (!(Activator.CreateInstance(Class, _luDownFileName, _relativePath) is IEnumerable<object[]> data))
            //{
            //    throw new ArgumentException($"{Class.FullName} must implement IEnumerable<object[]> to be used as ClassData for the test method named '{testMethod.Name}' on {testMethod.DeclaringType.FullName}");
            //}

            //return data;
        }
    }
}
