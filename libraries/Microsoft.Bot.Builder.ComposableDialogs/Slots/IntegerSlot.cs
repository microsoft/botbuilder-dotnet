using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.ComposableDialogs.Slots
{
    public class IntegerSlot : Slot<int>, IRangeSlot<int>
    {
        public IntegerSlot() 
        {
        }

        public int MinValue { get; set; } = int.MinValue;
        public int MaxValue { get; set; } = int.MaxValue;

        public string TooSmallText { get; set; } = "The value FORMAT($.newValue, r) is smaller then the minimum value of FORMAT(slot.MinValue).";
        public string TooLargeText { get; set; } = "The value FORMAT($.newValue, r) is larger then the minimum value of FORMAT(slot.MaxValue).";

        public override Task ValidateValue(int newValue)
        {
            if (newValue < MinValue)
                throw new ArgumentOutOfRangeException(this.BindToText(this.NameText), newValue, this.BindToText(TooSmallText));

            if (newValue > MaxValue)
                throw new ArgumentOutOfRangeException(this.BindToText(this.NameText), newValue, this.BindToText(TooLargeText));

            return base.ValidateValue(newValue);
        }

    }
}
