// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Expressions.TriggerTrees
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Microsoft.Bot.Expressions;

    public class Clause : Expression
    {
        private Dictionary<string, string> anyBindings = new Dictionary<string, string>();

        // These are the ignored predicates
        private Expression ignored;

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

        public Dictionary<string, string> AnyBindings { get => anyBindings; set => anyBindings = value; }

        internal bool Subsumed { get; set; } = false;

        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }

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
            if (ignored != null)
            {
                builder.Append(" ignored(");
                builder.Append(ignored.ToString());
                builder.Append(')');
            }

            foreach (var binding in AnyBindings)
            {
                builder.Append($" {binding.Key}->{binding.Value}");
            }
        }

        public RelationshipType Relationship(Clause other, Dictionary<string, IPredicateComparer> comparers)
        {
            var soFar = RelationshipType.Incomparable;
            var shorter = this;
            var shorterCount = shorter.Children.Count();
            var longer = other;
            var longerCount = longer.Children.Count();
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

        public bool Matches(Clause clause, object memory)
        {
            var matched = false;
            if (clause.DeepEquals(this))
            {
                matched = true;
                if (ignored != null)
                {
                    var (match, err) = ignored.TryEvaluate<bool>(memory);
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
                ignored = Expression.AndExpression(ignores.ToArray());
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
