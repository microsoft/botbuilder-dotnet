// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
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
}
