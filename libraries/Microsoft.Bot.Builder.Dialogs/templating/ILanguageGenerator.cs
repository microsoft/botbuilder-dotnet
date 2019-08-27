using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines interface for a Language Generator system to bind to text.
    /// </summary>
    public interface ILanguageGenerator
    {
        /// <summary>
        /// Method to bind data to string.
        /// </summary>
        /// <param name="turnContext">turnContext.</param>
        /// <param name="template">template or [templateId].</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>text.</returns>
        Task<string> Generate(ITurnContext turnContext, string template, object data);
    }
}
