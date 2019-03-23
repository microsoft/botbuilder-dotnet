using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Accessor : Unary
    {
        public Accessor(Expression instance, string property)
            : base(ExpressionType.Accessor, instance)
        {
            Instance = instance;
            Property = property;
        }

        public Expression Instance { get; }

        public string Property { get; }

        public override (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> state)
        {
            object value;
            string error = null;
            if (Instance == null)
            {
                if (!state.TryGetValue(Property, out value))
                {
                    error = $"State did not have {Property}.";
                }
            }
            else
            {
                (value, error) = Instance.TryEvaluate(state);
                if (error == null)
                {
                    if (value is IReadOnlyDictionary<string, object> dict)
                    {
                        if (!dict.TryGetValue(Property, out value))
                        {
                            error = $"{Instance} does not have {Property}.";
                        }
                    }
                    else
                    {
                        error = $"{Instance} is not a dictionary.";
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
            var instance = Instance == null ? "<state>" : Instance.ToString();
            return $"{instance}.{Property}";
        }
    }
}
