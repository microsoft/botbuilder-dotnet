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

        public override Task<object> Evaluate(IDictionary<string, object> state)
        {
            object result = null;
            if (Instance == null)
            {
                result = state[Property];
            }
            else
            {
                result = (Instance.Evaluate(state) as IDictionary<string, object>)[Property];
            }
            return Task.FromResult(result);
        }

        public override string ToString()
        {
            var instance = Instance == null ? "<state>" : Instance.ToString();
            return $"{instance}.{Property}";
        }
    }
}
