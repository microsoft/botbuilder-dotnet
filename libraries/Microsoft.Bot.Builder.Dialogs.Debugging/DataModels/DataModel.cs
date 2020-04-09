// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class DataModel : IDataModel
    {
        private readonly ICoercion coercion;

        private readonly Dictionary<Type, IDataModel> modelByType = new Dictionary<Type, IDataModel>();

        public DataModel(ICoercion coercion)
        {
            this.coercion = coercion ?? throw new ArgumentNullException(nameof(coercion));
        }

        int IDataModel.Rank => int.MaxValue;

        object IDataModel.this[object context, object name]
        {
            get => ModelFor(context)[context, name];
            set => ModelFor(context)[context, name] = value;
        }

        bool IDataModel.IsScalar(object context) => ModelFor(context).IsScalar(context);

        IEnumerable<object> IDataModel.Names(object context) => ModelFor(context).Names(context);

        string IDataModel.ToString(object context) => ModelFor(context).ToString(context);

        private IDataModel Create(Type definition, params Type[] typeArguments) =>
            (IDataModel)Activator.CreateInstance(definition.MakeGenericType(typeArguments), this.coercion);

        private IEnumerable<IDataModel> Options(Type type)
        {
            if (type.IsPrimitive || type.IsValueType || type == typeof(string))
            {
                yield return ScalarDataModel.Instance;
                yield break;
            }

            if (type == typeof(JValue))
            {
                yield return JValueDataModel.Instance;
                yield break;
            }

            var ifaces = type.GetInterfaces();
            foreach (var iface in ifaces)
            {
                if (iface.IsGenericType)
                {
                    var definition = iface.GetGenericTypeDefinition();
                    var arguments = iface.GetGenericArguments();
                    if (definition == typeof(IReadOnlyDictionary<,>))
                    {
                        yield return Create(typeof(ReadOnlyDictionaryDataModel<,>), arguments);
                    }
                    else if (definition == typeof(IDictionary<,>))
                    {
                        yield return Create(typeof(DictionaryDataModel<,>), arguments);
                    }
                    else if (definition == typeof(IList<>))
                    {
                        yield return Create(typeof(ListDataModel<>), arguments);
                    }
                    else if (definition == typeof(IEnumerable<>))
                    {
                        yield return Create(typeof(EnumerableDataModel<>), arguments);
                    }
                }
            }

            if (type.IsClass)
            {
                yield return Create(typeof(ReflectionDataModel<>), type);
            }
        }

        private IDataModel ModelFor(object context)
        {
            if (context == null)
            {
                return NullDataModel.Instance;
            }

            var type = context.GetType();
            lock (modelByType)
            {
                if (!modelByType.TryGetValue(type, out var model))
                {
                    var options = Options(type);
                    model = options.OrderByDescending(m => m.Rank).First();
                    modelByType.Add(type, model);
                }

                return model;
            }
        }
    }
}
