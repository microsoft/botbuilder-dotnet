// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Expressions
{
    public class ReferenceVisitor : IExpressionVisitor
    {
        public HashSet<string> References = new HashSet<string>();
        protected string _path = null;

        public virtual void TerminatePath()
        {
            if (_path != null)
            {
                References.Add(_path);
                _path = null;
            }
        }

        public virtual void VisitChildren(ExpressionWithChildren expression)
        {
            TerminatePath();
            var tree = expression as ExpressionWithChildren;
            foreach (var child in tree.Children)
            {
                child.Accept(this);
                TerminatePath();
            }
        }

        public virtual void Visit(Accessor expression)
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


        public virtual void Visit(Constant expression)
        {
            TerminatePath();
        }

        public virtual void Visit(Expression expression)
        {
            TerminatePath();
        }

        public virtual void Visit(ExpressionWithChildren expression)
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
                VisitChildren(expression);
            }
        }
    }
}
