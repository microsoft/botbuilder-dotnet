using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.ComposableDialogs.Slots
{
    public class TextSlot : Slot<string>
    {
        public TextSlot()
        {
        }

        public int MinLength { get; set; } = 0;
        public int MaxLength { get; set; } = 1000;

        public string TooSmallText { get; set; } = "The value not long enough. The minimum length is slot.MinLength.";
        public string TooLargeText { get; set; } = "The value is too long. The maximum length is slot.MaxLength.";

        public override Task ValidateValue(string newValue)
        {
            if (newValue == null || newValue.Length < MinLength)
                throw new ArgumentOutOfRangeException(this.Id,
                    newValue,
                    this.BindToText(this.TooSmallText));

            if (newValue.Length > MaxLength)
                throw new ArgumentOutOfRangeException(this.Id,
                    newValue,
                    this.BindToText(this.TooLargeText));

            return base.ValidateValue(newValue);
        }
    }
}
