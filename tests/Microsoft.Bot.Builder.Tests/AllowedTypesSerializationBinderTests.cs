// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class AllowedTypesSerializationBinderTests
    {
        [Fact]
        public void ConstructorValidation()
        {
            // Empty AllowedTypes.
            Assert.NotNull(new AllowedTypesSerializationBinder().AllowedTypes);
            Assert.Empty(new AllowedTypesSerializationBinder().AllowedTypes);

            // Null AllowedTypes.
            Assert.NotNull(new AllowedTypesSerializationBinder(null).AllowedTypes);
            Assert.Empty(new AllowedTypesSerializationBinder(null).AllowedTypes);

            // With AllowedTypes.
            Assert.NotNull(new AllowedTypesSerializationBinder(new List<Type> { typeof(ExampleType) }).AllowedTypes);
            Assert.Single(new AllowedTypesSerializationBinder(new List<Type> { typeof(ExampleType) }).AllowedTypes);
        }

        [Fact]
        public void BindToNameWithEmptyAllowedTypes()
        {
            var binder = new AllowedTypesSerializationBinder();
            binder.BindToName(null, out var assemblyName, out var typeName);

            Assert.Null(assemblyName);
            Assert.Null(typeName);
        }

        [Fact]
        public void BindToNameWithoutAllowedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { typeof(string) });
            binder.BindToName(expectedType, out var assemblyName, out var typeName);

            Assert.Null(assemblyName);
            Assert.NotNull(typeName);
            Assert.Throws<InvalidOperationException>(() => binder.Verify());
        }

        [Fact]
        public void BindToNameWithExplicitAllowedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { expectedType });
            binder.BindToName(expectedType, out var assemblyName, out var typeName);

            Assert.Null(assemblyName);
            Assert.Equal(expectedType.AssemblyQualifiedName, typeName);
        }

        [Fact]
        public void BindToNameWithNestedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { typeof(AllowedTypesSerializationBinderTests) });
            binder.BindToName(expectedType, out var assemblyName, out var typeName);

            Assert.Null(assemblyName);
            Assert.Equal(expectedType.AssemblyQualifiedName, typeName);
            Assert.Contains(binder.AllowedTypes, e => e.AssemblyQualifiedName == typeName);
        }

        [Fact]
        public void BindToNameWithInterfaces()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { typeof(IDisposable) });
            binder.BindToName(expectedType, out var assemblyName, out var typeName);

            Assert.Null(assemblyName);
            Assert.Equal(expectedType.AssemblyQualifiedName, typeName);
            Assert.Contains(binder.AllowedTypes, e => e.AssemblyQualifiedName == typeName);
        }

        [Fact]
        public void BindToTypeWithExplicitAllowedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { expectedType });
            var resultType = binder.BindToType(expectedType.Assembly.GetName().Name, expectedType.FullName);

            Assert.Equal(expectedType, resultType);
        }

        [Fact]
        public void BindToTypeWithoutProcessedAllowedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { typeof(AllowedTypesSerializationBinderTests) });
            var resultType = binder.BindToType(expectedType.Assembly.GetName().Name, expectedType.FullName);

            Assert.Equal(expectedType, resultType);
            binder.Verify();
        }

        [Fact]
        public void BindToTypeWithoutAllowedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { typeof(string) });
            var resultType = binder.BindToType(expectedType.Assembly.GetName().Name, expectedType.FullName);

            Assert.Equal(expectedType, resultType);
            Assert.Throws<InvalidOperationException>(() => binder.Verify());
        }

        private class ExampleType : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
