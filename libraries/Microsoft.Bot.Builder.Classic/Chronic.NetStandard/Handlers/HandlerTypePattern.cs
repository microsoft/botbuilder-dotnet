namespace Chronic.Handlers
{
    public class HandlerTypePattern : HandlerPattern
    {
        public HandlerType Type { get; private set; }

        public HandlerTypePattern(HandlerType type)
            : this(type, false)
        {

        }

        public HandlerTypePattern(HandlerType type, bool optional)
            : base(optional)
        {
            Type = type;
        }

        public override string ToString()
        {
            return "[Handler:" + Type.GetType().Name + "]" + (IsOptional ? "?" : "");
        }
    }
}