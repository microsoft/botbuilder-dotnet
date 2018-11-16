using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.TemplateManager;

namespace Microsoft.Bot.Builder.ComposableDialogs
{
    interface IRangeSlot<ValueT> : ITypedSlot<ValueT>
    {
        /// <summary>
        /// The min value to accept 
        /// </summary>
        ValueT MinValue { get; set; }

        /// <summary>
        /// The max value to accept 
        /// </summary>
        ValueT MaxValue { get; set; }

        /// <summary>
        /// Text for when value is too small
        /// </summary>
        string TooSmallText { get; set; }

        /// <summary>
        /// Text for when value is too large
        /// </summary>
        string TooLargeText { get; set; }
    }
}
