using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    /// <summary>
    /// Relationship between trigger expressions.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// First argument specializes the second, i.e. applies to a subset of the states the second argument covers.
        /// </summary>
        Specializes,

        /// <summary>
        /// First and second argument are the same expression.
        /// </summary>
        Equal,

        /// <summary>
        /// First argument generalizes the second, i.e. applies to a superset of the states the second argument covers.
        /// </summary>
        Generalizes,

        /// <summary>
        /// Cannot tell how the first and second arguments relate.
        /// </summary>
        Incomparable
    }

    /// <summary>
    /// Optimize a clause by rewriting it.
    /// </summary>
    /// <remarks>
    /// If returned clause is null, then the expression will always be false.
    /// This is to allow things like combining simple comparisons into a range predicate.
    /// </remarks>
    public interface IOptimizer
    {
        /// <summary>
        /// Optionally rewrite a clause.
        /// </summary>
        /// <param name="clause">Original clause.</param>
        /// <returns>Optimized clause.</returns>
        Clause Optimize(Clause clause);
    }

    /// <summary>
    /// Compare two predicates to identify the relationship between them.
    /// </summary>
    public interface IPredicateComparer
    {
        /// <summary>
        /// Gets name of predicate.
        /// </summary>
        /// <value>
        /// Name of predicate.
        /// </value>
        string Predicate { get;  }

        /// <summary>
        /// Identify the relationship between two predicates.
        /// </summary>
        /// <param name="predicate">First predicate.</param>
        /// <param name="other">Second predicate.</param>
        /// <returns>Relationship beteween predicates.</returns>
        /// <remarks>
        /// This is useful for doing things like identifying that Range("size", 1, 5) is more specialized than Range("size", 1, 10).
        /// </remarks>
        RelationshipType Relationship(Expression predicate, Expression other);
    }
}
