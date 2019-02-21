using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Expressions;
using System.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    class GetMethodExtensions
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
        public EvaluationDelegate GetMethodX(string name)
        {
            switch (name)
            {
                case "count":
                    return this.Count;
                case "join":
                    return this.Join;
                case "foreach":
                case "map":
                    return this.Foreach;
                case "mapjoin":
                case "humanize":
                    return this.ForeachThenJoin;
                default:
                    return MethodBinder.All(name);
            }
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
            if (parameters.Count == 2 &&
                parameters[0] is IList p0 &&
                parameters[1] is String sep)
            {
                return String.Join(sep + " ", p0.OfType<object>().Select(x => x.ToString())); // "," => ", " 
            }

            if (parameters.Count == 3 &&
                parameters[0] is IList li &&
                parameters[1] is String sep1 &&
                parameters[2] is String sep2)
            {
                sep1 = sep1 + " "; // "," => ", "
                sep2 = " " + sep2 + " "; // "and" => " and "

                if (li.Count < 3)
                {
                    return String.Join(sep2, li.OfType<object>().Select(x => x.ToString()));
                }
                else
                {
                    var firstPart = String.Join(sep1, li.OfType<object>().SkipLast(1));
                    return firstPart + sep2 + li.OfType<object>().Last().ToString();
                }
            }
            throw new NotImplementedException();
        }

        public object Foreach(IReadOnlyList<object> parameters)
        {
            if (parameters.Count == 2 && 
                parameters[0] is IList li && 
                parameters[1] is string func)
            {
                if (!_evaluator.Context.TemplateContexts.ContainsKey(func))
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
                parameters[1] is string func)
            {
                if (!_evaluator.Context.TemplateContexts.ContainsKey(func))
                {
                    throw new Exception($"No such template defined: {func}");
                }

                var result = li.OfType<object>().Select(x =>
                {
                    var newScope = _evaluator.ConstructScope(func, new List<object>() { x });
                    var evaled = _evaluator.EvaluateTemplate(func, newScope);
                    return evaled;
                }).ToList();

                var newParameter = parameters.Skip(2).ToList();
                newParameter.Insert(0, result);
                return this.Join(newParameter);
                
            }

            throw new NotImplementedException();
        }

    }

}
