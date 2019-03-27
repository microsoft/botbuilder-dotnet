// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Expressions
{
    public interface IExpressionVisitor
    {
        void Visit(Accessor expression);
        void Visit(Constant expression);
        void Visit(Expression expression);
        void Visit(ExpressionWithChildren expression);
    }
}
