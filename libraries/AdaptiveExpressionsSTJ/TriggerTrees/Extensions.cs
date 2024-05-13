namespace AdaptiveExpressions.TriggerTrees
{
    /// <summary>
    /// Extension method to swap between <see cref="RelationshipType"/> "Generalizes" and "Specializes".
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Swap operation between RelationshipType.Generalizes and RelationshipType.Specializes.
        /// If the original is RelationshipType.Specializes, the swap operation returns RelationshipType.Generalizes.
        /// If the original is RelationshipType.Generalizes, the swap operation returns RelationshipType.Specializes.
        /// Otherwise, the swap operation returns the original RelationType.
        /// </summary>
        /// <param name="original">The original RelationType.</param>
        /// <returns>The RelationshipType after the swap.</returns>
        public static RelationshipType Swap(this RelationshipType original)
        {
            var relationship = original;
            switch (original)
            {
                case RelationshipType.Specializes:
                    relationship = RelationshipType.Generalizes;
                    break;
                case RelationshipType.Generalizes:
                    relationship = RelationshipType.Specializes;
                    break;
            }

            return relationship;
        }
    }
}
