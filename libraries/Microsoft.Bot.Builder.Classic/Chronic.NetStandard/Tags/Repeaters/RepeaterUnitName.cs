using System;

namespace Chronic.Tags.Repeaters
{
    public abstract class RepeaterUnitName : Repeater<UnitName>
    {
        protected RepeaterUnitName(UnitName value) : base(value)
        {
        }
    }
}