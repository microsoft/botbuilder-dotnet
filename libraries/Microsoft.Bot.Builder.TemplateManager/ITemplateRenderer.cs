using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.TemplateManager
{
    /// <summary>
    /// Defines interface for data binding to template and rendering a string.
    /// </summary>
    public interface ITemplateRenderer
    {
        /// <summary>
        /// render a template to an activity or string.
        /// </summary>
        /// <param name="turnContext">context.</param>
        /// <param name="language">language to render.</param>
        /// <param name="templateId">template to render.</param>
        /// <param name="data">data object to use to render.</param>
        /// <returns>Task.</returns>
#pragma warning disable UseAsyncSuffix // Use Async suffix (we can't change this without breaking compat)
        Task<object> RenderTemplate(ITurnContext turnContext, string language, string templateId, object data);
#pragma warning restore UseAsyncSuffix // Use Async suffix
    }
}
