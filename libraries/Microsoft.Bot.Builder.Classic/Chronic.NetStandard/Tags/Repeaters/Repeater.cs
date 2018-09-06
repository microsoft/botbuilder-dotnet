using System.Collections.Generic;

namespace Chronic.Tags.Repeaters
{
    public abstract class Repeater<T> : Tag<T>, IRepeater
    {
        protected Repeater(T type)
            : base(type)
        {
        }

        public abstract int GetWidth();

        /// <summary>
        /// Returns the next occurance of this repeatable.
        /// </summary>
        public Span GetNextSpan(Pointer.Type pointer)
        {
            if (Now == null)
            {
                throw new IllegalStateException("StartSecond point must be set before calling #next");
            }
            var span = NextSpan(pointer);
            return span;
        }

        protected abstract Span NextSpan(Pointer.Type pointer);

        public Span GetCurrentSpan(Pointer.Type pointer)
        {
            if (Now == null)
            {
                throw new IllegalStateException("StartSecond point must be set before calling #this");
            }
            return CurrentSpan(pointer);
        }

        protected abstract Span CurrentSpan(Pointer.Type pointer);

        public abstract Span GetOffset(Span span, int amount, Pointer.Type pointer);

        public override string ToString()
        {
            return "repeater";
        }
    }
}