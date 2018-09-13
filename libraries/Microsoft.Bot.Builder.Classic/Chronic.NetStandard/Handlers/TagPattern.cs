using System;

namespace Chronic.Handlers
{
    public class TagPattern : HandlerPattern
    {
        public Type TagType { get; private set; }

        public TagPattern(Type tagType)
            : this(tagType, false)
        {
        }

        public TagPattern(Type tagType, bool isOptional)
            : base(isOptional)
        {
            TagType = tagType;
        }

        public override string ToString()
        {
            return "[Tag:" + TagType.Name + (IsOptional ? "-?" : "") + "]";
        }
    }
}