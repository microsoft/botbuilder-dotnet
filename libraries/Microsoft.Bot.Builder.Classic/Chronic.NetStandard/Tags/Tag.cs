using System;

namespace Chronic
{
    public abstract class Tag<T> : ITag
    {
        public DateTime? Now { get; set; }
        public T Value { get; protected set; }
        public object RawValue
        {
            get { return Value; }
        }

        protected Tag(T value)
        {
            Value = value;
        }

    }

    public interface IRepeater : ITag
    {
        /// <summary>
        /// Returns the width (in seconds or months) of this repeatable.
        /// </summary>
        int GetWidth();

        Span GetNextSpan(Pointer.Type pointer);
        Span GetCurrentSpan(Pointer.Type pointer);
        Span GetOffset(Span span, int amount, Pointer.Type pointer);
    }

    public interface ITag
    {
        DateTime? Now { get; set; }
        object RawValue { get; }
    }
}