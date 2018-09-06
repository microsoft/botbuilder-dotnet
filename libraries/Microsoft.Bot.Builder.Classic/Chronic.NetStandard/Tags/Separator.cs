namespace Chronic
{
    public class Separator : Tag<Separator.Type>
    {
        public Separator(Type value)
            : base(value)
        {

        }

        public enum Type
        {
            Comma,
            Dash,
            Slash,
            At,
            NewLine,
            In,
            On
        }
    }
}