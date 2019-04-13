using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public interface IDataModel
    {
        object this[object context, object name]
        {
            get;
            set;
        }
        int Rank { get; }
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
        object IDataModel.this[object context, object name] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        int IDataModel.Rank => 0;
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
        object IDataModel.this[object context, object name] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        int IDataModel.Rank => 1;
        bool IDataModel.IsScalar(object context) => true;
        IEnumerable<object> IDataModel.Names(object context) => Enumerable.Empty<object>();
        string IDataModel.ToString(object context) => context.ToString();
    }
    public abstract class DataModelBase<Context, Name, Value> : IDataModel
    {
        private readonly ICoercion coercion;
        protected DataModelBase(ICoercion coercion)
        {
            this.coercion = coercion ?? throw new ArgumentNullException(nameof(coercion));
        }
        protected T Coerce<T>(object item) => (T)this.coercion.Coerce(item, typeof(T));
        public abstract int Rank { get; }
        public abstract Value this[Context context, Name name]
        {
            get;
            set;
        }
        public virtual bool IsScalar(Context context) => false;
        public abstract IEnumerable<Name> Names(Context context);
        public virtual string ToString(Context context) => (context is ICollection collection)
            ? $"Count = {collection.Count}"
            : context.ToString();
        object IDataModel.this[object context, object name]
        {
            get => this[(Context)context, Coerce<Name>(name)];
            set => this[(Context)context, Coerce<Name>(name)] = Coerce<Value>(value);
        }
        bool IDataModel.IsScalar(object context) => IsScalar((Context)context);
        string IDataModel.ToString(object context) => ToString((Context)context);
        IEnumerable<object> IDataModel.Names(object context) => Names((Context)context).Cast<object>();
    }
    public sealed class DictionaryDataModel<K, V> : DataModelBase<IDictionary<K, V>, K, V>
    {
        public DictionaryDataModel(ICoercion coercion) : base(coercion) { }
        public override int Rank => 6;
        public override V this[IDictionary<K, V> context, K name]
        {
            get => context[name];
            set => context[name] = value;
        }
        public override IEnumerable<K> Names(IDictionary<K, V> context) => context.Keys;
    }
    public sealed class ReadOnlyDictionaryDataModel<K, V> : DataModelBase<IReadOnlyDictionary<K, V>, K, V>
    {
        public ReadOnlyDictionaryDataModel(ICoercion coercion) : base(coercion) { }
        public override int Rank => 5;
        public override V this[IReadOnlyDictionary<K, V> context, K name]
        {
            get => context[name];
            set => throw new NotSupportedException();
        }
        public override IEnumerable<K> Names(IReadOnlyDictionary<K, V> context) => context.Keys;
    }
    public sealed class ListDataModel<T> : DataModelBase<IList<T>, int, T>
    {
        public ListDataModel(ICoercion coercion) : base(coercion) { }
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
        public EnumerableDataModel(ICoercion coercion) : base(coercion) { }
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
        public ReflectionDataModel(ICoercion coercion) : base(coercion) { }
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
        public DataModel(ICoercion coercion)
        {
            this.coercion = coercion ?? throw new ArgumentNullException(nameof(coercion));
        }

        private readonly Dictionary<Type, IDataModel> modelByType = new Dictionary<Type, IDataModel>();
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
        int IDataModel.Rank => int.MaxValue;
        object IDataModel.this[object context, object name]
        {
            get => ModelFor(context)[context, name];
            set => ModelFor(context)[context, name] = value;
        }
        bool IDataModel.IsScalar(object context) => ModelFor(context).IsScalar(context);
        IEnumerable<object> IDataModel.Names(object context) => ModelFor(context).Names(context);
        string IDataModel.ToString(object context) => ModelFor(context).ToString(context);
    }
}
