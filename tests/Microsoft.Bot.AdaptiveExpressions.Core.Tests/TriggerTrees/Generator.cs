// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1601 // Partial elements should be documented

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.AdaptiveExpressions.Core;

namespace Microsoft.Bot.AdaptiveExpressions.Core.TriggerTrees.Tests
{
    public partial class Generator
    {
        private const double DoubleEpsilon = 0.000001;

        private static readonly string[] Comparisons = new string[]
        {
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.Equal,

            // TODO: null values are always not equal ExpressionType.NotEqual,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.GreaterThan
        };

        public Generator(int seed = 0)
        {
            Rand = new Random(seed);
        }

        public Random Rand { get; set; }

        /* Predicates */

        public Expression GenerateString(int length) => Expression.ConstantExpression(RandomString(length));

        public string RandomString(int length)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < length; ++i)
            {
                builder.Append((char)('a' + Rand.Next(26)));
            }

            return builder.ToString();
        }

        public ExpressionInfo GenerateSimpleComparison(string name)
        {
            Expression expression = null;
            object value = null;
            var type = RandomChoice<string>(Comparisons);
            switch (Rand.Next(2))
            {
                case 0:
                    {
                        value = Rand.Next();
                        expression = Expression.MakeExpression(
                            type,
                            Expression.Accessor(name),
                            Expression.ConstantExpression(AdjustValue((int)value, type)));
                    }

                    break;
                case 1:
                    {
                        value = Rand.NextDouble();
                        expression = Expression.MakeExpression(
                            type,
                            Expression.Accessor(name),
                            Expression.ConstantExpression(AdjustValue((double)value, type)));
                    }

                    break;
            }

            return new ExpressionInfo(expression, name, value, type);
        }

        public ExpressionInfo GenerateHasValueComparison(string name)
        {
            Expression expression = null;
            object value = null;
            switch (Rand.Next(3))
            {
                case 0:
                    expression = Expression.MakeExpression(ExpressionType.Exists, Expression.Accessor(name));
                    value = Rand.Next();
                    break;
                case 1:
                    expression = Expression.MakeExpression(ExpressionType.Exists, Expression.Accessor(name));
                    value = Rand.NextDouble();
                    break;
                case 2:
                    expression = Expression.MakeExpression(ExpressionType.NotEqual, Expression.Accessor(name), Expression.ConstantExpression(null));
                    value = RandomString(5);
                    break;
            }

            return new ExpressionInfo(expression, name, value, ExpressionType.Not);
        }

        public List<ExpressionInfo> GeneratePredicates(int n, string nameBase)
        {
            var expressions = new List<ExpressionInfo>();
            for (var i = 0; i < n; ++i)
            {
                var name = $"{nameBase}{i}";
                var selection = RandomWeighted(new double[] { 1.0, 1.0 });
                switch (selection)
                {
                    case 0: expressions.Add(GenerateSimpleComparison(name)); break;
                    case 1: expressions.Add(GenerateHasValueComparison(name)); break;
                }
            }

            return expressions;
        }

        public List<ExpressionInfo> GenerateConjunctions(List<ExpressionInfo> predicates, int numConjunctions, int minClause, int maxClause)
        {
            var conjunctions = new List<ExpressionInfo>();
            for (var i = 0; i < numConjunctions; ++i)
            {
                var clauses = minClause + Rand.Next(maxClause - minClause);
                var expressions = new List<ExpressionInfo>();
                var used = new List<int>();
                for (var j = 0; j < clauses; ++j)
                {
                    int choice;
                    do
                    {
                        choice = Rand.Next(predicates.Count);
                    }
                    while (used.Contains(choice));

                    expressions.Add(predicates[choice]);
                    used.Add(choice);
                }

                var conjunction = Binary(ExpressionType.And, expressions, out var bindings);
                conjunctions.Add(new ExpressionInfo(conjunction, bindings));
            }

            return conjunctions;
        }

        public List<ExpressionInfo> GenerateDisjunctions(List<ExpressionInfo> predicates, int numDisjunctions, int minClause, int maxClause)
        {
            var disjunctions = new List<ExpressionInfo>();
            for (var i = 0; i < numDisjunctions; ++i)
            {
                var clauses = minClause + Rand.Next(maxClause - minClause);
                var expressions = new List<ExpressionInfo>();
                var used = new List<int>();
                for (var j = 0; j < clauses; ++j)
                {
                    int choice;
                    do
                    {
                        choice = Rand.Next(predicates.Count);
                    }
                    while (used.Contains(choice));
                    expressions.Add(predicates[choice]);
                    used.Add(choice);
                }

                var disjunction = Binary(ExpressionType.Or, expressions, out var bindings);
                disjunctions.Add(new ExpressionInfo(disjunction, bindings));
            }

            return disjunctions;
        }

        public List<ExpressionInfo> GenerateOptionals(List<ExpressionInfo> predicates, int numOptionals, int minClause, int maxClause)
        {
            var optionals = new List<ExpressionInfo>();
            for (var i = 0; i < numOptionals; ++i)
            {
                var clauses = minClause + Rand.Next(maxClause - minClause);
                var expressions = new List<ExpressionInfo>();
                var used = new List<int>();
                for (var j = 0; j < clauses; ++j)
                {
                    int choice;
                    do
                    {
                        choice = Rand.Next(predicates.Count);
                    }
                    while (used.Contains(choice));

                    var predicate = predicates[choice];
                    if (j == 0)
                    {
                        var optional = Expression.MakeExpression(Expression.Lookup(ExpressionType.Optional), predicate.Expression);
                        if (Rand.NextDouble() < 0.25)
                        {
                            optional = Expression.NotExpression(optional);
                        }

                        expressions.Add(new ExpressionInfo(optional, predicate.Bindings));
                    }
                    else
                    {
                        expressions.Add(predicate);
                    }

                    used.Add(choice);
                }

                var conjunction = Binary(ExpressionType.And, expressions, out var bindings);
                optionals.Add(new ExpressionInfo(conjunction, bindings));
            }

            return optionals;
        }

        public Expression Binary(
            string type,
            IEnumerable<ExpressionInfo> expressions,
            out Dictionary<string, Comparison> bindings)
        {
            bindings = MergeBindings(expressions);
            Expression binaryExpression = null;
            foreach (var info in expressions)
            {
                if (binaryExpression == null)
                {
                    binaryExpression = info.Expression;
                }
                else
                {
                    binaryExpression = Expression.MakeExpression(type, binaryExpression, info.Expression);
                }
            }

            return binaryExpression;
        }

        public IEnumerable<Expression> Predicates(IEnumerable<ExpressionInfo> expressions)
        {
            foreach (var info in expressions)
            {
                yield return info.Expression;
            }
        }

        public List<ExpressionInfo> GenerateQuantfiers(List<ExpressionInfo> predicates, int numExpressions, int maxVariable, int maxExpansion, int maxQuantifiers)
        {
            var result = new List<ExpressionInfo>();
            var allBindings = MergeBindings(predicates);
            var allTypes = VariablesByType(allBindings);
            for (var exp = 0; exp < numExpressions; ++exp)
            {
                var expression = RandomChoice(predicates);
                var info = new ExpressionInfo(expression.Expression);
                var numQuants = 1 + Rand.Next(maxQuantifiers - 1);
                var chosen = new HashSet<string>();
                var maxBase = Math.Min(expression.Bindings.Count, numQuants);
                for (var quant = 0; quant < maxBase; ++quant)
                {
                    KeyValuePair<string, Comparison> baseBinding;

                    // Can only map each expression variable once in a quantifier
                    do
                    {
                        baseBinding = expression.Bindings.ElementAt(Rand.Next(expression.Bindings.Count));
                    }
                    while (chosen.Contains(baseBinding.Key));
                    chosen.Add(baseBinding.Key);
                    SplitMemory(baseBinding.Key, out var baseName);
                    var mappings = new List<string>();
                    var expansion = 1 + Rand.Next(maxExpansion - 1);
                    for (var i = 0; i < expansion; ++i)
                    {
                        if (i == 0)
                        {
                            mappings.Add($"{baseBinding.Key}");
                        }
                        else
                        {
                            var mapping = RandomChoice<string>(allTypes[baseBinding.Value.Value.GetType()]);
                            if (!mappings.Contains(mapping))
                            {
                                mappings.Add(mapping);
                            }
                        }
                    }

                    var any = Rand.NextDouble() < 0.5;
                    if (any)
                    {
                        var mem = RandomChoice(mappings);
                        if (!info.Bindings.ContainsKey(mem))
                        {
                            info.Bindings.Add(mem, baseBinding.Value);
                        }

                        info.Quantifiers.Add(new Quantifier(baseBinding.Key, QuantifierType.Any, mappings));
                    }
                    else
                    {
                        foreach (var mapping in mappings)
                        {
                            if (!info.Bindings.ContainsKey(mapping))
                            {
                                info.Bindings.Add(mapping, baseBinding.Value);
                            }
                        }

                        info.Quantifiers.Add(new Quantifier(baseBinding.Key, QuantifierType.All, mappings));
                    }
                }

                result.Add(info);
            }

            return result;
        }

        public IEnumerable<ExpressionInfo> GenerateNots(IList<ExpressionInfo> predicates, int numNots)
        {
            for (var i = 0; i < numNots; ++i)
            {
                var expr = RandomChoice(predicates);
                var bindings = new Dictionary<string, Comparison>();
                foreach (var binding in expr.Bindings)
                {
                    var comparison = NotValue(binding.Value);
                    if (comparison != null)
                    {
                        bindings.Add(binding.Key, comparison);
                    }
                }

                yield return new ExpressionInfo(Expression.NotExpression(expr.Expression), bindings, expr.Quantifiers);
            }
        }

        public Dictionary<string, Comparison> MergeBindings(IEnumerable<ExpressionInfo> expressions)
        {
            var bindings = new Dictionary<string, Comparison>();
            foreach (var info in expressions)
            {
                foreach (var binding in info.Bindings)
                {
                    bindings[binding.Key] = binding.Value;
                }
            }

            return bindings;
        }

        public T RandomChoice<T>(IList<T> choices) => choices[Rand.Next(choices.Count)];

        public T RandomWeighted<T>(IEnumerable<WeightedChoice<T>> choices)
        {
            var totalWeight = 0.0;
            foreach (var choice in choices)
            {
                totalWeight += choice.Weight;
            }

            var selection = Rand.NextDouble() * totalWeight;
            var soFar = 0.0;
            var result = default(T);
            foreach (var choice in choices)
            {
                if (soFar <= selection)
                {
                    soFar += choice.Weight;
                    result = choice.Choice;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public int RandomWeighted(IReadOnlyList<double> weights)
        {
            var totalWeight = 0.0;
            foreach (var weight in weights)
            {
                totalWeight += weight;
            }

            var selection = Rand.NextDouble() * totalWeight;
            var soFar = 0.0;
            var result = 0;
            for (var i = 0; i < weights.Count; ++i)
            {
                if (soFar <= selection)
                {
                    soFar += weights[i];
                    result = i;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private int SplitMemory(string mem, out string baseName)
        {
            var i = 0;
            for (; i < mem.Length; ++i)
            {
                if (char.IsDigit(mem[i]))
                {
                    break;
                }
            }

            baseName = mem.Substring(0, i);
            return int.Parse(mem.Substring(i));
        }

        private int AdjustValue(int value, string type)
        {
            var result = value;
            const int epsilon = 1;
            switch (type)
            {
                case ExpressionType.LessThan: result += epsilon; break;
                case ExpressionType.NotEqual: result += epsilon; break;
                case ExpressionType.GreaterThan: result -= epsilon; break;
            }

            return result;
        }

        private double AdjustValue(double value, string type)
        {
            var result = value;
            switch (type)
            {
                case ExpressionType.LessThan: result += DoubleEpsilon; break;
                case ExpressionType.NotEqual: result += DoubleEpsilon; break;
                case ExpressionType.GreaterThan: result -= DoubleEpsilon; break;
            }

            return result;
        }

        private Comparison NotValue(Comparison comparison)
        {
            var value = comparison.Value;
            var type = value.GetType();
            var isNot = false;

            if (type != typeof(int) && type != typeof(double) && type != typeof(string))
            {
                throw new Exception($"Unsupported type {type}");
            }

            switch (comparison.Type)
            {
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.LessThan:
                    {
                        if (type == typeof(int))
                        {
                            value = (int)value + 1;
                        }
                        else if (type == typeof(double))
                        {
                            value = (double)value + DoubleEpsilon;
                        }
                    }

                    break;
                case ExpressionType.Equal:
                    {
                        if (type == typeof(int))
                        {
                            value = (int)value - 1;
                        }
                        else if (type == typeof(double))
                        {
                            value = (double)value - DoubleEpsilon;
                        }
                    }

                    break;
                case ExpressionType.NotEqual:
                    {
                        if (type == typeof(int))
                        {
                            value = (int)value - 1;
                        }
                        else if (type == typeof(double))
                        {
                            value = (double)value - DoubleEpsilon;
                        }
                    }

                    break;
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                    {
                        if (type == typeof(int))
                        {
                            value = (int)value - 1;
                        }
                        else if (type == typeof(double))
                        {
                            value = (double)value - DoubleEpsilon;
                        }
                    }

                    break;
                case ExpressionType.Not:
                    {
                        isNot = true;
                    }

                    break;
            }

            return isNot ? null : new Comparison(comparison.Type, value);
        }

        private Dictionary<Type, List<string>> VariablesByType(Dictionary<string, Comparison> bindings)
        {
            var result = new Dictionary<Type, List<string>>();
            foreach (var binding in bindings)
            {
                var type = binding.Value.Value.GetType();
                if (!result.ContainsKey(type))
                {
                    result.Add(type, new List<string>());
                }

                result[type].Add(binding.Key);
            }

            return result;
        }
    }
}
