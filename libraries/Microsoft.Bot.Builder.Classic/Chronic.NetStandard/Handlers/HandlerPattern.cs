namespace Chronic.Handlers
{
    public class HandlerPattern
    {
        public bool IsOptional { get; private set; }

        public HandlerPattern(bool isOptional)
        {
            IsOptional = isOptional;
        }
    }
}