// This will trace the whole process, but will generate a lot of output
// #define TraceTree

// This adds a counter to each comparison when building the tree so that you can find it in the trace.
// There is a node static count and boolean ShowTrace that can be turned on/off if needed.
// #define Count

// This will verify the tree as it is built by checking invariants
// #define VerifyTree

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    /// <summary>
    /// Node in a trigger tree.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// All of the most specific triggers that contain the <see cref="Clause"/> in this node.
        /// </summary>
        public IReadOnlyList<Trigger> Triggers => _triggers;

        /// <summary>
        /// All triggers that contain the <see cref="Clause"/> in this node. 
        /// </summary>
        /// <remarks>
        /// Triggers only contain the most specific trigger, so if this node 
        /// as Pred(A) and there was a rule R1: Pred(A) -> A1 and R2: Pred(A) v Pred(B) -> A2
        /// then the second trigger would be in AllTriggers, but not Triggers because it 
        /// is more general.
        /// </remarks>
        public IReadOnlyList<Trigger> AllTriggers => _allTriggers;

        /// <summary>
        /// Specialized children of this node.
        /// </summary>
        public IReadOnlyList<Node> Specializations => _specializations;

        /// <summary>
        /// The logical conjunction this node represents.
        /// </summary>
        public Clause Clause { get; }

        /// <summary>
        /// Expression to evaluate for node.
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// The tree this node is found in.
        /// </summary>
        public TriggerTree Tree { get; }

        private List<Trigger> _allTriggers = new List<Trigger>();
        private List<Trigger> _triggers = new List<Trigger>();
        private List<Node> _specializations = new List<Node>();

#if Count
        private static int _count = 0;
#endif

#if TraceTree
        public static bool ShowTrace = true;
#endif

        internal Node(Clause clause, TriggerTree tree, Trigger trigger = null)
        {
            // In order to debug:
            // 1) Enable Count and VerifyTree
            // 2) Run your scenario
            // 3) You will most likely get a beak on the error.
            // 4) Enable TraceTree and set it hear to get the trace before count
            // Node._count has the global count for breakpointd
            // ShowTrace = _count > 280000;
            Clause = clause;
            Tree = tree;
            if (trigger != null)
            {
                _allTriggers.Add(trigger);
                _triggers.Add(trigger);
                if (Clause != null)
                {
                    var children = new List<Expression>();
                    foreach (var child in Clause.Children)
                    {
                        var predicate = child;
                        if (predicate.Type == TriggerTree.Ignore)
                        {
                            predicate = child.Children[0];
                        }
                        children.Add(predicate);
                    }
                    if (children.Any())
                    {
                        Expression = Expression.MakeExpression(ExpressionType.And, null, children.ToArray());
                    }
                }
            }
            if (Expression == null)
            {
                Expression = Expression.ConstantExpression(true);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }

        public void ToString(StringBuilder builder, int indent = 0)
            => Clause.ToString(builder, indent);

        /// <summary>
        /// Return the most specific matches below this node.
        /// </summary>
        /// <param name="state">Frame to evaluate against.</param>
        /// <returns>List of the most specific matches found.</returns>
        internal IReadOnlyList<Node> Matches(object state)
        {
            var matches = new List<Node>();
            Matches(state, matches, new Dictionary<Node, bool>());
            return matches;
        }

        /// <summary>
        /// Identify the relationship between two nodes.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Relationship between this node and the other.</returns>
        public RelationshipType Relationship(Node other)
            => Clause.Relationship(other.Clause, Tree.Comparers);

        private enum Operation { None, Found, Added, Removed, Inserted };

#pragma warning disable IDE0022
        internal bool AddNode(Node triggerNode)
        {
#if TraceTree
            if (Node.ShowTrace)
            {
                Debug.WriteLine("");
                Debug.WriteLine($"***** Add Trigger {triggerNode.Triggers.First().OriginalExpression} *****");
                Debug.IndentSize = 2;
            }
#endif
            return AddNode(triggerNode, new Dictionary<Node, Operation>()) == Operation.Added;
        }
#pragma warning restore IDE0022

        internal bool RemoveTrigger(Trigger trigger)
        {
            var removed = new Dictionary<Node, Operation>();
            return RemoveTrigger(trigger, null, removed) == Operation.Removed;
        }

        // In order to add a trigger we have to walk over the whole tree
        // If I am adding B and encounter A, A could have a specialization of A & B without B being present.
        private Operation AddNode(Node triggerNode, Dictionary<Node, Operation> ops)
        {
            var op = Operation.None;
            if (!ops.TryGetValue(this, out op))
            {
                var trigger = triggerNode.Triggers.First();
                var relationship = Relationship(triggerNode);
#if TraceTree
                if (Node.ShowTrace)
                {
                    Debug.WriteLine("");
#if Count
                    Debug.Write($"{_count}:");
#endif
                    Debug.WriteLine(this);
                    Debug.WriteLine($"{relationship}");
                    Debug.WriteLine(triggerNode);
                }
#endif
#if Count
                ++_count;
#endif
                switch (relationship)
                {
                    case RelationshipType.Equal:
                        {
                            // Ensure action is not already there
                            var found = false;
                            foreach (var existing in _allTriggers)
                            {
                                if (trigger.Action != null && trigger.Action.Equals(existing.Action))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            op = Operation.Found;
                            if (!found)
                            {
                                _allTriggers.Add(trigger);
                                var add = true;
                                for (var i = 0; i < _triggers.Count();)
                                {
                                    var existing = _triggers[i];
                                    var reln = trigger.Relationship(existing, Tree.Comparers);
                                    if (reln == RelationshipType.Generalizes)
                                    {
#if TraceTree
                                        if (Node.ShowTrace) Debug.WriteLine($"Trigger specialized by {existing}");
#endif
                                        add = false;
                                        break;
                                    }
                                    else if (reln == RelationshipType.Specializes)
                                    {
#if TraceTree
                                        if (Node.ShowTrace) Debug.WriteLine($"Trigger replaces {existing}");
#endif
                                        _triggers.RemoveAt(i);
                                    }
                                    else
                                    {
                                        ++i;
                                    }
                                }
                                if (add)
                                {
#if TraceTree
                                    if (Node.ShowTrace) Debug.WriteLine("Add trigger");
#endif
                                    _triggers.Add(trigger);
                                }
#if DEBUG
                                Debug.Assert(CheckInvariants());
#endif
                                op = Operation.Added;
                            }
                        }
                        break;
                    case RelationshipType.Incomparable:
                        {
                            foreach (var child in _specializations)
                            {
                                child.AddNode(triggerNode, ops);
                            }
                        }
                        break;
                    case RelationshipType.Specializes:
                        {
                            triggerNode.AddSpecialization(this);
                            op = Operation.Inserted;
                        }
                        break;
                    case RelationshipType.Generalizes:
                        {
                            var foundOne = false;
                            List<Node> removals = null;
#if TraceTree
                            if (Node.ShowTrace) ++Debug.IndentLevel;
#endif
                            foreach (var child in _specializations)
                            {
                                var childOp = child.AddNode(triggerNode, ops);
                                if (childOp != Operation.None)
                                {
                                    foundOne = true;
                                    if (childOp == Operation.Inserted)
                                    {
                                        if (removals == null)
                                        {
                                            removals = new List<Node>();
                                        }
                                        removals.Add(child);
                                        op = Operation.Added;
                                    }
                                    else
                                    {
                                        op = childOp;
                                    }
                                }
                            }
#if TraceTree
                            if (Node.ShowTrace) --Debug.IndentLevel;
#endif
                            if (removals != null)
                            {
                                foreach (var child in removals)
                                {
                                    _specializations.Remove(child);
                                }
#if TraceTree
                                if (Node.ShowTrace)
                                {
                                    Debug.WriteLine("Generalized");
                                    foreach (var removal in removals)
                                    {
                                        Debug.WriteLine(removal);
                                    }
                                    Debug.WriteLine($"in {this}");
                                }
#endif
                                _specializations.Add(triggerNode);
#if DEBUG
                                Debug.Assert(CheckInvariants());
#endif
                            }
                            if (!foundOne)
                            {
                                _specializations.Add(triggerNode);
#if DEBUG
                                Debug.Assert(CheckInvariants());
#endif
                                op = Operation.Added;
                            }
                        }
                        break;
                }
                // Prevent visiting this node again
                ops[this] = op;
            }
            return op;
        }

#if DEBUG
#pragma warning disable IDE0022
        private bool CheckInvariants()
        {
#if VerifyTree
            foreach (var child in _specializations)
            {
                var reln = Relationship(child);
                Debug.Assert(reln == RelationshipType.Generalizes);
            }

            // Siblings should be incomparable
            for (var i = 0; i < _specializations.Count; ++i)
            {
                var first = _specializations[i];
                for (var j = i + 1; j < _specializations.Count; ++j)
                {
                    var second = _specializations[j];
                    var reln = first.Relationship(second);
                    Debug.Assert(reln == RelationshipType.Incomparable);
                }
            }

            // Triggers should be incomparable
            for (var i = 0; i < _triggers.Count(); ++i)
            {
                for (var j = i + 1; j < _triggers.Count(); ++j)
                {
                    var reln = _triggers[i].Relationship(_triggers[j], Tree.Comparers);
                    if (reln == RelationshipType.Specializes || reln == RelationshipType.Generalizes)
                    {
                        Debug.Assert(false, $"{this} triggers overlap");
                        break;
                    }
                }
            }

            // All triggers should all be found in triggers
            for (var i = 0; i < _allTriggers.Count(); ++i)
            {
                var allTrigger = _allTriggers[i];
                var found = false;
                for (var j = 0; j < _triggers.Count(); ++j)
                {
                    var trigger = _triggers[j];
                    var reln = allTrigger.Relationship(trigger, Tree.Comparers);
                    if (allTrigger == trigger || reln == RelationshipType.Generalizes)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.Assert(false, $"{this} missing all trigger {allTrigger}");
                }
            }
#endif
            return true;
        }
#pragma warning restore IDE0022
#endif

        private bool AddSpecialization(Node specialization)
        {
            var added = false;
            List<Node> removals = null;
            var skip = false;
            foreach (var child in _specializations)
            {
                var reln = specialization.Relationship(child);
                if (reln == RelationshipType.Equal)
                {
                    skip = true;
#if TraceTree
                    if (Node.ShowTrace) Debug.WriteLine($"Already has child");
#endif
                    break;
                }
                if (reln == RelationshipType.Generalizes)
                {
                    if (removals == null) removals = new List<Node>();
                    removals.Add(child);
                }
                else if (reln == RelationshipType.Specializes)
                {
#if TraceTree
                    if (Node.ShowTrace) Debug.WriteLine($"Specialized by {child}");
#endif
                    skip = true;
                    break;
                }
            }
            if (!skip)
            {
                if (removals != null)
                {
                    foreach (var removal in removals)
                    {
                        // Don't need to add back because specialization already has them

                        _specializations.Remove(removal);
#if TraceTree
                        if (Node.ShowTrace)
                        {
                            Debug.WriteLine($"Replaced {removal}");
                            ++Debug.IndentLevel;
                        }
#endif
                        specialization.AddSpecialization(removal);
#if TraceTree
                        if (Node.ShowTrace) --Debug.IndentLevel;
#endif
                    }
                }
                _specializations.Add(specialization);
                added = true;
#if TraceTree
                if (Node.ShowTrace) Debug.WriteLine("Added as specialization");

#endif
#if DEBUG
                Debug.Assert(CheckInvariants());
#endif
            }
            return added;
        }

        // TODO: Implement remove and drop parent
        // Need to make sure we update triggers from allTriggers as well.
        private Operation RemoveTrigger(Trigger trigger, Node parent, Dictionary<Node, Operation> removed)
        {
            var op = Operation.None;
            /*
            if (!removed.TryGetValue(this, out op))
            {
                var relationship = trigger.Relationship(_triggers.First());
#if TraceTree
                Debug.WriteLine(trigger.ToString());
                Debug.WriteLine($"Remove {relationship}");
                Debug.WriteLine(ToString());
                Debug.WriteLine("");
#endif
                switch (relationship)
                {
                    case RelationshipType.Equal:
                        {
                            if (_triggers.Remove(trigger))
                            {
                                op = Operation.Removed;
                            }
                            else
                            {
                                op = Operation.Found;
                            }
                            if (parent != null && _triggers.Count == 0)
                            {
                                parent._specializations.AddRange(_specializations);
                            }
                        }
                        break;
                    case RelationshipType.Specializes:
                        {
                            foreach (var child in new List<Node>(_specializations))
                            {
                                op = child.RemoveTrigger(trigger, this, removed);
                                if (op != Operation.None)
                                {
                                    break;
                                }
                            }
                        }
                        break;
                }
                removed[this] = op;
            }
            */
            return op;
        }

        private bool Matches(object state, List<Node> matches, Dictionary<Node, bool> matched)
        {
            if (!matched.TryGetValue(this, out var found))
            {
                found = false;
                foreach (var child in _specializations)
                {
                    if (child.Matches(state, matches, matched))
                    {
                        found = true;
                    }
                }
                // No child matched so we might
                if (!found)
                {
                    var (value, error) = Expression.TryEvaluate(state);
                    if (error == null && value is bool match && match && Triggers.Any())
                    {
                        matches.Add(this);
                        found = true;
                    }
                }
                matched.Add(this, found);
            }
            return found;
        }
    }
}
