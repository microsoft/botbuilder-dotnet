using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public interface IGetValue
    {
        object GetValueX(object instance, object property);
    }

    public class FuncParameterGetValue : IGetValue
    {
        private readonly Func<string, object, object> getValueFunc;

        public FuncParameterGetValue(Func<string, object, object> func)
        {
            this.getValueFunc = func ?? throw new ArgumentNullException(nameof(func));
        }

        public object GetValueX(object instance, object property)
        {
            return getValueFunc(property?.ToString(), instance);
        }
    }


    class GetValueExtensions : IGetValue
    {
        private readonly Evaluator _evaluator;
        public GetValueExtensions(Evaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public object GetValueX(object instance, object property)
        {
            // LG engine will not do nothing special on GetValue anymore
            return  PropertyBinder.Auto(instance, property);
        }
    }
}
