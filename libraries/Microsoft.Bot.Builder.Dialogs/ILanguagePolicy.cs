using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Language policy map
    /// "en-us" -> "en-us","en","".
    /// </summary>
    public interface ILanguagePolicy : IDictionary<string, string[]>
    {
    }
}
