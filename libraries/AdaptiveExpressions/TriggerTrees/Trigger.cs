// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdaptiveExpressions.TriggerTrees
{
    /// <summary>
    /// A trigger is a combination of a trigger expression and the corresponding action.
    /// </summary>
    public class Trigger
    {
        private readonly IEnumerable<Quantifier> _quantifiers;

        private readonly TriggerTree _tree;
        private List<Clause> _clauses;

        /// <summary>
        /// Initializes a new instance of the <see cref="Trigger"/> class.
        /// Construct a trigger expression.
        /// </summary>
        /// <param name="tree">Trigger tree that contains this trigger.</param>
        /// <param name="expression">Expression for when the trigger action is possible.</param>
        /// <param name="action">Action to take when a trigger matches.</param>
        /// <param name="quantifiers">Quantifiers to dynamically expand the expression.</param>
        internal Trigger(TriggerTree tree, Expression expression, object action, params Quantifier[] quantifiers)
        {
            _tree = tree;
            Action = action;
            OriginalExpression = expression;
            _quantifiers = quantifiers;
            if (expression != null)
            {
                var normalForm = expression.PushDownNot();
                _clauses = GenerateClauses(normalForm).ToList();
                RemoveDuplicatedPredicates();
                OptimizeClauses();
                ExpandQuantifiers();
                RemoveDuplicates();
                MarkSubsumedClauses();
                SplitIgnores();
            }
            else
            {
                _clauses = new List<Clause>();
            }
        }

        /// <summary>
        /// Gets the original trigger expression.
        /// </summary>
        /// <value>
        /// Original trigger expression.
        /// </value>
        public Expression OriginalExpression { get; }

        /// <summary>
        /// Gets action to take when trigger is true.
        /// </summary>
        /// <value>
        /// Action to take when trigger is true.
        /// </value>
        public object Action { get; }

        /// <summary>
        /// Gets list of expressions converted into Disjunctive Normal Form where ! is pushed to the leaves and 
        /// there is an implicit || between clauses and &amp;&amp; within a clause. 
        /// </summary>
        /// <value>
        /// List of expressions converted into Disjunctive Normal Form where ! is pushed to the leaves and 
        /// there is an implicit || between clauses and &amp;&amp; within a clause. 
        /// </value>
        public IReadOnlyList<Clause> Clauses => _clauses;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }

        /// <summary>
        /// Determines the relationship between current instance and another Trigger instance.
        /// </summary>
        /// <param name="other">The other Trigger instance.</param>
        /// <param name="comparers">The comparer dictionary.</param>
        /// <returns>A RelationshipType value.</returns>
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

        /// <summary>
        /// Determines whether there is a member in the current Clause that matches the nodeClause parameter.
        /// </summary>
        /// <param name="nodeClause">The other Clause instance to match.</param>
        /// <param name="state">The scope for looking up variables.</param>
        /// <returns>
        /// A boolean value indicating  whether there is a member matches.
        /// Returns True if such member exists, otherwise returns False.
        /// </returns>
        public bool Matches(Clause nodeClause, object state)
        {
            var found = false;
            foreach (var clause in Clauses)
            {
                if (clause.Matches(nodeClause, state))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <param name="builder">A StringBuilder object.</param>
        /// <param name="indent">
        /// An integer represents the number of spaces at the start of a line. 
        /// </param>
        protected void ToString(StringBuilder builder, int indent = 0)
        {
            builder.Append(' ', indent);
            if (_clauses.Any())
            {
                var first = true;
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

                    builder.Append(clause);
                }
            }
            else
            {
                builder.Append("<Empty>");
            }
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

                    if (clauseSoFar == RelationshipType.Equal)
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

        private IEnumerable<Clause> GenerateClauses(Expression expression)
        {
            switch (expression.Type)
            {
                case ExpressionType.And:
                    // Need to combine every combination of clauses
                    var soFar = new List<Clause>();
                    var first = true;
                    foreach (var child in expression.Children)
                    {
                        var clauses = GenerateClauses(child);
                        if (!clauses.Any())
                        {
                            // Encountered false
                            soFar.Clear();
                            break;
                        }

                        if (first)
                        {
                            soFar.AddRange(clauses);
                            first = false;
                        }
                        else
                        {
                            var newClauses = new List<Clause>();
                            foreach (var old in soFar)
                            {
                                foreach (var clause in clauses)
                                {
                                    var children = new List<Expression>();
                                    children.AddRange(old.Children);
                                    children.AddRange(clause.Children);
                                    newClauses.Add(new Clause(children));
                                }
                            }

                            soFar = newClauses;
                        }
                    }

                    foreach (var clause in soFar)
                    {
                        yield return clause;
                    }

                    break;

                case ExpressionType.Or:
                    foreach (var child in expression.Children)
                    {
                        foreach (var clause in GenerateClauses(child))
                        {
                            yield return clause;
                        }
                    }

                    break;

                case ExpressionType.Optional:
                    yield return new Clause();
                    foreach (var clause in GenerateClauses(expression.Children[0]))
                    {
                        yield return clause;
                    }

                    break;

                default:
                    // True becomes empty expression and false drops clause
                    if (expression is Constant cnst && cnst.Value is bool val)
                    {
                        if (val)
                        {
                            yield return new Clause();
                        }
                    }
                    else
                    {
                        yield return new Clause(expression);
                    }

                    break;
            }
        }

        // Remove any duplicate predicates within a clause
        // NOTE: This is annoying but expression hash codes of DeepEquals expressions are different
        private void RemoveDuplicatedPredicates()
        {
            // Rewrite clauses to remove duplicated tests
            for (var i = 0; i < _clauses.Count; ++i)
            {
                var clause = _clauses[i];
                var children = new List<Expression>();
                for (var p = 0; p < clause.Children.Length; ++p)
                {
                    var pred = clause.Children[p];
                    var found = false;
                    for (var q = p + 1; q < clause.Children.Length; ++q)
                    {
                        if (pred.DeepEquals(clause.Children[q]))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        children.Add(pred);
                    }
                }

                _clauses[i] = new Clause(children);
            }
        }

        // Mark clauses that are more specific than another clause as subsumed and also remove any = clauses.
        private void MarkSubsumedClauses()
        {
            for (var i = 0; i < _clauses.Count; ++i)
            {
                var clause = _clauses[i];
                if (!clause.Subsumed)
                {
                    for (var j = i + 1; j < _clauses.Count; ++j)
                    {
                        var other = _clauses[j];
                        if (!other.Subsumed)
                        {
                            var reln = clause.Relationship(other, _tree.Comparers);
                            if (reln == RelationshipType.Equal)
                            {
                                _clauses.RemoveAt(j);
                                --j;
                            }
                            else
                            {
                                if (reln == RelationshipType.Specializes)
                                {
                                    clause.Subsumed = true;
                                    break;
                                }

                                if (reln == RelationshipType.Generalizes)
                                {
                                    other.Subsumed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SplitIgnores()
        {
            foreach (var clause in _clauses)
            {
                clause.SplitIgnores();
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

        private Expression SubstituteVariable(string variable, string binding, Expression expression, out bool changed)
        {
            var newExpr = expression;
            changed = false;
            if (expression.Type == ExpressionType.Accessor
                && expression.Children.Length == 1
                && expression.Children[0] is Constant cnst
                && cnst.Value is string str
                && str == variable)
            {
                newExpr = Expression.Accessor(binding);
                changed = true;
            }
            else
            {
                var children = new List<Expression>();
                foreach (var child in expression.Children)
                {
                    children.Add(SubstituteVariable(variable, binding, child, out var childChanged));
                    changed = changed || childChanged;
                }

                if (changed)
                {
                    newExpr = new Expression(expression.Evaluator, children.ToArray());
                }
            }

            return newExpr;
        }

        private IEnumerable<Clause> ExpandQuantifier(Quantifier quantifier, Clause clause)
        {
            if (quantifier.Type == QuantifierType.All)
            {
                var children = new List<Expression>();
                if (quantifier.Bindings.Any())
                {
                    foreach (var predicate in clause.Children)
                    {
                        foreach (var binding in quantifier.Bindings)
                        {
                            var newPredicate = SubstituteVariable(quantifier.Variable, binding, predicate, out var changed);
                            children.Add(newPredicate);
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
                    foreach (var predicate in clause.Children)
                    {
                        SubstituteVariable(quantifier.Variable, string.Empty, predicate, out var changed);
                        if (!changed)
                        {
                            children.Add(predicate);
                        }
                    }
                }

                yield return new Clause(children);
            }
            else
            {
                if (quantifier.Bindings.Any())
                {
                    var changed = false;
                    foreach (var binding in quantifier.Bindings)
                    {
                        var newClause = new Clause(clause);
                        var children = new List<Expression>();
                        foreach (var predicate in clause.Children)
                        {
                            var newPredicate = SubstituteVariable(quantifier.Variable, binding, predicate, out var predicateChanged);
                            changed = changed || predicateChanged;
                            children.Add(newPredicate);
                        }

                        if (changed)
                        {
                            newClause.AnyBindings.Add(quantifier.Variable, binding);
                        }

                        newClause.Children = children.ToArray();
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
                    foreach (var predicate in clause.Children)
                    {
                        SubstituteVariable(quantifier.Variable, string.Empty, predicate, out var predicateChanged);
                        if (predicateChanged)
                        {
                            changed = true;
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
                var predicates = new List<Expression>(clause.Children);
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

                clause.Children = predicates.ToArray();
            }
        }
    }
}
