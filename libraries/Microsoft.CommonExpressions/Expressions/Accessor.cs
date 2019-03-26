using System.Collections.Generic;

namespace Microsoft.Expressions
{
    public class Accessor : ExpressionWithChildren
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
                (value, error) = instance.AccessProperty(Property, this);
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

        public static Accessor MakeExpression(Expression instance, string property)
        {
            var expr = new Accessor(instance, property);
            expr.Validate();
            return expr;
        }
    }
}
