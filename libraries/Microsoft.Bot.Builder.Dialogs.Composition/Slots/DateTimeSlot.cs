using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Slots
{
    public class DateTimeSlot : Slot<DateTime>, IRangeSlot<DateTime>
    {
        public DateTimeSlot() 
        {
        }

        public DateTime MinValue { get; set; } = DateTime.MinValue;
        public DateTime MaxValue { get; set; } = DateTime.MaxValue;

        public string TooSmallText { get; set; } = "The value DATE($.newValue, r) is smaller then the minimum value of DATE(slot.MinValue).";
        public string TooLargeText { get; set; } = "The value DATE($.newValue, r) is larger then the minimum value of DATE(slot.MaxValue).";

        public override Task ValidateValue(DateTime newValue)
        {
            if (newValue < MinValue)
                throw new ArgumentOutOfRangeException(this.Id,
                    newValue,
                    this.BindToText(this.TooSmallText));

            if (newValue > MaxValue)
                throw new ArgumentOutOfRangeException(this.Id,
                    newValue,
                    this.BindToText(this.TooLargeText));

            return base.ValidateValue(newValue);
        }
    }
}
