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
            try
            {
                // EvaluateTemplate with the auto property binder first
              
                var result = PropertyBinder.Auto(instance, property);
                return result;
            }
            catch (Exception ex)
            {
                // If sth wrong, we chech this indentifier is a templateName or not
                // Which means, normal property has high priority here
                if (property is string s)
                {
                    if (_evaluator.Context.TemplateContexts.ContainsKey(s))
                    {
                        return s;
                    }
                }
                try
                {
                    return ((dynamic)instance)[property];
                }
                catch (Exception)
                {
                    throw new Exception($"instance {instance} does not have property {property}");
                }
            }

            
        }
    }
}
