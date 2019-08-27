using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    // Each trigger is normalized to disjunctive normal form and then expanded with quantifiers.
    // Each of those clauses is then put into a DAG where the most restrictive clauses are at the bottom.
    // When matching the most specific clauses block out any more general clauses.
    // 
    // Disjunctions and quantification do not change the tree construction, but they are used in determing
    // what triggers are returned.  For example, from a strictly logical sense A&B v C&D is more general then A&B or C.  If we had these rules:
    // R1(A)
    // R2(A&B)
    // R3(A&BvC&D)
    // R4(C)
    // Then from a strictly logic viewpoint the tree should be:
    //               Root
    //     |           |       |
    // R3(A&B v C&D)   R1(A) R4(C)
    //    |                  /
    // R2(A&B)
    // The problem is that from the developer standpoint R3 is more of a shortcut for two rules, i.e.A&B and another rule for C&D.
    // In the tree above if you had C&D you would get both R3 and R4—which does not seem like what you really want.
    // Even though R3 is a disjunction, C&D is more specific than just C.
    // The fix is build the tree just based on the conjunctions and then filter triggers on a specific clause so that more specific triggers remove more general ones, i.e. disjunctions.  
    // This is what the correspoinding tree looks like:
    // Root
    //    |                                                   |
    // A: R1(A)                                           C: R4(C)
    //    |                                                    |
    // A&B: R2(A&B), R3(A&BvC&D)                        C&D: R3(A&BvC&D)
    // If you had A&B you can look at the triggers and return R2 instead of R3—that seems appropriate.
    // But, if you also had C&D at the same time you would still get R3 triggering because of C&D,  I think this is the right thing.
    // Even though R3 was filtered out of the A&B branch, it is still the most specific answer in the C&D branch. 
    // If we remove R3 all together then we would end up returning R4 instead which doesn’t seem correct from the standpoint of disjunctions being a shortcut for multiple rules.

    /// <summary>
    /// A trigger tree organizes evaluators according to generalization/specialization in order to make it easier to use rules.
    /// </summary>
    /// <remarks>
    /// A trigger expression generates true if the expression evaluated on a frame is true.
    /// The expression itself consists of arbitrary boolean functions ("predicates") combined with &amp;&amp; || !.
    /// Most predicates are expressed over the frame passed in, but they can be anything--there are even ways of optimizing or comparing them.
    /// By organizing evaluators into a tree (techinically a DAG) it becomes easier to use rules by reducing the coupling between rules.
    /// For example if a rule applies if some predicate A is true, then another rule that applies if A &amp;&amp; B are true is
    /// more specialized.  If the second expression is true, then because we know of the relationship we can ignore the first
    /// rule--even though its expression is true.  Without this kind of capability in order to add the second rule, you would
    /// have to change the first to become A &amp;&amp; !B.
    /// </remarks>
    [DebuggerDisplay("{ToString()}")]
    [DebuggerTypeProxy(typeof(Debugger))]
    public class TriggerTree
    {
        /// <summary>
        /// Mark a sub-expression as optional.
        /// </summary>
        /// <remarks>
        /// When an expression is being processed, optional creates a disjunction where the expression is both included and not
        /// included with the rest of the expression.  This is a simple way to express this common relationship.  By generating
        /// both clauses then matching the expression can be more specific when the optional expression is true.
        /// </remarks>
        public const string Optional = "optional";

        /// <summary>
        /// Any predicate expression wrapped in this will be ignored for specialization.
        /// </summary>
        /// <remarks>
        /// This is useful for when you need to add expression to the trigger that are part of rule mechanics rather than of intent.
        /// For example, if you have a counter for how often a particular message is displayed, then that is part of the triggering condition, 
        /// but all such rules would be incomparable because they counter is per-rule. 
        /// </remarks>
        public const string Ignore = "ignore";

        public List<IOptimizer> Optimizers = new List<IOptimizer>();
        public Dictionary<string, IPredicateComparer> Comparers = new Dictionary<string, IPredicateComparer>();
        public Node Root;
        public int TotalTriggers = 0;

        private static readonly IExpressionParser _parser = new ExpressionEngine(LookupFunction);

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerTree"/> class.
        /// </summary>
        public TriggerTree()
        {
            Root = new Node(new Clause(), this);
        }

        public static ExpressionEvaluator LookupFunction(string type)
        {
            ExpressionEvaluator eval;
            if (type == Optional || type == Ignore)
            {
                eval = new ExpressionEvaluator(type, null, ReturnType.Boolean, BuiltInFunctions.ValidateUnaryBoolean);
                eval.Negation = eval;
            }
            else 
            {
                eval = BuiltInFunctions.Lookup(type);
            }

            return eval;
        }

        public static Expression Parse(string expr) => _parser.Parse(expr);

        public override string ToString() => $"TriggerTree with {TotalTriggers} triggers";

        /// <summary>
        /// Add a trigger expression to the tree.
        /// </summary>
        /// <param name="expression">Trigger to add.</param>
        /// <param name="action">Action when triggered.</param>
        /// <param name="quantifiers">Quantifiers to use when expanding expressions.</param>
        /// <returns>New trigger.</returns>
        public Trigger AddTrigger(string expression, object action, params Quantifier[] quantifiers)
                    => AddTrigger(_parser.Parse(expression), action, quantifiers);

        /// <summary>
        /// Add a trigger expression to the tree.
        /// </summary>
        /// <param name="expression">Trigger to add.</param>
        /// <param name="action">Action when triggered.</param>
        /// <param name="quantifiers">Quantifiers to use when expanding expressions.</param>
        /// <returns>New trigger.</returns>
        public Trigger AddTrigger(Expression expression, object action, params Quantifier[] quantifiers)
        {
            var trigger = new Trigger(this, expression, action, quantifiers);
            var added = false;
            if (trigger.Clauses.Any())
            {
                foreach (var clause in trigger.Clauses)
                {
                    var newNode = new Node(clause, this, trigger);
                    if (Root.AddNode(newNode))
                    {
                        added = true;
                    }
                }
            }

            if (added)
            {
                ++TotalTriggers;
            }

            return trigger;
        }

        /// <summary>
        /// Remove trigger from tree.
        /// </summary>
        /// <param name="trigger">Trigger to remove.</param>
        /// <returns>True if removed trigger.</returns>
        public bool RemoveTrigger(Trigger trigger)
        {
            var result = Root.RemoveTrigger(trigger);
            if (result)
            {
                --TotalTriggers;
            }

            return result;
        }

        /// <summary>
        /// Generate a string describing the tree.
        /// </summary>
        /// <param name="indent">Current indent level.</param>
        /// <returns>string describing the tree.</returns>
        public string TreeToString(int indent = 0)
        {
            var builder = new StringBuilder();
            TreeToString(builder, Root, indent);
            return builder.ToString();
        }

        public void GenerateGraph(string outPath)
        {
            using (var output = new StreamWriter(outPath))
            {
                var visited = new HashSet<Node>();
                output.WriteLine("strict digraph TriggerTree {");
                GenerateGraph(output, Root, 0, visited);
                output.WriteLine("}");
            }
        }

        /// <summary>
        /// Return the possible matches given the current state.
        /// </summary>
        /// <param name="state">State to evaluate against.</param>
        /// <returns>Enumeration of possible matches.</returns>
        public IEnumerable<Node> Matches(object state) => Root.Matches(state);

        /// <summary>
        /// Verify the tree meets speicalization/generalization invariants. 
        /// </summary>
        /// <returns>Bad node if found.</returns>
        public Node VerifyTree() => VerifyTree(Root, new HashSet<Node>());

        private void TreeToString(StringBuilder builder, Node node, int indent)
        {
            node.ToString(builder, indent);
            builder.Append($" [{node.Triggers.Count}]");
            builder.AppendLine();
            foreach (var child in node.Specializations)
            {
                TreeToString(builder, child, indent + 2);
            }
        }

        private string NameNode(Node node) => '"' + node.ToString().Replace("\"", "\\\"") + '"';

        private void GenerateGraph(StreamWriter output, Node node, int indent, HashSet<Node> visited)
        {
            if (!visited.Contains(node))
            {
                visited.Add(node);
                output.Write($"{string.Empty.PadLeft(indent)}{NameNode(node)}");
                var spaces = string.Empty.PadLeft(indent + 2);
                var first = true;
                foreach (var child in node.Specializations)
                {
                    if (first)
                    {
                        output.WriteLine(" -> {");
                        first = false;
                    }

                    output.WriteLine($"{spaces}{NameNode(child)}");
                }

                if (!first)
                {
                    output.WriteLine($"{string.Empty.PadLeft(indent)}}}");
                }

                foreach (var child in node.Specializations)
                {
                    GenerateGraph(output, child, indent + 2, visited);
                }
            }
        }

        private Node VerifyTree(Node node, HashSet<Node> visited)
        {
            Node badNode = null;
            if (!visited.Contains(node))
            {
                visited.Add(node);
                for (var i = 0; badNode == null && i < node.Specializations.Count; ++i)
                {
                    var first = node.Specializations[i];
                    if (node.Relationship(first) != RelationshipType.Generalizes)
                    {
                        badNode = node;
                    }
                    else
                    {
                        VerifyTree(node.Specializations[i], visited);
                        for (var j = i + 1; j < node.Specializations.Count; ++j)
                        {
                            var second = node.Specializations[j];
                            if (first.Relationship(second) != RelationshipType.Incomparable)
                            {
                                badNode = node;
                                break;
                            }
                        }
                    }
                }
            }

            return badNode;
        }

        private class Debugger
        {
            public string TreeString;
            public List<IOptimizer> _optimizers;
            public Dictionary<string, IPredicateComparer> _comparers;

            public Debugger(TriggerTree triggers)
            {
                TreeString = triggers.TreeToString();
                _optimizers = triggers.Optimizers;
                _comparers = triggers.Comparers;
            }
        }
    }
}
