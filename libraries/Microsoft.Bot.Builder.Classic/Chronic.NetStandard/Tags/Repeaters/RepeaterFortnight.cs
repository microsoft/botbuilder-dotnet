using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public class RepeaterFortnight : RepeaterUnit
    {
        public RepeaterFortnight()
            : base(UnitName.Fortnight)
        {

        }
        public override int GetWidth()
        {
            throw new System.NotImplementedException();
        }

        protected override Span NextSpan(Pointer.Type pointer)
        {
            throw new System.NotImplementedException();
        }

        protected override Span CurrentSpan(Pointer.Type pointer)
        {
            throw new System.NotImplementedException();
        }

        public override Span GetOffset(Span span, int amount, Pointer.Type pointer)
        {
            throw new System.NotImplementedException();
        }
    }
}