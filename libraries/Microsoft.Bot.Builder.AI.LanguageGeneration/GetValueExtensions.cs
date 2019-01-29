using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Expressions;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    class GetValueExtensions
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
                // Evaluate with the auto property binder first
              
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
                throw ex;
            }

            
        }
    }
}
