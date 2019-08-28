//using System;
//using System.Threading.Tasks;

//namespace Microsoft.Bot.Builder.Dialogs.Composition.Slots
//{
//    public class TimeSpanSlot : Slot<TimeSpan>, IRangeSlot<TimeSpan>
//    {
//        public TimeSpanSlot()
//        {
//        }

//        public TimeSpan MinValue { get; set; } = TimeSpan.MinValue;
//        public TimeSpan MaxValue { get; set; } = TimeSpan.MaxValue;
//        // public Timex Resolution { get; set; } 

//        public string TooSmallText { get; set; } = "The value DATE($.newValue, r) is smaller then the minimum value of DATE(slot.MinValue).";
//        public string TooLargeText { get; set; } = "The value DATE($.newValue, r) is larger then the minimum value of DATE(slot.MaxValue).";

//        public override Task ValidateValue(TimeSpan newValue)
//        {
//            if (newValue < MinValue)
//                throw new ArgumentOutOfRangeException(this.Id,
//                    newValue,
//                    this.BindToText(this.TooSmallText));

//            if (newValue > MaxValue)
//                throw new ArgumentOutOfRangeException(this.Id,
//                    newValue,
//                    this.BindToText(this.TooLargeText));

//            return base.ValidateValue(newValue);
//        }
//    }
//}
