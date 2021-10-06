// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Debugging.DataModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Tests.DataModels
{
    public sealed class DataModelsTests
    {
        [Fact]
        public void DataModel_Properties()
        {
            ICoercion coercion = new Coercion();
            IDataModel dataModel = new DataModel(coercion);

            Assert.Equal(2147483647, dataModel.Rank);

            var scalar = dataModel.IsScalar("context");
            Assert.True(scalar);

            scalar = dataModel.IsScalar(null);
            Assert.True(scalar);

            var names = dataModel.Names("context");
            Assert.Empty(names);
        }

        [Fact]
        public void DataModel_ToString()
        {
            ICoercion coercion = new Coercion();
            IDataModel dataModel = new DataModel(coercion);

            var model = dataModel.ToString("context");
            Assert.Equal("context", model);

            var jTokenResult = dataModel.ToString(new JValue(true));
            Assert.Equal("True", jTokenResult);

            var classResult = dataModel.ToString(new Coercion());
            Assert.Equal("Microsoft.Bot.Builder.Dialogs.Debugging.DataModels.Coercion", classResult);

            var enumerableTypeResult = dataModel.ToString(new TestEnumerableDataModel());
            Assert.Equal(
                "Microsoft.Bot.Builder.Dialogs.Debugging.Tests.DataModels.DataModelsTests+TestEnumerableDataModel",
                enumerableTypeResult);

            var readOnlyDictionaryTypeResult = dataModel.ToString(new TestReadOnlyDictionaryDataModel());
            Assert.Equal(
                "Microsoft.Bot.Builder.Dialogs.Debugging.Tests.DataModels.DataModelsTests+TestReadOnlyDictionaryDataModel",
                readOnlyDictionaryTypeResult);

            var dictionaryTypeResult = dataModel.ToString(new TestDictionaryDataModel());
            Assert.Equal(
                "Microsoft.Bot.Builder.Dialogs.Debugging.Tests.DataModels.DataModelsTests+TestDictionaryDataModel",
                dictionaryTypeResult);

            var listTypeResult = dataModel.ToString(new TestListDataModel());
            Assert.Equal("Microsoft.Bot.Builder.Dialogs.Debugging.Tests.DataModels.DataModelsTests+TestListDataModel", listTypeResult);
        }

        [Fact]
        public void DictionaryDataModel_Properties()
        {
            ICoercion coercion = new Coercion();
            var dictionaryDataModel = new DictionaryDataModel<string, string>(coercion);

            Assert.Equal(6, dictionaryDataModel.Rank);

            var context = new Dictionary<string, string> { { "key", "value" } };
            var names = dictionaryDataModel.Names(context);
            Assert.Single(names);
            Assert.Equal("key", names.First());
        }

        [Fact]
        public void EnumerableDataModel_Properties()
        {
            ICoercion coercion = new Coercion();
            var dictionaryDataModel = new EnumerableDataModel<string>(coercion);

            Assert.Equal(3, dictionaryDataModel.Rank);

            var names = dictionaryDataModel.Names(new List<string> { "context" });
            Assert.Single(names);
            Assert.Equal(0, names.First());
        }

        [Fact]
        public void JValueDataModel_Properties()
        {
            var jValueDataModel = JValueDataModel.Instance;

            Assert.Equal(1, jValueDataModel.Rank);

            Assert.True(jValueDataModel.IsScalar("context"));

            Assert.Empty(jValueDataModel.Names("context"));

            Assert.Equal("context", jValueDataModel.ToString("context"));
        }

        [Fact]
        public void ListDataModel_Properties()
        {
            ICoercion coercion = new Coercion();
            IDataModel listDataModel = new ListDataModel<string>(coercion);

            Assert.Equal(4, listDataModel.Rank);

            var names = listDataModel.Names(new List<string> { "context" });
            Assert.Single(names);
        }

        [Fact]
        public void NullDataModel_Properties()
        {
            var nullDataModel = NullDataModel.Instance;

            Assert.Equal(0, nullDataModel.Rank);

            Assert.True(nullDataModel.IsScalar("context"));

            Assert.Empty(nullDataModel.Names("context"));

            Assert.Equal("null", nullDataModel.ToString("context"));
        }

        [Fact]
        public void ReadOnlyDictionaryDataModel_Properties()
        {
            ICoercion coercion = new Coercion();
            var readOnlyDictDataModel = new ReadOnlyDictionaryDataModel<string, string>(coercion);

            Assert.Equal(5, readOnlyDictDataModel.Rank);

            var context = new Dictionary<string, string> { { "key", "value" } };
            var names = readOnlyDictDataModel.Names(context);
            Assert.Single(names);
            Assert.Equal("key", names.First());
        }

        [Fact]
        public void ReflectionDataModel_Properties()
        {
            ICoercion coercion = new Coercion();
            IDataModel reflectionDataModel = new ReflectionDataModel<string>(coercion);

            Assert.Equal(2, reflectionDataModel.Rank);

            var names = reflectionDataModel.Names(new List<string> { "context" });
            Assert.Equal(2, names.Count());
        }

        [Fact]
        public void ScalarDataModel_Properties()
        {
            var scalarDataModel = ScalarDataModel.Instance;

            Assert.Equal(1, scalarDataModel.Rank);

            Assert.True(scalarDataModel.IsScalar("context"));

            Assert.Empty(scalarDataModel.Names("context"));

            Assert.Equal("context", scalarDataModel.ToString("context"));
        }

        internal class TestEnumerableDataModel : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }
        }

        internal class TestReadOnlyDictionaryDataModel : IReadOnlyDictionary<string, string>
        {
            public IEnumerable<string> Keys => throw new System.NotImplementedException();

            public IEnumerable<string> Values => throw new System.NotImplementedException();

            public int Count => throw new System.NotImplementedException();

            public string this[string key] => throw new System.NotImplementedException();

            public bool ContainsKey(string key)
            {
                throw new System.NotImplementedException();
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            public bool TryGetValue(string key, out string value)
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }
        }

        internal class TestDictionaryDataModel : IDictionary<string, string>
        {
            public int Count => throw new System.NotImplementedException();

            public bool IsReadOnly => throw new System.NotImplementedException();

            public ICollection<string> Values => throw new System.NotImplementedException();

            public ICollection<string> Keys => throw new System.NotImplementedException();

            public string this[string key]
            {
                get => throw new System.NotImplementedException();
                set => throw new System.NotImplementedException();
            }

            public void Add(string key, string value)
            {
                throw new System.NotImplementedException();
            }

            public void Add(KeyValuePair<string, string> item)
            {
                throw new System.NotImplementedException();
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                throw new System.NotImplementedException();
            }

            public bool ContainsKey(string key)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(string key)
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                throw new System.NotImplementedException();
            }

            public bool TryGetValue(string key, out string value)
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }
        }

        internal class TestListDataModel : IList<string>
        {
            public int Count => throw new System.NotImplementedException();

            public bool IsReadOnly => throw new System.NotImplementedException();

            public string this[int index]
            {
                get => throw new System.NotImplementedException();
                set => throw new System.NotImplementedException();
            }

            public void Add(string item)
            {
                throw new System.NotImplementedException();
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains(string item)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public IEnumerator<string> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            public int IndexOf(string item)
            {
                throw new System.NotImplementedException();
            }

            public void Insert(int index, string item)
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(string item)
            {
                throw new System.NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
