// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
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
            Assert.Empty(new AllowedTypesSerializationBinder(null).AllowedTypes);
            Assert.NotNull(new AllowedTypesSerializationBinder(null).AllowedTypes);

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
        }

        [Fact]
        public void BindToNameWithType()
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
        public void BindToTypeWithAllowedType()
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
        }

        [Fact]
        public void BindToTypeWithoutAllowedType()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { typeof(string) });
            Assert.Throws<InvalidOperationException>(() => binder.BindToType(expectedType.Assembly.GetName().Name, expectedType.FullName));
        }

        [Fact]
        public void CleanupTypesWithAllowedTypes()
        {
            var expectedType = typeof(ExampleType);
            var unknownType = typeof(string);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { expectedType });
            var resolvedTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
            var unknownTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", unknownType.FullName, unknownType.Assembly.GetName().Name);
            var obj = JObject.Parse(@$"{{'$type':'{resolvedTypeName}', 'inner': {{ '$type': '{unknownTypeName}' }} }}");
            binder.CleanupTypes(obj);

            Assert.Equal(resolvedTypeName, obj["$type"]);
            Assert.Null(obj["inner"]["$type"]);
        }
        
        [Fact]
        public void CleanupTypesWithoutAllowedTypes()
        {
            var expectedType = typeof(ExampleType);
            var stringType = typeof(string);
            var activityType = typeof(Activity);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { expectedType });
            var resolvedTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
            var stringTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", stringType.FullName, stringType.Assembly.GetName().Name);
            var activityTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", activityType.FullName, activityType.Assembly.GetName().Name);
            var obj = (JContainer)JToken.Parse(@$"[
                {{'$type':'{resolvedTypeName}', 'inner': {{ '$type': '{stringTypeName}' }}, 'activity': {{ '$type': '{activityTypeName}' }} }},
                {{'$type':'{stringTypeName}', 'inner': {{ '$type': '{resolvedTypeName}' }} }},
                {{'$type':'{activityTypeName}', 'inner': {{ '$type': '{resolvedTypeName}' }} }}
            ]");
            Assert.Throws<InvalidOperationException>(() => binder.CleanupTypes(obj));
        }
        
        [Fact]
        public void CleanupTypesWithEmptyObject()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { expectedType });
            var obj = (JContainer)JToken.Parse("{}");
            binder.CleanupTypes(obj);
        }

        [Fact]
        public void CleanupTypesWithEmptyArray()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder(new List<Type> { expectedType });
            var obj = (JContainer)JToken.Parse("[]");
            binder.CleanupTypes(obj);
        }

        [Fact]
        public void CleanupTypesWithEmptyAllowedTypes()
        {
            var expectedType = typeof(ExampleType);
            var binder = new AllowedTypesSerializationBinder();
            var resolvedTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
            var obj = JObject.Parse(@$"{{'$type':'{resolvedTypeName}'}}");
            binder.CleanupTypes(obj);

            Assert.Equal(resolvedTypeName, obj["$type"]);
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
