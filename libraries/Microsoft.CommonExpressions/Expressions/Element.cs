using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Element: Binary
    {
        public Element(Expression instance, Expression index)
            : base(ExpressionType.Element, instance, index)
        {
            Instance = instance;
            Index = index;
        }

        public Expression Index { get; }

        public Expression Instance { get; }

        public override async Task<object> Evaluate(IDictionary<string, object> state)
        {
            var idx = (int) await Index.Evaluate(state);
            var val = (await Instance.Evaluate(state) as object[])[idx];
            return val;
        }

        public override string ToString()
        {
            return $"{Instance}[{Index}]";
        }
    }
}
