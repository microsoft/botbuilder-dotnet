using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Expressions
{
    public partial class Extensions
    {
        /// <summary>
        /// Return the static reference paths to memory.
        /// </summary>
        /// <remarks>
        /// Return all static paths to memory.  If there is a computed element index, then the path is terminated there, 
        /// but you might get paths from the computed part as well.
        /// </remarks>
        /// <param name="expression">Expresion to get references from.</param>
        /// <returns>List of the static reference paths.</returns>
        public static IReadOnlyList<string> References(this Expression expression)
        {
            var walker = new ReferenceVisitor();
            expression.Accept(walker);
            return walker.References;
        }

        private class ReferenceVisitor : IExpressionVisitor
        {
            public List<string> References = new List<string>();
            private string _path = null;

            public void Visit(Accessor expression)
            {
                if (expression.Instance == null)
                {
                    _path = expression.Property;
                }
                else
                {
                    expression.Instance.Accept(this);
                    if (_path != null)
                    {
                        _path += $".{expression.Property}";
                    }
                }
            }

            protected void NonPath(Expression expression)
            {
                foreach (var child in expression.Children)
                {
                    child.Accept(this);
                    if (_path != null)
                    {
                        References.Add(_path);
                    }
                }
            }

            public void Visit(Binary expression)
            {
                NonPath(expression);
            }

            public void Visit(Call expression)
            {
                NonPath(expression);
            }

            public void Visit(Constant expression)
            {
                NonPath(expression);
            }

            public void Visit(Element expression)
            {
                expression.Instance.Accept(this);
                if (_path != null)
                {
                    if (expression.Index is Constant constant)
                    {
                        _path += $"[{constant.Value}]";
                    }
                    else
                    {
                        References.Add(_path);
                        _path = null;
                        expression.Index.Accept(this);
                    }
                } 
                else
                {
                    expression.Index.Accept(this);
                }
            }

            public void Visit(Expression expression)
            {
                NonPath(expression);
            }

            public void Visit(NAry expression)
            {
                NonPath(expression);
            }

            public void Visit(Unary expression)
            {
                NonPath(expression);
            }

            public void Vist(Unary expression)
            {
                NonPath(expression);
            }
        }
    }
}
