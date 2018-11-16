using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    // inteface for defining a slot
    public interface ITypedSlot<ValueT> : ISlot
    {
        /// <summary>
        /// default value for the slot
        /// </summary>
        ValueT DefaultValue { get; set; }

        /// <summary>
        /// Validate that a value is compatible 
        /// </summary>  
        /// <remarks>performs validation, throws exception if value is not valid </remarks>
        Task ValidateValue(ValueT newValue);
    }
}
