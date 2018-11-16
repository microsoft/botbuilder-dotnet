using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.ComposableDialogs.Slots
{
    public class Slot<ValueT> : ITypedSlot<ValueT>, ISlot
    {
        public Slot()
        {
            this.ValueType = typeof(ValueType);
        }


        public string Id { get; set; }

        public Type ValueType { get; set; }

        public ValueT DefaultValue { get; set; } = default(ValueT);

        public string NameText { get; set; }
        public string DescriptionText { get; set; }
        public string ValueText { get; set; }
        public string CurrentValueDescription { get; set; }
        public string PromptText { get; set; }

        public string BindToText(string expression, params object[] args)
        {
            if (expression.StartsWith("@"))
            {
                return expression;
            }
            return expression;
        }

        public virtual Task ValidateValue(ValueT value)
        {
            return Task.CompletedTask;
        }

        public Task ValidateType(object value)
        {
            if (!value.GetType().IsAssignableFrom(ValueType))
                throw new ArgumentException("bad type");

            return Task.CompletedTask;
        }
    }
}
