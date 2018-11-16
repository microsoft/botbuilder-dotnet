using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TemplateManager;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    /// <summary>
    /// interface for defining a slot
    /// </summary>
    public interface ISlot
    {
        /// <summary>
        /// Programmatic id for the slot
        /// </summary>
        /// <example>"ArrivalDate" </example>   
        string Id { get; set; }

        /// <summary>
        /// The actual value type of the slot
        /// </summary>
        Type ValueType { get; set; }

        /// <summary>
        /// (RESOURCE) Display name for the slot
        /// </summary>
        /// <example>"Arrival Date" </example>
        string NameText { get; set; }

        /// <summary>
        /// (RESOURCE) Description of the slot (without the value)
        /// </summary>
        /// <example>"The date which your flight will arrive."</example> 
        string DescriptionText { get; set; }

        /// <summary>
        /// (RESOURCE) Formats a value as text (can be used for CurrentValue,DefaultValue,MinValue,MaxValue, etc.)
        /// </summary>
        /// <example>Saturday the 14th at 4:33PM</example>
        string ValueText { get; set; }

        /// <summary>
        /// (RESOURCE) Describes current value as sentence.
        /// </summary>
        /// <example>"Your arrival date will be Saturday the 14th at 4:33PM"</example>
        string CurrentValueDescription { get; set; }

        /// <summary>
        /// (RESOURCE) Text for prompting for this slot specifically
        /// </summary>
        string PromptText { get; set; }

        /// <summary>
        /// Validate that value is valid for slot
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task ValidateType(object value);
    }
}
