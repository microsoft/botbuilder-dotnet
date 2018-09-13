using System.Collections.Generic;
using System.Linq;
using System;

namespace Chronic
{
    public class Token
    {
        readonly List<ITag> _tags = new List<ITag>();
        readonly string _value;
        public string Value { get { return _value; } }

        public Token(string value)
        {
            _value = value;
        }

        public void Tag(ITag tag)
        {
            _tags.Add(tag);
        }

        public void Untag<T>()
            where T : ITag
        {
            _tags.RemoveAll(tag => tag is T);
        }

        public void Tag(params ITag[] tags)
        {
            tags.ForEach(tag => _tags.Add(tag));
        }

        public void Tag(IEnumerable<ITag> tags)
        {
            tags.ForEach(tag => _tags.Add(tag));
        }

        public bool HasTags()
        {
            return _tags.Count > 0;
        }

        public T GetTag<T>()
            where T : ITag
        {
            return (T)_tags.FirstOrDefault(tag => tag is T);
        }

        public bool IsTaggedAs<T>()
            where T : ITag
        {
            return GetTag<T>() != null;
        }

        public bool IsNotTaggedAs<T>()
            where T : ITag
        {
            return GetTag<T>() == null;
        }

        internal bool IsTaggedAs(Type type)
        {
            return _tags.Any(tag => type.IsAssignableFrom(tag.GetType()));
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", Value, string.Join(",", _tags.Select(tag => tag.ToString())));
        }
    }
}