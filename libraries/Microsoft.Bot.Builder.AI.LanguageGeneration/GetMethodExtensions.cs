using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Expressions;
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
        private readonly TemplateEvaluator _evaluator;
        public GetMethodExtensions(TemplateEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        // 
        public ExpressionEvaluator GetMethodX(string name)
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
            }
            throw new ArgumentException($"Unknown function {name} in expression.");
        }

        public Task<object> Count(IReadOnlyList<object> parameters)
        {
            if (parameters[0] is IList li)
            {
                return Task.FromResult<object>(li.Count);
            }
            throw new NotImplementedException();
        }

        public Task<object> Join(IReadOnlyList<object> parameters)
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
            return Task.FromResult(result);
        }

        public Task<object> Foreach(IReadOnlyList<object> parameters)
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

                return Task.FromResult<object>(result);
            }

            throw new NotImplementedException();
        }

        public Task<object> ForeachThenJoin(IReadOnlyList<object> parameters)
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
                return Task.FromResult<object>(this.Join(newParameter));
                
            }

            throw new NotImplementedException();
        }

    }

}
