// Copyright(c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Expressions
{
    public static partial class Extensions
    {
        /// <summary>
        /// Rewrite the expression by pushing not down to the leaves.
        /// </summary>
        /// <remarks>
        /// This uses DeMorgan's law to push not through and/or/not to the leaves.  
        /// It also rewrites comparisons to invert them.
        /// </remarks>
        /// <param name="expression">Expression to rewrite.</param>
        /// <param name="passThrough">Any functions that do not change with not.</param>
        /// <returns>Rewritten expression.</returns>
        static public Expression PushDownNot(this Expression expression, HashSet<string> passThrough = null) => PushDownNot(expression, passThrough, false);

        /// <summary>
        /// Rewrite expression into disjunctive normal form.
        /// </summary>
        /// <remarks>
        /// Rewrites to either a simple expression or a disjunction of conjunctions or simple expressions with not pushed down
        /// to leaves.
        /// </remarks>
        /// <param name="expression">Expression to rewrite.</param>
        /// <param name="passThrough">Any functions that not should pass through.</param>
        /// <returns>Normalized expression.</returns>
        static public Expression DisjunctiveNormalForm(this Expression expression, HashSet<string> passThrough = null)
        {
            Expression result;
            if (passThrough == null)
            {
                passThrough = new HashSet<string>();
            }
            var clauses = Conjunctions(expression.PushDownNot(passThrough));
            var children = new List<Expression>();
            foreach(var clause in clauses)
            {
                if (clause.Type == ExpressionType.And &&
                    clause.Children.Length == 1)
                {
                    children.Add(clause.Children[0]);
                }
                else
                {
                    children.Add(clause);
                }
            }
            if (children.Count == 0)
            {
                result = Expression.ConstantExpression(false);
            }
            else if (children.Count == 1)
            {
                result = children[0];
            }
            else
            {
                result = Expression.MakeExpression(ExpressionType.Or, null, children.ToArray());
            }
            return result;
        }

        static private IEnumerable<Expression> Conjunctions(Expression expression)
        {
            switch (expression.Type)
            {
                case ExpressionType.And:
                    {
                        // Each element of SoFar is a conjunction
                        // Need to combine every combination of clauses
                        var soFar = new List<Expression>();
                        var first = true;
                        foreach (var child in expression.Children)
                        {
                            var clauses = Conjunctions(child).ToList();
                            if (clauses.Count() == 0)
                            {
                                // Encountered false
                                soFar.Clear();
                                break;
                            }
                            if (first)
                            {
                                foreach (var clause in clauses)
                                {
                                    soFar.Add(clause);
                                }
                                first = false;
                            }
                            else
                            {
                                var newClauses = new List<Expression>();
                                foreach (var old in soFar)
                                {
                                    foreach (var clause in clauses)
                                    {
                                        if (clause.Type == ExpressionType.And)
                                        {
                                            var children = new List<Expression>();
                                            children.AddRange(old.Children);
                                            children.AddRange(clause.Children);
                                            newClauses.Add(Expression.MakeExpression(ExpressionType.And, null, children.ToArray()));
                                        }
                                        else
                                        {
                                            newClauses.Add(old);
                                        }
                                    }
                                }
                                soFar = newClauses;
                            }
                        }
                        foreach (var clause in soFar)
                        {
                            yield return clause;
                        }
                    }
                    break;
                case ExpressionType.Or:
                    {
                        foreach (var child in expression.Children)
                        {
                            foreach (var clause in Conjunctions(child))
                            {
                                yield return clause;
                            }
                        }
                    }
                    break;
                default:
                    // True becomes empty expression and false drops clause
                    if (expression is Constant cnst && cnst.Value is bool val)
                    {
                        if (val == true)
                        {
                            yield return expression;
                        }
                    }
                    else
                    {
                        yield return Expression.MakeExpression(ExpressionType.And, null, expression);
                    }
                    break;
            }
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
                case ExpressionType.NotEqual:
                    if (inNot)
                    {
                        newExpr = Expression.MakeExpression(ExpressionType.Equal, null, expression.Children);
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
                        newExpr = Expression.MakeExpression(expression.Type, expression.Evaluator, PushDownNot(expression.Children[0], passThrough, inNot));
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
