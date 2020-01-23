// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Properties
{
    /// <summary>
    /// ObjectExpression(T) - represents a property which is either an object of type T or a string expression which resolves to a object of type T.
    /// </summary>
    /// <typeparam name="T">the type of object.</typeparam>
    /// <remarks>String values are always interpreted as an expression, whether it has '=' prefix or not.</remarks>
    public class ObjectExpression<T> : ExpressionProperty<T>
    {
        public ObjectExpression()
        {
        }

        public ObjectExpression(T value) 
            : base(value)
        {
        }

        public ObjectExpression(string expressionOrString)
            : base(expressionOrString)
        {
        }

        public ObjectExpression(JToken value)
            : base(value)
        {
        }

        public static implicit operator ObjectExpression<T>(T value) => new ObjectExpression<T>(value);

        public static implicit operator ObjectExpression<T>(string value) => new ObjectExpression<T>(value);

        public static implicit operator ObjectExpression<T>(JToken value) => new ObjectExpression<T>(value);
    }
}
