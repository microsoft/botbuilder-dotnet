namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Bot.Builder.Expressions;

    public class Clause : Expression
    {
        internal bool Subsumed = false;

        private Dictionary<string, string> anyBindings = new Dictionary<string, string>();

        internal Clause()
            : base(ExpressionType.And)
        {
        }

        internal Clause(Clause fromClause)
            : base(ExpressionType.And)
        {
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
            foreach (var binding in AnyBindings)
            {
                builder.Append($" {binding.Key}->{binding.Value}");
            }
        }

        public RelationshipType Relationship(Clause other, Dictionary<string, IPredicateComparer> comparers)
        {
            var soFar = RelationshipType.Incomparable;
            var shorter = this;
            var shorterCount = shorter.PredicateCount();
            var longer = other;
            var longerCount = longer.PredicateCount();
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
                // If every one of shorter predicates is equal or superset of one in longer, then shoter is a superset of longer
                foreach (var shortPredicate in shorter.Children)
                {
                    var shorterRel = RelationshipType.Incomparable;
                    if (shortPredicate.Type != TriggerTree.Ignore)
                    {
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

            soFar = IgnoreRelationship(soFar, shorter, longer);
            return swapped ? soFar.Swap() : soFar;
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

        private RelationshipType IgnoreRelationship(RelationshipType soFar, Clause shorterClause, Clause longerClause)
        {
            if (soFar == RelationshipType.Equal)
            {
                var shortIgnores = shorterClause.Children.Where(p => p.Type == TriggerTree.Ignore);
                var longIgnores = longerClause.Children.Where(p => p.Type == TriggerTree.Ignore);
                var swapped = false;
                if (longIgnores.Count() < shortIgnores.Count())
                {
                    var old = longIgnores;
                    longIgnores = shortIgnores;
                    shortIgnores = old;
                    swapped = true;
                }

                foreach (var shortPredicate in shortIgnores)
                {
                    var found = false;
                    foreach (var longPredicate in longIgnores)
                    {
                        if (shortPredicate.DeepEquals(longPredicate))
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

                if (soFar == RelationshipType.Equal)
                {
                    if (shorterClause.Children.Count() == 0 && longerClause.Children.Count() > 0)
                    {
                        soFar = RelationshipType.Generalizes;
                    }
                    else if (shortIgnores.Count() < longIgnores.Count())
                    {
                        soFar = RelationshipType.Incomparable;
                    }
                }

                soFar = Swap(soFar, swapped);
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

        private int PredicateCount() => Children.Count(e => e.Type != TriggerTree.Ignore);
    }
}
