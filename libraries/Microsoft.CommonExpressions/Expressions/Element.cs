using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{
    public class Element : Binary
    {
        public Element(Expression instance, Expression index)
            : base(ExpressionType.Element, instance, index)
        {
            Instance = instance;
            Index = index;
            Children = new List<Expression> { Instance, Index };
        }

        public Expression Index { get; }

        public Expression Instance { get; }

        public override (object value, string error) TryEvaluate(IReadOnlyDictionary<string, object> state)
        {
            object value;
            string error = null;
            (value, error) = Instance.TryEvaluate(state);
            if (error == null)
            {
                var inst = value;
                (value, error) = Index.TryEvaluate(state);
                if (error == null)
                {
                    if (value is int idx)
                    {
                        var itype = inst.GetType();
                        var counter = itype.GetMethod("Count");
                        var indexer = itype.GetProperties().Except(itype.GetDefaultMembers().OfType<PropertyInfo>());
                        if (counter != null && indexer != null)
                        {
                            dynamic idyn = inst;
                            if (idx >= 0 && idyn.Count() > idx)
                            {
                                value = idyn[idx];
                            }
                            else
                            {
                                error = $"{Index} is out of range for ${Instance}";
                            }
                        }
                        else
                        {
                            error = $"{Instance} is not a collection.";
                        }
                    }
                    else
                    {
                        error = $"Could not coerce {Index} to an int.";
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
            return $"{Instance}[{Index}]";
        }
    }
}
