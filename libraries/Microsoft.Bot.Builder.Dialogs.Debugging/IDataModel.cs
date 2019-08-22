using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IDataModel
    {
        int Rank { get; }

        object this[object context, object name]
        {
            get;
            set;
        }

        bool IsScalar(object context);

        IEnumerable<object> Names(object context);

        string ToString(object context);
    }

    public sealed class NullDataModel : IDataModel
    {
        public static readonly IDataModel Instance = new NullDataModel();

        private NullDataModel()
        {
        }

        int IDataModel.Rank => 0;

        object IDataModel.this[object context, object name] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        bool IDataModel.IsScalar(object context) => true;

        IEnumerable<object> IDataModel.Names(object context) => Enumerable.Empty<object>();

        string IDataModel.ToString(object context) => "null";
    }

    public sealed class ScalarDataModel : IDataModel
    {
        public static readonly IDataModel Instance = new ScalarDataModel();

        private ScalarDataModel()
        {
        }

        int IDataModel.Rank => 1;

        object IDataModel.this[object context, object name] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        bool IDataModel.IsScalar(object context) => true;

        IEnumerable<object> IDataModel.Names(object context) => Enumerable.Empty<object>();

        string IDataModel.ToString(object context) => context.ToString();
    }

    public abstract class DataModelBase<TContext, TName, TValue> : IDataModel
    {
        private readonly ICoercion coercion;

        protected DataModelBase(ICoercion coercion)
        {
            this.coercion = coercion ?? throw new ArgumentNullException(nameof(coercion));
        }

        public abstract int Rank { get; }

        public abstract TValue this[TContext context, TName name]
        {
            get;
            set;
        }

        object IDataModel.this[object context, object name]
        {
            get => this[(TContext)context, Coerce<TName>(name)];
            set => this[(TContext)context, Coerce<TName>(name)] = Coerce<TValue>(value);
        }

        public virtual bool IsScalar(TContext context) => false;

        public abstract IEnumerable<TName> Names(TContext context);

        public virtual string ToString(TContext context) => (context is ICollection collection)
            ? $"Count = {collection.Count}"
            : context.ToString();

        bool IDataModel.IsScalar(object context) => IsScalar((TContext)context);

        string IDataModel.ToString(object context) => ToString((TContext)context);

        IEnumerable<object> IDataModel.Names(object context) => Names((TContext)context).Cast<object>();

        protected T Coerce<T>(object item) => (T)this.coercion.Coerce(item, typeof(T));
    }

    public sealed class DictionaryDataModel<TKey, TValue> : DataModelBase<IDictionary<TKey, TValue>, TKey, TValue>
    {
        public DictionaryDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 6;

        public override TValue this[IDictionary<TKey, TValue> context, TKey name]
        {
            get => context[name];
            set => context[name] = value;
        }

        public override IEnumerable<TKey> Names(IDictionary<TKey, TValue> context) => context.Keys;
    }

    public sealed class ReadOnlyDictionaryDataModel<TKey, TValue> : DataModelBase<IReadOnlyDictionary<TKey, TValue>, TKey, TValue>
    {
        public ReadOnlyDictionaryDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 5;

        public override TValue this[IReadOnlyDictionary<TKey, TValue> context, TKey name]
        {
            get => context[name];
            set => throw new NotSupportedException();
        }

        public override IEnumerable<TKey> Names(IReadOnlyDictionary<TKey, TValue> context) => context.Keys;
    }

    public sealed class ListDataModel<T> : DataModelBase<IList<T>, int, T>
    {
        public ListDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 4;

        public override T this[IList<T> context, int name]
        {
            get => context[name];
            set => context[name] = value;
        }

        public override IEnumerable<int> Names(IList<T> context) => Enumerable.Range(0, context.Count);
    }

    public sealed class EnumerableDataModel<T> : DataModelBase<IEnumerable<T>, int, T>
    {
        public EnumerableDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 3;

        public override T this[IEnumerable<T> context, int name]
        {
            get => context.ElementAt(name);
            set => throw new NotSupportedException();
        }

        public override IEnumerable<int> Names(IEnumerable<T> context) => context.Select((_, index) => index);
    }

    public sealed class ReflectionDataModel<T> : DataModelBase<object, string, object>
    {
        public ReflectionDataModel(ICoercion coercion)
            : base(coercion)
        {
        }

        public override int Rank => 2;

        public override object this[object context, string name]
        {
            get => context.GetType().GetProperty(name).GetValue(context);
            set => context.GetType().GetProperty(name).SetValue(context, value);
        }

        public override IEnumerable<string> Names(object context) =>
            context.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).Select(p => p.Name);
    }

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
            if (type.IsPrimitive || type == typeof(string))
            {
                yield return ScalarDataModel.Instance;
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
