using System;

namespace Chronic.Handlers
{
    public static class Handle
    {
        public static HandlerBuilder Optional<THandler>()
        {
            return new HandlerBuilder().Optional<THandler>();
        }

        public static HandlerBuilder Required<THandler>()
        {
            return new HandlerBuilder().Required<THandler>();
        }

        public static Repetition Repeat(Action<HandlerBuilder> pattern)
        {
            return new HandlerBuilder().Repeat(pattern);
        }
    }
}