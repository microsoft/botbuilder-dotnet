using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Expressions;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public interface IGetMethod
    {
        ExpressionEvaluator GetMethodX(string name);
    }

    class GetMethodExtensions : IGetMethod
    {
        // Hold an evaluator instance to make sure all functions have access
        // This ensentially make all functions as closure
        // This is perticularly used for using templateName as lambda
        // Such as {foreach(alarms, ShowAlarm)}
        private readonly Evaluator _evaluator;

        public GetMethodExtensions(Evaluator evaluator)
        {
            _evaluator = evaluator;
        }

        // 
        public ExpressionEvaluator GetMethodX(string name)
        {
            // TODO: Should add verifiers and validators
            switch (name)
            {
                case "join":
                    return new ExpressionEvaluator(BuiltInFunctions.Apply(this.Join));
                case "foreach":
                case "map":
                    return new ExpressionEvaluator(BuiltInFunctions.Apply(this.Foreach));
                case "mapjoin":
                case "humanize":
                    return new ExpressionEvaluator(BuiltInFunctions.Apply(this.ForeachThenJoin));
            }
            return BuiltInFunctions.Lookup(name);
        }

        public object Count(IReadOnlyList<object> parameters)
        {
            if (parameters[0] is IList li)
            {
                return li.Count;
            }
            throw new NotImplementedException();
        }

        public object Join(IReadOnlyList<object> parameters)
        {
            object result = null;
            if (parameters.Count == 2 &&
                parameters[0] is IList p0 &&
                parameters[1] is String sep)
            {
                result = String.Join(sep + " ", p0.OfType<object>().Select(x => x.ToString())); // "," => ", " 
            }
            else if (parameters.Count == 3 &&
                parameters[0] is IList li &&
                parameters[1] is String sep1 &&
                parameters[2] is String sep2)
            {
                sep1 = sep1 + " "; // "," => ", "
                sep2 = " " + sep2 + " "; // "and" => " and "

                if (li.Count < 3)
                {
                    result = String.Join(sep2, li.OfType<object>().Select(x => x.ToString()));
                }
                else
                {
                    var firstPart = String.Join(sep1, li.OfType<object>().TakeWhile(o => o != null && o != li.OfType<object>().LastOrDefault()));
                    result = firstPart + sep2 + li.OfType<object>().Last().ToString();
                }
            }
            return result;
        }

        public object Foreach(IReadOnlyList<object> parameters)
        {
            if (parameters.Count == 2 && 
                parameters[0] is IList li && 
                parameters[1] is string func)
            {
                if (!IsTemplateRef(ref func) || !_evaluator.TemplateMap.ContainsKey(func))
                {
                    throw new Exception($"No such template defined: {func}");
                }

                var result = li.OfType<object>().Select(x =>
                {
                    var newScope = _evaluator.ConstructScope(func, new List<object>() { x });
                    var evaled = _evaluator.EvaluateTemplate(func, newScope);
                    return evaled;
                }).ToList();

                return result;
            }

            throw new NotImplementedException();
        }

        public object ForeachThenJoin(IReadOnlyList<object> parameters)
        {
            if (parameters.Count >= 2 &&
                parameters[0] is IList li &&
                parameters[1] is string template)
            {
                template = template.TrimStart('[').TrimEnd(']');
                if (!_evaluator.TemplateMap.ContainsKey(template))
                {
                    throw new Exception($"No such template defined: {template}");
                }

                var result = li.OfType<object>().Select(x =>
                {
                    var newScope = _evaluator.ConstructScope(template, new List<object>() { x });
                    var evaled = _evaluator.EvaluateTemplate(template, newScope);
                    return evaled;
                }).ToList();

                var newParameter = parameters.Skip(2).ToList();
                newParameter.Insert(0, result);
                return this.Join(newParameter);
                
            }

            throw new NotImplementedException();
        }


        private bool IsTemplateRef(ref string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                return false;

            if(templateName.StartsWith("[") && templateName.EndsWith("]"))
            {
                templateName = templateName.Substring(1, templateName.Length - 2);
                return true;
            }

            return false;
        }
    }
}
