using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Accessor : ExpressionTree
    {
        protected Accessor(Expression instance, string property)
            : base(ExpressionType.Accessor, instance != null ? new List<Expression> { instance } : new List<Expression>(), _accessor)
        {
            Property = property;
        }

        private static IExpressionEvaluator _accessor = new ExpressionEvaluator(
             (expression, state) => (expression as Accessor).Evaluate(state),
             ExpressionReturnType.Object,
            (expression) => { });

        public string Property { get; }

        private (object value, string error) Evaluate(object state)
        {
            object value = null;
            string error = null;
            object instance = state;
            if (Children.Count == 1)
            {
                (instance, error) = Children[0].TryEvaluate(state);
            }
            if (error == null)
            {
                if (instance is IReadOnlyDictionary<string, object> dict)
                {
                    if (!dict.TryGetValue(Property, out value))
                    {
                        error = $"{instance} does not have {Property}.";
                    }
                }
                else
                {
                    // Use reflection
                    var type = instance.GetType();
                    var prop = type.GetProperty(Property);
                    if (prop != null)
                    {
                        value = prop.GetValue(instance);
                    }
                    else
                    {
                        error = $"{instance} does not have {Property}.";
                    }
                }
            }
            return (value, error);
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            var instance = Children.Count == 0 ? "<state>" : Children[0].ToString();
            return $"{instance}.{Property}";
        }

        public static Accessor MakeAccessor(Expression instance, string property)
        {
            var expr = new Accessor(instance, property);
            expr.Validate();
            return expr;
        }
    }
}
