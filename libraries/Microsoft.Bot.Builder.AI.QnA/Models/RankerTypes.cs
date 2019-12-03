namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Enumeration of types of ranking.
    /// </summary>
    public enum RankerTypes
    {
        /// <summary>
        /// Default Ranker Behaviour. i.e. Ranking based on Questions and Answer.
        /// </summary>
        Default,

        /// <summary>
        /// Ranker based on question Only.
        /// </summary>
        QuestionOnly,

        /// <summary>
        /// Ranker based on Autosuggest for question field Only.
        /// </summary>
        AutoSuggestQuestion
    }
}
