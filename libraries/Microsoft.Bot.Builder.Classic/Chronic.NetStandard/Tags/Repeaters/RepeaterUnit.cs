using Chronic.Tags.Repeaters;

namespace Chronic.Tags.Repeaters
{
    public abstract class RepeaterUnit : Repeater<UnitName>
    {
        protected RepeaterUnit(UnitName type)
            : base(type)
        {
        }
    }
}
