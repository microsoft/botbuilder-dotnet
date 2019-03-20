using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Expressions;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    /// <summary>
    /// A trigger is a combination of a trigger expression and the corresponding action.
    /// </summary>
    public class Trigger
    {
        /// <summary>
        /// Original trigger expression.
        /// </summary>
        public string OriginalExpression;

        private IParseTree _parse;
        private TriggerTree _tree;
        private IEnumerable<Quantifier> _quantifiers;
        private List<Clause> _clauses;

        /// <summary>
        /// Action to take when trigger is true.
        /// </summary>
        public object Action { get; }

        /// <summary>
        /// Expressions are converted into Disjunctive Normal Form where ! is pushed to the leaves and there is an implicit || between clauses and && within a clause. 
        /// </summary>
        public IReadOnlyList<Clause> Clauses { get { return _clauses; } }

        /// <summary>
        /// Construct a trigger expression.
        /// </summary>
        /// <param name="tree">Trigger tree that contains this trigger.</param>
        /// <param name="expression">Expression for when the trigger action is possible.</param>
        /// <param name="action">Action to take when a trigger matches.</param>
        /// <param name="quantifiers">Quantifiers to dynamically expand the expression.</param>
        internal Trigger(TriggerTree tree, string expression, object action, params Quantifier[] quantifiers)
        {
            _parse = ExpressionEngine.Parse(expression);
            _tree = tree;
            Action = action;
            OriginalExpression = expression;
            _quantifiers = quantifiers;
            if (expression != null)
            {
                var notNormalized = PushDownNot(_parse, false);
                _clauses = GenerateClauses(notNormalized).ToList();
                RemoveDuplicatedPredicates();
                OptimizeClauses();
                ExpandQuantifiers();
                RemoveDuplicates();
                MarkSubsumedClauses();
            }
            else
            {
                _clauses = new List<Clause>();
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }

        public RelationshipType Relationship(Trigger other, Dictionary<string, IPredicateComparer> comparers)
        {
            RelationshipType result;
            var first = Relationship(this, other, comparers);
            var second = Relationship(other, this, comparers);
            if (first == RelationshipType.Equal)
            {
                if (second == RelationshipType.Equal)
                {
                    // All first clauses == second clauses
                    result = RelationshipType.Equal;
                }
                else
                {
                    // All first clauses found in second
                    result = RelationshipType.Specializes;
                }
            }
            else if (first == RelationshipType.Specializes)
            {
                // All first clauses specialize or equal a second clause
                result = RelationshipType.Specializes;
            }
            else if (second == RelationshipType.Equal || second == RelationshipType.Specializes)
            {
                // All second clauses are equal or specialize a first clause
                result = RelationshipType.Generalizes;
            }
            else
            {
                // All other cases are incomparable
                result = RelationshipType.Incomparable;
            }
            return result;
        }

        private RelationshipType Relationship(Trigger trigger, Trigger other, Dictionary<string, IPredicateComparer> comparers)
        {
            var soFar = RelationshipType.Incomparable;
            foreach (var clause in trigger.Clauses)
            {
                if (!clause.Subsumed)
                {
                    // Check other for = or clause that is specialized
                    var clauseSoFar = RelationshipType.Incomparable;
                    foreach (var second in other.Clauses)
                    {
                        if (!second.Subsumed)
                        {
                            var reln = clause.Relationship(second, comparers);
                            if (reln == RelationshipType.Equal || reln == RelationshipType.Specializes)
                            {
                                clauseSoFar = reln;
                                break;
                            }
                        }
                    }
                    if (clauseSoFar == RelationshipType.Incomparable || clauseSoFar == RelationshipType.Generalizes)
                    {
                        // Some clause is not comparable
                        soFar = RelationshipType.Incomparable;
                        break;
                    }
                    else if (clauseSoFar == RelationshipType.Equal)
                    {
                        if (soFar == RelationshipType.Incomparable)
                        {
                            // Start on equal clause
                            soFar = clauseSoFar;
                        }
                    }
                    else if (clauseSoFar == RelationshipType.Specializes)
                    {
                        // Either going from incomparable or equal to specializes
                        soFar = clauseSoFar;
                    }
                }
            }
            // Either incomparable, equal or specializes
            return soFar;
        }

        protected void ToString(StringBuilder builder, int indent = 0)
        {
            builder.Append(' ', indent);
            if (_clauses.Any())
            {
                bool first = true;
                foreach (var clause in _clauses)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.AppendLine();
                        builder.Append(' ', indent);
                        builder.Append("|| ");
                    }
                    builder.Append(clause.ToString());
                }
            }
            else
            {
                builder.Append("<Empty>");
            }
        }

        // Push not down to leaves using De Morgan's rule
        private Expression PushDownNot(IParseTree expression, bool inNot)
        {
            var e = new Parse
            Expression newExpr = expression;
            var unary = expression as UnaryExpression;
            var binary = expression as BinaryExpression;
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                    {
                        if (inNot)
                        {
                            newExpr = System.Linq.Expressions.Expression.OrElse(PushDownNot(binary.Left, true), PushDownNot(binary.Right, true));
                        }
                        else
                        {
                            newExpr = System.Linq.Expressions.Expression.AndAlso(PushDownNot(binary.Left, false), PushDownNot(binary.Right, false));
                        }
                    }
                    break;
                case ExpressionType.OrElse:
                    {
                        if (inNot)
                        {
                            newExpr = System.Linq.Expressions.Expression.AndAlso(PushDownNot(binary.Left, true), PushDownNot(binary.Right, true));
                        }
                        else
                        {
                            newExpr = System.Linq.Expressions.Expression.OrElse(PushDownNot(binary.Left, false), PushDownNot(binary.Right, false));
                        }
                    }
                    break;
                case ExpressionType.Not:
                    newExpr = PushDownNot(((UnaryExpression)expression).Operand, !inNot);
                    break;
                // Rewrite comparison operators
                case ExpressionType.LessThan:
                    if (inNot)
                    {
                        newExpr = Expression.GreaterThanOrEqual(binary.Left, binary.Right);
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (inNot)
                    {
                        newExpr = Expression.GreaterThan(binary.Left, binary.Right);
                    }
                    break;
                case ExpressionType.Equal:
                    if (inNot)
                    {
                        newExpr = Expression.NotEqual(binary.Left, binary.Right);
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (inNot)
                    {
                        newExpr = Expression.LessThan(binary.Left, binary.Right);
                    }
                    break;
                case ExpressionType.GreaterThan:
                    if (inNot)
                    {
                        newExpr = Expression.LessThanOrEqual(binary.Left, binary.Right);
                    }
                    break;
                case ExpressionType.Call:
                    {
                        var special = TriggerTree.GetOptional(expression) ?? TriggerTree.GetIgnore(expression);
                        if (special != null)
                        {
                            // Pass not through optional/ignore
                            newExpr = Expression.Call(special.Method, PushDownNot(special.Arguments[0], inNot));
                        }
                        else
                        {
                            if (inNot)
                            {
                                newExpr = Expression.Not(expression);
                            }
                        }
                    }
                    break;
                default:
                    if (inNot)
                    {
                        newExpr = Expression.Not(expression);
                    }
                    break;
            }
            return newExpr;
        }

        private IEnumerable<Expression> OrLeaves(Expression expression)
        {
            if (expression.NodeType == ExpressionType.OrElse)
            {
                var or = (BinaryExpression)expression;
                foreach (var leaf in OrLeaves(or.Left))
                {
                    yield return leaf;
                }
                foreach (var leaf in OrLeaves(or.Right))
                {
                    yield return leaf;
                }
            }
            else
            {
                yield return expression;
            }
        }

        private IEnumerable<Clause> GenerateClauses(Expression expression)
        {
            var binary = expression as BinaryExpression;
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                    {
                        var rightClauses = GenerateClauses(binary.Right);
                        foreach (var left in GenerateClauses(binary.Left))
                        {
                            foreach (var right in rightClauses)
                            {
                                var clause = new Clause();
                                clause.Predicates.AddRange(left.Predicates);
                                clause.Predicates.AddRange(right.Predicates);
                                yield return clause;
                            }
                        }
                    }
                    break;
                case ExpressionType.OrElse:
                    {
                        foreach (var left in GenerateClauses(binary.Left))
                        {
                            yield return left;
                        }
                        foreach (var right in GenerateClauses(binary.Right))
                        {
                            yield return right;
                        }
                    }
                    break;
                case ExpressionType.Call:
                    {
                        var optional = TriggerTree.GetOptional(expression);
                        if (optional != null)
                        {
                            yield return new Clause();
                            foreach (var clause in GenerateClauses(optional.Arguments[0]))
                            {
                                yield return clause;
                            }
                        }
                        else
                        {
                            yield return new Clause(expression);
                        }
                    }
                    break;
                default:
                    yield return new Clause(expression);
                    break;
            }
        }

        // Remove any duplicate predicates within a clause
        // NOTE: This is annoying but expression hash codes of DeepEquals expressions are different
        private void RemoveDuplicatedPredicates()
        {
            // Rewrite clauses to remove duplicated tests
            for (var i = 0; i < _clauses.Count(); ++i)
            {
                var clause = _clauses[i];
                var newClause = new Clause();
                for (var p = 0; p < clause.Predicates.Count(); ++p)
                {
                    var pred = clause.Predicates[p];
                    var found = false;
                    for (var q = p + 1; q < clause.Predicates.Count(); ++q)
                    {
                        if (pred.DeepEquals(clause.Predicates[q]))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        newClause.Predicates.Add(pred);
                    }
                }
                _clauses[i] = newClause;
            }
        }

        // Mark clauses that are more specific than another clause as subsumed and also remove any = clauses.
        private void MarkSubsumedClauses()
        {
            for (var i = 0; i < _clauses.Count(); ++i)
            {
                var clause = _clauses[i];
                if (!clause.Subsumed)
                {
                    for (var j = i + 1; j < _clauses.Count(); ++j)
                    {
                        var other = _clauses[j];
                        if (!other.Subsumed)
                        {
                            var reln = clause.Relationship(other, _tree.Comparers);
                            if (reln == RelationshipType.Equal)
                            {
                                _clauses.RemoveAt(j);
                            }
                            else
                            {
                                if (reln == RelationshipType.Specializes)
                                {
                                    clause.Subsumed = true;
                                    break;
                                }
                                else if (reln == RelationshipType.Generalizes)
                                {
                                    other.Subsumed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OptimizeClauses()
        {
            foreach (var clause in _clauses)
            {
                foreach (var optimizer in _tree.Optimizers)
                {
                    optimizer.Optimize(clause);
                }
            }
        }

        private void ExpandQuantifiers()
        {
            if (_quantifiers != null && _quantifiers.Any())
            {
                foreach (var quantifier in _quantifiers)
                {
                    var newClauses = new List<Clause>();
                    foreach (var clause in _clauses)
                    {
                        newClauses.AddRange(ExpandQuantifier(quantifier, clause));
                    }
                    _clauses = newClauses;
                }
            }
        }

        private Expression SubstituteVariable(string variable, string binding, Expression expression, ref bool changed)
        {
            var newExpr = expression;
            var unary = expression as UnaryExpression;
            var binary = expression as BinaryExpression;
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    {
                        var call = (MethodCallExpression)expression;
                        var args = new List<Expression>();
                        foreach (var arg in call.Arguments)
                        {
                            args.Add(SubstituteVariable(variable, binding, arg, ref changed));
                        }
                        newExpr = System.Linq.Expressions.Expression.Call(call.Object, call.Method, args);
                    }
                    break;
                case ExpressionType.Not:
                    newExpr = System.Linq.Expressions.Expression.Not(SubstituteVariable(variable, binding, unary.Operand, ref changed));
                    break;
                case ExpressionType.LessThan:
                    newExpr = System.Linq.Expressions.Expression.LessThan(
                        SubstituteVariable(variable, binding, binary.Left, ref changed),
                        SubstituteVariable(variable, binding, binary.Right, ref changed));
                    break;
                case ExpressionType.LessThanOrEqual:
                    newExpr = System.Linq.Expressions.Expression.LessThanOrEqual(
                        SubstituteVariable(variable, binding, binary.Left, ref changed),
                        SubstituteVariable(variable, binding, binary.Right, ref changed));
                    break;
                case ExpressionType.Equal:
                    newExpr = System.Linq.Expressions.Expression.Equal(
                        SubstituteVariable(variable, binding, binary.Left, ref changed),
                        SubstituteVariable(variable, binding, binary.Right, ref changed));
                    break;
                case ExpressionType.NotEqual:
                    newExpr = System.Linq.Expressions.Expression.NotEqual(
                        SubstituteVariable(variable, binding, binary.Left, ref changed),
                        SubstituteVariable(variable, binding, binary.Right, ref changed));
                    break;
                case ExpressionType.GreaterThan:
                    newExpr = System.Linq.Expressions.Expression.GreaterThan(
                        SubstituteVariable(variable, binding, binary.Left, ref changed),
                        SubstituteVariable(variable, binding, binary.Right, ref changed));
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    newExpr = System.Linq.Expressions.Expression.GreaterThanOrEqual(
                        SubstituteVariable(variable, binding, binary.Left, ref changed),
                        SubstituteVariable(variable, binding, binary.Right, ref changed));
                    break;
                case ExpressionType.Constant:
                    {
                        var constant = (ConstantExpression)expression;
                        if (constant.Type == typeof(string) && (string)constant.Value == variable)
                        {
                            newExpr = System.Linq.Expressions.Expression.Constant(binding);
                            changed = true;
                        }
                    }
                    break;
                case ExpressionType.Convert:
                    newExpr = System.Linq.Expressions.Expression.Convert(SubstituteVariable(variable, binding, unary.Operand, ref changed), unary.Type);
                    break;
                case ExpressionType.ConvertChecked:
                    newExpr = System.Linq.Expressions.Expression.ConvertChecked(SubstituteVariable(variable, binding, unary.Operand, ref changed), unary.Type);
                    break;
                default:
                    break;
            }
            return newExpr;
        }

        private IEnumerable<Clause> ExpandQuantifier(Quantifier quantifier, Clause clause)
        {
            if (quantifier.Type == QuantifierType.All)
            {
                var newClause = new Clause(clause);
                if (quantifier.Bindings.Any())
                {
                    foreach (var predicate in clause.Predicates)
                    {
                        foreach (var binding in quantifier.Bindings)
                        {
                            var changed = false;
                            var newPredicate = SubstituteVariable(quantifier.Variable, binding, predicate, ref changed);
                            newClause.Predicates.Add(newPredicate);
                            if (!changed)
                            {
                                // No change to first predicate, so can stop
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Empty quantifier is trivially true so remove any predicate that refers to quantifier
                    foreach (var predicate in clause.Predicates)
                    {
                        var changed = false;
                        SubstituteVariable(quantifier.Variable, string.Empty, predicate, ref changed);
                        if (!changed)
                        {
                            newClause.Predicates.Add(predicate);
                        }
                    }
                }
                yield return newClause;
            }
            else
            {
                if (quantifier.Bindings.Any())
                {
                    var changed = false;
                    foreach (var binding in quantifier.Bindings)
                    {
                        var newClause = new Clause(clause);
                        foreach (var predicate in clause.Predicates)
                        {
                            var newPredicate = SubstituteVariable(quantifier.Variable, binding, predicate, ref changed);
                            newClause.Predicates.Add(newPredicate);
                        }
                        if (changed)
                        {
                            newClause.AnyBindings.Add(quantifier.Variable, binding);
                        }
                        yield return newClause;
                        if (!changed)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // Keep clause if does not contain any binding
                    var changed = false;
                    foreach (var predicate in clause.Predicates)
                    {
                        SubstituteVariable(quantifier.Variable, string.Empty, predicate, ref changed);
                        if (changed)
                        {
                            break;
                        }
                    }
                    if (!changed)
                    {
                        yield return clause;
                    }
                }
            }
        }

        private void RemoveDuplicates()
        {
            foreach (var clause in _clauses)
            {
                // NOTE: This is quadratic in clause length but GetHashCode is not equal for expressions and we expect the number of clauses to be small.
                var predicates = clause.Predicates;
                for (var i = 0; i < predicates.Count; ++i)
                {
                    var first = predicates[i];
                    for (var j = i + 1; j < predicates.Count;)
                    {
                        var second = predicates[j];
                        if (first.DeepEquals(second))
                        {
                            predicates.RemoveAt(j);
                        }
                        else
                        {
                            ++j;
                        }
                    }
                }
            }
        }
    }
}
