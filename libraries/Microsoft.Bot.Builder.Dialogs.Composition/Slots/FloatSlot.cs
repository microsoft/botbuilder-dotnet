//using System;
//using System.Threading.Tasks;

//namespace Microsoft.Bot.Builder.Dialogs.Composition.Slots
//{
//    public class FloatSlot : Slot<float>, IRangeSlot<float>
//    {
//        public FloatSlot() 
//        {
//        }


//        public float MinValue { get; set; } = float.MinValue;
//        public float MaxValue { get; set; } = float.MaxValue;

//        public string TooSmallText { get; set; } = "The value FORMAT($.newValue, r) is smaller then the minimum value of FORMAT(slot.MinValue).";
//        public string TooLargeText { get; set; } = "The value FORMAT($.newValue, r) is larger then the minimum value of FORMAT(slot.MaxValue).";

//        public override Task ValidateValue(float newValue)
//        {
//            if (newValue < MinValue)
//                throw new ArgumentOutOfRangeException(this.BindToText(this.NameText), newValue, this.BindToText(TooSmallText));

//            if (newValue > MaxValue)
//                throw new ArgumentOutOfRangeException(this.BindToText(this.NameText), newValue, this.BindToText(TooLargeText));

//            return base.ValidateValue(newValue);
//        }

//    }
//}
