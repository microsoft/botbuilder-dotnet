using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Expressions
{
    public class ExpressionException: Exception
    {
        public ExpressionException(string type, IReadOnlyList<object> operands)
            : base(BuildMessage(type, operands))
        {
        }

        private static string BuildMessage(string type, IReadOnlyList<object> operands)
        {
            var builder = new StringBuilder();
            builder.Append($"Cannot apply {type} to");
            foreach (var operand in operands)
            {
                builder.Append(" ");
                builder.Append(operand.ToString());
            }
            return builder.ToString();
        }
    }
}
