using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Expressions;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public interface IGetMethod
    {
        ExpressionEvaluator GetMethodX(string name);
    }

    internal class GetMethodExtensions : IGetMethod
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

        public ExpressionEvaluator GetMethodX(string name)
        {
            if (name.StartsWith("builtin."))
            {
                return BuiltInFunctions.Lookup(name.Substring("builtin.".Length));
            }

            // TODO: Should add verifiers and validators
            switch (name)
            {
                case "lgTemplate":
                    return new ExpressionEvaluator("lgTemplate", BuiltInFunctions.Apply(this.LgTemplate), ReturnType.String, this.ValidLgTemplate);
                case "join":
                    return new ExpressionEvaluator("join", BuiltInFunctions.Apply(this.Join));
            }

            if (_evaluator.TemplateMap.ContainsKey(name))
            {
                // TODO
                // 1. add validation function here
                return new ExpressionEvaluator($"lgTemplate({name})", BuiltInFunctions.Apply(this.TemplateEvaluator(name)), ReturnType.String, null);
            }

            return BuiltInFunctions.Lookup(name);
        }

        public Func<IReadOnlyList<object>, object> TemplateEvaluator(string templateName)
        {
            return (IReadOnlyList<object> args) =>
            {
                var newArgs = new List<object>();
                newArgs.Add(templateName);
                newArgs.AddRange(args);
                return this.LgTemplate(newArgs);
            };
        }

        public object LgTemplate(IReadOnlyList<object> args)
        {
            var templateName = (string)args[0];
            var newScope = _evaluator.ConstructScope(templateName, args.Skip(1).ToList());
            var result = _evaluator.EvaluateTemplate(templateName, newScope);
            return result;
        }

        public void ValidLgTemplate(Expression expression)
        {
            if (expression.Children.Length == 0)
            {
                throw new Exception("lgTemplate requires 1 or more arguments");
            }

            if (!(expression.Children[0] is Constant cnst && cnst.Value is string))
            {
                throw new Exception($"lgTemplate expect a string as first argument, acutal {expression.Children[0]}");
            }

            var templateName = (string)(expression.Children[0] as Constant).Value;

            if (!_evaluator.TemplateMap.ContainsKey(templateName))
            {
                throw new Exception($"no such template '{templateName}' to call in {expression}");
            }

            var expectedArgsCount = _evaluator.TemplateMap[templateName].Parameters.Count();
            var actualArgsCount = expression.Children.Length - 1;

            if (expectedArgsCount != actualArgsCount)
            {
                throw new Exception($"arguments mismatch for template {templateName}, expect {expectedArgsCount} actual {actualArgsCount}");
            }
        }

        public object Join(IReadOnlyList<object> parameters)
        {
            object result = null;
            if (parameters.Count == 2 &&
                BuiltInFunctions.TryParseList(parameters[0], out var p0) &&
                parameters[1] is string sep)
            {
                result = string.Join(sep, p0.OfType<object>().Select(x => x.ToString())); 
            }
            else if (parameters.Count == 3 &&
                BuiltInFunctions.TryParseList(parameters[0], out var li) &&
                parameters[1] is string sep1 &&
                parameters[2] is string sep2)
            {
                if (li.Count < 3)
                {
                    result = string.Join(sep2, li.OfType<object>().Select(x => x.ToString()));
                }
                else
                {
                    var firstPart = string.Join(sep1, li.OfType<object>().TakeWhile(o => o != null && o != li.OfType<object>().LastOrDefault()));
                    result = firstPart + sep2 + li.OfType<object>().Last().ToString();
                }
            }

            return result;
        }
    }
}
