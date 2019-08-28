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
            // user can always choose to use builtin.xxx to disambiguate with template xxx
            var builtInPrefix = "builtin.";

            if (name.StartsWith(builtInPrefix))
            {
                return BuiltInFunctions.Lookup(name.Substring(builtInPrefix.Length));
            }

            if (_evaluator.TemplateMap.ContainsKey(name))
            {
                return new ExpressionEvaluator(name, BuiltInFunctions.Apply(this.TemplateEvaluator(name)), ReturnType.String, this.ValidTemplateReference);
            }

            return BuiltInFunctions.Lookup(name);
        }

        public Func<IReadOnlyList<object>, object> TemplateEvaluator(string templateName)
            => (IReadOnlyList<object> args) =>
            {
                var newScope = _evaluator.ConstructScope(templateName, args.ToList());
                return _evaluator.EvaluateTemplate(templateName, newScope);
            };

        public void ValidTemplateReference(Expression expression)
        {
            var templateName = expression.Type;

            if (!_evaluator.TemplateMap.ContainsKey(templateName))
            {
                throw new Exception($"no such template '{templateName}' to call in {expression}");
            }

            var expectedArgsCount = _evaluator.TemplateMap[templateName].Parameters.Count();
            var actualArgsCount = expression.Children.Length;

            if (expectedArgsCount != actualArgsCount)
            {
                throw new Exception($"arguments mismatch for template {templateName}, expect {expectedArgsCount} actual {actualArgsCount}");
            }
        }
    }
}
