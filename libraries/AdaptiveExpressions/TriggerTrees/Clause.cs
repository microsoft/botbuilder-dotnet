// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.TriggerTrees
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using AdaptiveExpressions;

    /// <summary>
    /// A canonical normal form expression.
    /// </summary>
    public class Clause : Expression
    {
        private Dictionary<string, string> _anyBindings = new Dictionary<string, string>();

        // These are the ignored predicates
        private Expression _ignored;

        internal Clause()
            : base(ExpressionType.And)
        {
        }

        internal Clause(Clause fromClause)
            : base(ExpressionType.And)
        {
            Children = (Expression[])fromClause.Children.Clone();
            foreach (var pair in fromClause.AnyBindings)
            {
                AnyBindings.Add(pair.Key, pair.Value);
            }
        }

        internal Clause(Expression expression)
            : base(ExpressionType.And, expression)
        {
        }

        internal Clause(IEnumerable<Expression> children)
            : base(ExpressionType.And, children.ToArray())
        {
        }

        /// <summary>
        /// Gets or sets the anyBinding dictionary.
        /// </summary>
        /// <value>A dictionary of strings, with string keys.</value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public Dictionary<string, string> AnyBindings { get => _anyBindings; set => _anyBindings = value; }
#pragma warning restore CA2227 // Collection properties should be read only

        internal bool Subsumed { get; set; } = false;

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
        /// Returns a string that represents the current object.
        /// </summary>
        /// <param name="builder">A StringBuilder object.</param>
        /// <param name="indent">An integer represents the number of spaces at the start of a line.</param>
        public void ToString(StringBuilder builder, int indent = 0)
        {
            builder.Append(' ', indent);
            if (Subsumed)
            {
                builder.Append('*');
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
                    builder.Append(" && ");
                }

                builder.Append(child.ToString());
            }

            builder.Append(')');
            if (_ignored != null)
            {
                builder.Append(" ignored(");
                builder.Append(_ignored.ToString());
                builder.Append(')');
            }

            foreach (var binding in AnyBindings)
            {
                builder.Append($" {binding.Key}->{binding.Value}");
            }
        }

        /// <summary>
        /// Compares the current Clause with another Clause.
        /// </summary>
        /// <param name="other">The other Clause to compare.</param>
        /// <param name="comparers">A comparer, which is a dictionary of IPredicateComparer with string keys.</param>
        /// <returns>A RelationshipType value between two Clause instances.</returns>
        public RelationshipType Relationship(Clause other, Dictionary<string, IPredicateComparer> comparers)
        {
            var soFar = RelationshipType.Incomparable;
            var shorter = this;
            var shorterCount = shorter.Children.Length;
            var longer = other;
            var longerCount = longer.Children.Length;
            var swapped = false;
            if (longerCount < shorterCount)
            {
                longer = this;
                shorter = other;
                var tmp = longerCount;
                longerCount = shorterCount;
                shorterCount = tmp;
                swapped = true;
            }

            if (shorterCount == 0)
            {
                if (longerCount == 0)
                {
                    soFar = RelationshipType.Equal;
                }
                else
                {
                    soFar = RelationshipType.Generalizes;
                }
            }
            else
            {
                // If every one of shorter predicates is equal or superset of one in longer, then shorter is a superset of longer
                foreach (var shortPredicate in shorter.Children)
                {
                    var shorterRel = RelationshipType.Incomparable;
                    foreach (var longPredicate in longer.Children)
                    {
                        shorterRel = Relationship(shortPredicate, longPredicate, comparers);
                        if (shorterRel != RelationshipType.Incomparable)
                        {
                            // Found related predicates
                            break;
                        }
                    }

                    if (shorterRel == RelationshipType.Incomparable)
                    {
                        // Predicate in shorter is incomparable so done
                        soFar = RelationshipType.Incomparable;
                        break;
                    }
                    else
                    {
                        if (soFar == RelationshipType.Incomparable)
                        {
                            soFar = shorterRel;
                        }

                        if (soFar == RelationshipType.Equal)
                        {
                            if (shorterRel == RelationshipType.Generalizes
                                || (shorterRel == RelationshipType.Specializes && shorterCount == longerCount)
                                || shorterRel == RelationshipType.Equal)
                            {
                                soFar = shorterRel;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else if (soFar != shorterRel)
                        {
                            // Not continued with sub/super so incomparable
                            break;
                        }
                    }
                }

                if (shorterCount != longerCount)
                {
                    switch (soFar)
                    {
                        case RelationshipType.Equal:
                        case RelationshipType.Generalizes: soFar = RelationshipType.Generalizes; break;
                        default: soFar = RelationshipType.Incomparable; break;
                    }
                }

                soFar = BindingsRelationship(soFar, shorter, longer);
            }

            return swapped ? soFar.Swap() : soFar;
        }

        /// <summary>
        /// Determines whether the current Clause matches with another Clause.
        /// </summary>
        /// <param name="clause">The other Clause instance to compare with.</param>
        /// <param name="memory">The scope for looking up variables.</param>
        /// <returns>
        /// A boolean value indicating  whether the two Clauses are matched.
        /// Returns True if two Clauses are matched, otherwise returns False.
        /// </returns>
        public bool Matches(Clause clause, object memory)
        {
            var matched = false;
            if (clause.DeepEquals(this))
            {
                matched = true;
                if (_ignored != null)
                {
                    var (match, err) = _ignored.TryEvaluate<bool>(memory);
                    matched = err == null && match;
                }
            }

            return matched;
        }

        internal void SplitIgnores()
        {
            var children = new List<Expression>();
            var ignores = new List<Expression>();
            foreach (var child in Children)
            {
                if (child.Type == ExpressionType.Ignore)
                {
                    ignores.Add(child.Children[0]);
                }
                else
                {
                    children.Add(child);
                }

                Children = children.ToArray();
            }

            if (ignores.Any())
            {
                _ignored = Expression.AndExpression(ignores.ToArray());
            }
        }

        private RelationshipType BindingsRelationship(RelationshipType soFar, Clause shorterClause, Clause longerClause)
        {
            if (soFar == RelationshipType.Equal)
            {
                var swapped = false;
                var shorter = shorterClause.AnyBindings;
                var longer = longerClause.AnyBindings;
                if (shorterClause.AnyBindings.Count > longerClause.AnyBindings.Count)
                {
                    shorter = longerClause.AnyBindings;
                    longer = shorterClause.AnyBindings;
                    swapped = true;
                }

                foreach (var shortBinding in shorter)
                {
                    var found = false;
                    foreach (var longBinding in longer)
                    {
                        if (shortBinding.Key == longBinding.Key && shortBinding.Value == longBinding.Value)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        soFar = RelationshipType.Incomparable;
                        break;
                    }
                }

                if (soFar == RelationshipType.Equal && shorter.Count < longer.Count)
                {
                    soFar = RelationshipType.Specializes;
                }

                soFar = Swap(soFar, swapped);
            }

            return soFar;
        }

        private RelationshipType Swap(RelationshipType soFar, bool swapped)
        {
            if (swapped)
            {
                switch (soFar)
                {
                    case RelationshipType.Specializes: soFar = RelationshipType.Generalizes; break;
                    case RelationshipType.Generalizes: soFar = RelationshipType.Specializes; break;
                }
            }

            return soFar;
        }

        private RelationshipType Relationship(Expression expr, Expression other, Dictionary<string, IPredicateComparer> comparers)
        {
            var relationship = RelationshipType.Incomparable;
            var root = expr;
            var rootOther = other;
            if (expr.Type == ExpressionType.Not && other.Type == ExpressionType.Not)
            {
                root = expr.Children[0];
                rootOther = other.Children[0];
            }

            IPredicateComparer comparer = null;
            if (root.Type == other.Type)
            {
                comparers.TryGetValue(root.Type, out comparer);
            }

            if (comparer != null)
            {
                relationship = comparer.Relationship(root, rootOther);
            }
            else
            {
                relationship = expr.DeepEquals(other) ? RelationshipType.Equal : RelationshipType.Incomparable;
            }

            return relationship;
        }
    }
}
