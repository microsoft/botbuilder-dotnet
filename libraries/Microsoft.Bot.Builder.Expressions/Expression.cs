// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Expressions
{
    /// <summary>
    /// Type expected from evalating an expression.
    /// </summary>
    public enum ReturnType
    {
        /// <summary>
        /// True or false boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// Numerical value like int, float, double, ...
        /// </summary>
        Number,

        /// <summary>
        /// Any value is possible.
        /// </summary>
        Object,

        /// <summary>
        /// String value.
        /// </summary>
        String,
    }

    /// <summary>
    /// An expression which can be analyzed or evaluated to produce a value.
    /// </summary>
    /// <remarks>
    /// This provides an open-ended wrapper that supports a number of built-in functions and can also be extended at runtime.
    /// It also supports validation of the correctness of an expression and evaluation that should be exception free.
    /// </remarks>
    public class Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// Built-in expression constructor.
        /// </summary>
        /// <param name="type">Type of built-in expression from <see cref="ExpressionType"/>.</param>
        /// <param name="children">Child expressions.</param>
        public Expression(string type, params Expression[] children)
        {
            Evaluator = BuiltInFunctions.Lookup(type);
            Children = children;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// Expression constructor.
        /// </summary>
        /// <param name="evaluator">Information about how to validate and evaluate expression.</param>
        /// <param name="children">Child expressions.</param>
        public Expression(ExpressionEvaluator evaluator, params Expression[] children)
        {
            Evaluator = evaluator;
            Children = children;
        }

        /// <summary>
        /// Gets type of expression.
        /// </summary>
        /// <value>
        /// Type of expression.
        /// </value>
        public string Type => Evaluator.Type;

        public ExpressionEvaluator Evaluator { get; }

        /// <summary>
        /// Gets or sets children expressions.
        /// </summary>
        /// <value>
        /// Children expressions.
        /// </value>
        public Expression[] Children { get; set; }

        /// <summary>
        /// Gets expected result of evaluating expression.
        /// </summary>
        /// <value>
        /// Expected result of evaluating expression.
        /// </value>
        public ReturnType ReturnType => Evaluator.ReturnType;

        /// <summary>
        /// Make an expression and validate it.
        /// </summary>
        /// <param name="type">Type of expression from <see cref="ExpressionType"/>.</param>
        /// <param name="children">Child expressions.</param>
        /// <returns>New expression.</returns>
        public static Expression MakeExpression(string type, params Expression[] children)
        {
            var expr = new Expression(type, children);
            expr.Validate();
            return expr;
        }

        /// <summary>
        /// Make an expression and validate it.
        /// </summary>
        /// <param name="evaluator">Information about how to validate and evaluate expression.</param>
        /// <param name="children">Child expressions.</param>
        /// <returns>New expression.</returns>
        public static Expression MakeExpression(ExpressionEvaluator evaluator, params Expression[] children)
        {
            var expr = new Expression(evaluator, children);
            expr.Validate();
            return expr;
        }

        /// <summary>
        /// Construct an expression from a <see cref="EvaluateExpressionDelegate"/>.
        /// </summary>
        /// <param name="function">Function to create an expression from.</param>
        /// <returns>New expression.</returns>
        public static Expression LambaExpression(EvaluateExpressionDelegate function)
            => new Expression(new ExpressionEvaluator(ExpressionType.Lambda, function));

        /// <summary>
        /// Construct an expression from a lamba expression over the state.
        /// </summary>
        /// <remarks>Exceptions will be caught and surfaced as an error string.</remarks>
        /// <param name="function">Lambda expression to evaluate.</param>
        /// <returns>New expression.</returns>
        public static Expression Lambda(Func<object, object> function)
            => new Expression(new ExpressionEvaluator(ExpressionType.Lambda, (expression, state) =>
            {
                object value = null;
                string error = null;
                try
                {
                    value = function(state);
                }
                catch (Exception e)
                {
                    error = e.Message;
                }

                return (value, error);
            }));

        /// <summary>
        /// Construct and validate an Set a property expression to a value expression.
        /// </summary>
        /// <param name="property">property expression.</param>
        /// <param name="value">value expression.</param>
        /// <returns>New expression.</returns>
        public static Expression SetPathToValue(Expression property, Expression value)
            => Expression.MakeExpression(ExpressionType.SetPathToValue, property, value);

        /// <summary>
        /// Construct and validate an Set a property expression to a value expression.
        /// </summary>
        /// <param name="property">property expression.</param>
        /// <param name="value">value object.</param>
        /// <returns>New expression.</returns>
        public static Expression SetPathToValue(Expression property, object value)
        {
            if (value is Expression)
            {
                return Expression.MakeExpression(ExpressionType.SetPathToValue, property, (Expression)value);
            }
            else
            {
                return Expression.MakeExpression(ExpressionType.SetPathToValue, property, Expression.ConstantExpression(value));
            }
        }

        /// <summary>
        /// Construct and validate an Equals expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression EqualsExpression(params Expression[] children)
        => Expression.MakeExpression(ExpressionType.Equal, children);

        /// <summary>
        /// Construct and validate an And expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression AndExpression(params Expression[] children)
        {
            if (children.Count() > 1)
            {
                return Expression.MakeExpression(ExpressionType.And, children);
            }
            return children.Single();
        }

        /// <summary>
        /// Construct and validate an Or expression.
        /// </summary>
        /// <param name="children">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression OrExpression(params Expression[] children)
        {
            if (children.Count() > 1)
            {
                return Expression.MakeExpression(ExpressionType.Or, children);
            }
            return children.Single();
        }

        /// <summary>
        /// Construct and validate a Not expression.
        /// </summary>
        /// <param name="child">Child clauses.</param>
        /// <returns>New expression.</returns>
        public static Expression NotExpression(Expression child)
            => Expression.MakeExpression(ExpressionType.Not, child);

        /// <summary>
        /// Construct a constant expression.
        /// </summary>
        /// <param name="value">Constant value.</param>
        /// <returns>New expression.</returns>
        public static Expression ConstantExpression(object value)
            => new Constant(value);

        /// <summary>
        /// Construct and validate a property accessor.
        /// </summary>
        /// <param name="property">Property to lookup.</param>
        /// <param name="instance">Expression to get instance that contains property or null for global state.</param>
        /// <returns>New expression.</returns>
        public static Expression Accessor(string property, Expression instance = null)
            => instance == null
            ? MakeExpression(ExpressionType.Accessor, ConstantExpression(property))
            : MakeExpression(ExpressionType.Accessor, ConstantExpression(property), instance);

        /// <summary>
        /// Validate immediate expression.
        /// </summary>
        public void Validate() => Evaluator.ValidateExpression(this);

        /// <summary>
        /// Recursively validate the expression tree.
        /// </summary>
        public void ValidateTree()
        {
            Validate();
            foreach (var child in Children)
            {
                child.ValidateTree();
            }
        }

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <param name="state">
        /// Global state to evaluate accessor expressions against.  Can be <see cref="System.Collections.Generic.IDictionary{String, Object}"/>,
        /// <see cref="System.Collections.IDictionary"/> otherwise reflection is used to access property and then indexer.
        /// </param>
        /// <returns>Computed value and an error string.  If the string is non-null, then there was an evaluation error.</returns>
        public (object value, string error) TryEvaluate(object state)
            => Evaluator.TryEvaluate(this, state);

        public override string ToString()
        {
            var builder = new StringBuilder();
            var valid = false;

            // Special support for memory paths
            if (Type == ExpressionType.Accessor && Children.Length >= 1)
            {
                if (Children[0] is Constant cnst
                    && cnst.Value is string prop)
                {
                    if (Children.Length == 1)
                    {
                        valid = true;
                        builder.Append(prop);
                    }
                    else if (Children.Length == 2)
                    {
                        valid = true;
                        builder.Append(Children[1].ToString());
                        builder.Append('.');
                        builder.Append(prop);
                    }
                }
            }

            // Element support
            else if (Type == ExpressionType.Element && Children.Length == 2)
            {
                valid = true;
                builder.Append(Children[0].ToString());
                builder.Append('[');
                builder.Append(Children[1].ToString());
                builder.Append(']');
            }

            // Generic version
            if (!valid)
            {
                var infix = Type.Length > 0 && !char.IsLetter(Type[0]) && Children.Count() >= 2;
                if (!infix)
                {
                    builder.Append(Type);
                }

                builder.Append('(');
                var first = true;
                foreach (var child in Children)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (infix)
                        {
                            builder.Append(' ');
                            builder.Append(Type);
                            builder.Append(' ');
                        }
                        else
                        {
                            builder.Append(", ");
                        }
                    }

                    builder.Append(child.ToString());
                }

                builder.Append(')');
            }

            return builder.ToString();
        }
    }
}
