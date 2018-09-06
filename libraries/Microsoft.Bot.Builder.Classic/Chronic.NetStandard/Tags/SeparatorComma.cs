namespace Chronic
{
    public class SeparatorComma : Separator
    {
        public SeparatorComma() : base(Separator.Type.Comma) { }
    }

    public class SeparatorAt : Separator
    {
        public SeparatorAt() : base(Separator.Type.At) { }
    }

    public class SeparatorIn : Separator
    {
        public SeparatorIn() : base(Separator.Type.In) { }
    }

    public class SeparatorDate : Separator
    {
        public SeparatorDate(Separator.Type value) : base(value) { }
    }

    public class SeparatorOn : Separator
    {
        public SeparatorOn() : base(Separator.Type.On) { }
    }
}