// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    public static partial class Extensions
    {
        static public Expression PushDownNot(this Expression expression, HashSet<string> passThrough = null)
        {
            return PushDownNot(expression, passThrough, false);
        }

        static private Expression PushDownNot(Expression expression, HashSet<string> passThrough, bool inNot)
        {
            var newExpr = expression;
            switch (expression.Type)
            {
                case ExpressionType.And:
                    {
                        if (inNot)
                        {
                            newExpr = Expression.MakeExpression(ExpressionType.Or, null, (from child in expression.Children select PushDownNot(child, passThrough, true)).ToArray());
                        }
                        else
                        {
                            newExpr = Expression.MakeExpression(ExpressionType.And, null, (from child in expression.Children select PushDownNot(child, passThrough, false)).ToArray());
                        }
                    }
                    break;
                case ExpressionType.Or:
                    {
                        if (inNot)
                        {
                            newExpr = Expression.MakeExpression(ExpressionType.And, null, (from child in expression.Children select PushDownNot(child, passThrough, true)).ToArray());
                        }
                        else
                        {
                            newExpr = Expression.MakeExpression(ExpressionType.Or, null, (from child in expression.Children select PushDownNot(child, passThrough, false)).ToArray());
                        }
                    }
                    break;
                case ExpressionType.Not:
                    newExpr = PushDownNot(expression.Children[0], passThrough, !inNot);
                    break;
                // Rewrite comparison operators
                case ExpressionType.LessThan:
                    if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.GreaterThanOrEqual, null, expression.Children);
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.GreaterThan, null, expression.Children);
                    }
                    break;
                case ExpressionType.Equal:
                    if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.NotEqual, null, expression.Children);
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.LessThan, null, expression.Children);
                    }
                    break;
                case ExpressionType.GreaterThan:
                    if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.LessThanOrEqual, null, expression.Children);
                    }
                    break;
                case ExpressionType.Exists:
                    // Rewrite exists(x) -> x != null
                    newExpr = Expression.MakeExpression(inNot ? ExpressionType.Equal : ExpressionType.NotEqual, null, expression.Children[0], Expression.ConstantExpression(null));
                    break;
                default:
                    if (passThrough.Contains(expression.Type))
                    {
                        // Pass through marker functions like optional/ignore to children
                        newExpr = Expression.MakeExpression(expression.Type, null, PushDownNot(expression.Children[0], passThrough, inNot));
                    }
                    else if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.Not, null, expression);
                    }
                    break;
            }
            return newExpr;

        }
    }
}
