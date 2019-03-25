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
        /// <returns>Hash set of the static reference paths.</returns>
        public static HashSet<string> References(this Expression expression)
        {
            var walker = new ReferenceVisitor();
            expression.Accept(walker);
            walker.TerminalPath();
            return walker.References;
        }

        private class ReferenceVisitor : IExpressionVisitor
        {
            public HashSet<string> References = new HashSet<string>();
            private string _path = null;

            public void TerminalPath()
            {
                if (_path != null)
                {
                    References.Add(_path);
                    _path = null;
                }
            }

            protected void TreePath(Expression expression)
            {
                var tree = expression as ExpressionTree;
                foreach(var child in tree.Children)
                {
                    child.Accept(this);
                    TerminalPath();
                }
            }

            public void Visit(Accessor expression)
            {
                if (expression.Children.Count == 0)
                {
                    _path = expression.Property;
                }
                else
                {
                    expression.Children[0].Accept(this);
                    if (_path != null)
                    {
                        _path += $".{expression.Property}";
                    }
                }
            }


            public void Visit(Constant expression)
            {
                TerminalPath();
            }

            public void Visit(Expression expression)
            {
                TerminalPath();
            }

            public void Visit(ExpressionTree expression)
            {
                if (expression.Type == ExpressionType.Element)
                {
                    var instance = expression.Children[0];
                    var index = expression.Children[1];
                    instance.Accept(this);
                    if (_path != null)
                    {
                        if (index is Constant constant)
                        {
                            _path += $"[{constant.Value}]";
                        }
                        else
                        {
                            References.Add(_path);
                            _path = null;
                            index.Accept(this);
                        }
                    }
                    else
                    {
                        index.Accept(this);
                    }
                }
                else
                {
                    TreePath(expression);
                }
            }
        }
    }
}
