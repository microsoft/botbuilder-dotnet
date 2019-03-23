using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Expressions
{
    public interface IExpressionVisitor
    {
        void Visit(Accessor expression);
        void Visit(Binary expression);
        void Visit(Call expression);
        void Visit(Constant expression);
        void Visit(Element expression);
        void Visit(Expression expression);
        void Visit(NAry expression);
        void Vist(Unary expression);
    }
}
