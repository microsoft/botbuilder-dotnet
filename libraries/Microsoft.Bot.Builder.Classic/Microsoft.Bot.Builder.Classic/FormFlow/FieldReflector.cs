// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    #region Documentation
    /// <summary>   Fill in field information through reflection.</summary>
    /// <remarks>   The resulting information can be overridden through the fluent interface.</remarks>
    /// <typeparam name="T">    The form state. </typeparam>
    #endregion
    public class FieldReflector<T> : Field<T>
        where T : class
    {
        #region Documentation
        /// <summary>   Construct an <see cref="IField{T}"/> through reflection. </summary>
        /// <param name="name">                 Path to the field in your form state. </param>
        /// <param name="ignoreAnnotations">    True to ignore annotations. </param>
        #endregion
        public FieldReflector(string name, bool ignoreAnnotations = false)
            : base(name, FieldRole.Value)
        {
            _ignoreAnnotations = ignoreAnnotations;
            AddField(typeof(T), _name.Split('.'), 0);
        }

        #region IField

        #region IFieldState
        public override object GetValue(T state)
        {
            object current = state;
            Type ftype = null;
            foreach (var step in _path)
            {
                ftype = StepType(step);
                var field = step as FieldInfo;
                if (field != null)
                {
                    current = field.GetValue(current);
                }
                else
                {
                    var prop = (PropertyInfo)step;
                    current = prop.GetValue(current);
                }
                if (current == null)
                {
                    break;
                }
            }
            // Convert value types to null if appropriate
            return (ftype.IsEnum
                ? ((int)current == 0 ? null : current)
                : (ftype == typeof(DateTime) && ((DateTime)current) == DateTime.MinValue)
                    ? null
                    : current);
        }

        public override void SetValue(T state, object value)
        {
            object current = state;
            object lastClass = state;
            var last = _path.Last();
            foreach (var step in _path)
            {
                var field = step as FieldInfo;
                var prop = step as PropertyInfo;
                Type ftype = StepType(step);
                if (step == last)
                {
                    object newValue = value;
                    var utype = Nullable.GetUnderlyingType(ftype) ?? ftype;
                    if (ftype.IsIEnumerable())
                    {
                        if (value != null && ftype != typeof(string))
                        {
                            // Build list and coerce elements
                            var list = Activator.CreateInstance(ftype);
                            var addMethod = list.GetType().GetMethod("Add");
                            foreach (var elt in (System.Collections.IEnumerable)value)
                            {
                                addMethod.Invoke(list, new object[] { elt });
                            }
                            newValue = list;
                        }
                    }
                    else
                    {
                        if (value == null)
                        {
                            if (!ftype.IsNullable() && (ftype.IsEnum || ftype.IsIntegral() || ftype.IsDouble()))
                            {
                                // Null value for non-nullable numbers and enums is 0
                                newValue = 0;
                            }
                        }
                        else if (utype.IsIntegral())
                        {
                            newValue = Convert.ChangeType(value, utype);
                        }
                        else if (utype.IsDouble())
                        {
                            newValue = Convert.ChangeType(value, utype);
                        }
                        else if (utype == typeof(bool))
                        {
                            newValue = Convert.ChangeType(value, utype);
                        }
                    }
                    if (field != null)
                    {
                        field.SetValue(lastClass, newValue);
                    }
                    else
                    {
                        prop.SetValue(lastClass, newValue);
                    }
                }
                else
                {
                    current = (field == null ? prop.GetValue(current) : field.GetValue(current));
                    if (current == null)
                    {
                        var obj = Activator.CreateInstance(ftype);
                        current = obj;
                        if (field != null)
                        {
                            field.SetValue(lastClass, current);
                        }
                        else
                        {
                            prop.SetValue(lastClass, current);
                        }
                    }
                    lastClass = current;
                }
            }
        }

        public override bool IsUnknown(T state)
        {
            var unknown = false;
            var value = GetValue(state);
            if (value == null)
            {
                unknown = true;
            }
            else
            {
                var step = _path.Last();
                var ftype = StepType(step);
                if (ftype.IsValueType && ftype.IsEnum)
                {
                    unknown = ((int)value == 0);
                }
                else if (ftype == typeof(DateTime))
                {
                    unknown = ((DateTime)value) == default(DateTime);
                }
                else if (ftype.IsIEnumerable())
                {
                    unknown = !((System.Collections.IEnumerable)value).GetEnumerator().MoveNext();
                }
            }
            return unknown;
        }

        public override void SetUnknown(T state)
        {
            var step = _path.Last();
            var field = step as FieldInfo;
            var prop = step as PropertyInfo;
            var ftype = StepType(step);
            if (ftype.IsEnum)
            {
                SetValue(state, 0);
            }
            else if (ftype == typeof(DateTime))
            {
                SetValue(state, default(DateTime));
            }
            else
            {
                SetValue(state, null);
            }
        }

        #endregion
        #endregion

        #region Internals
        protected Type StepType(object step)
        {
            var field = step as FieldInfo;
            var prop = step as PropertyInfo;
            return (step == null ? null : (field == null ? prop.PropertyType : field.FieldType));
        }

        protected void AddField(Type type, string[] path, int ipath)
        {
            if (ipath < path.Length)
            {
                ProcessTemplates(type);
                var step = path[ipath];
                object field = type.GetField(step, BindingFlags.Public | BindingFlags.Instance);
                Type ftype;
                if (field == null)
                {
                    var prop = type.GetProperty(step, BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                    {
                        throw new MissingFieldException($"{step} is not a field or property in your type.");
                    }
                    field = prop;
                    ftype = prop.PropertyType;
                    _path.Add(prop);
                }
                else
                {
                    ftype = (field as FieldInfo).FieldType;
                    _path.Add(field);
                }
                if (ftype.IsNullable())
                {
                    _isNullable = true;
                    _keepZero = true;
                    ftype = Nullable.GetUnderlyingType(ftype);
                }
                else if (ftype.IsEnum || ftype.IsClass)
                {
                    _isNullable = true;
                }
                if (ftype.IsClass)
                {
                    if (ftype == typeof(string))
                    {
                        _type = ftype;
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype.IsIEnumerable())
                    {
                        var elt = ftype.GetGenericElementType();
                        _type = elt;
                        _allowsMultiple = true;
                        ProcessFieldAttributes(field);
                        if (elt.IsEnum)
                        {
                            ProcessEnumAttributes(elt);
                        }
                    }
                    else
                    {
                        AddField(ftype, path, ipath + 1);
                    }
                }
                else
                {
                    if (ftype.IsEnum)
                    {
                        ProcessFieldAttributes(field);
                        ProcessEnumAttributes(ftype);
                    }
                    else if (ftype == typeof(bool))
                    {
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype.IsIntegral())
                    {
                        long min = long.MinValue;
                        long max = long.MaxValue;
                        if (ftype == typeof(sbyte)) { min = sbyte.MinValue; max = sbyte.MaxValue; }
                        else if (ftype == typeof(byte)) { min = byte.MinValue; max = byte.MaxValue; }
                        else if (ftype == typeof(short)) { min = short.MinValue; max = short.MaxValue; }
                        else if (ftype == typeof(ushort)) { min = ushort.MinValue; max = ushort.MaxValue; }
                        else if (ftype == typeof(int)) { min = int.MinValue; max = int.MaxValue; }
                        else if (ftype == typeof(uint)) { min = uint.MinValue; max = uint.MaxValue; }
                        else if (ftype == typeof(long)) { min = long.MinValue; max = long.MaxValue; }
                        else if (ftype == typeof(ulong)) { min = long.MinValue; max = long.MaxValue; }
                        SetLimits(min, max, false);
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype.IsDouble())
                    {
                        double min = long.MinValue;
                        double max = long.MaxValue;
                        if (ftype == typeof(float)) { min = float.MinValue; max = float.MaxValue; }
                        else if (ftype == typeof(double)) { min = double.MinValue; max = double.MaxValue; }
                        SetLimits(min, max, false);
                        ProcessFieldAttributes(field);
                    }
                    else if (ftype == typeof(DateTime))
                    {
                        ProcessFieldAttributes(field);
                    }
                    _type = ftype;
                }
            }
        }

        protected void ProcessTemplates(Type type)
        {
            if (!_ignoreAnnotations)
            {
                foreach (var attribute in type.GetCustomAttributes(typeof(TemplateAttribute)))
                {
                    AddTemplate((TemplateAttribute)attribute);
                }
            }
        }

        protected void ProcessFieldAttributes(object step)
        {
            _optional = false;
            if (!_ignoreAnnotations)
            {
                var field = step as FieldInfo;
                var prop = step as PropertyInfo;
                var name = (field == null ? prop.Name : field.Name);
                var describe = (field == null ? prop.GetCustomAttribute<DescribeAttribute>() : field.GetCustomAttribute<DescribeAttribute>());
                var terms = (field == null ? prop.GetCustomAttribute<TermsAttribute>() : field.GetCustomAttribute<TermsAttribute>());
                var prompt = (field == null ? prop.GetCustomAttribute<PromptAttribute>() : field.GetCustomAttribute<PromptAttribute>());
                var optional = (field == null ? prop.GetCustomAttribute<OptionalAttribute>() : field.GetCustomAttribute<OptionalAttribute>());
                var numeric = (field == null ? prop.GetCustomAttribute<NumericAttribute>() : field.GetCustomAttribute<NumericAttribute>());
                var pattern = (field == null ? prop.GetCustomAttribute<PatternAttribute>() : field.GetCustomAttribute<PatternAttribute>());
                if (describe != null)
                {
                    _description = describe;
                }
                else
                {
                    _description = new DescribeAttribute(Language.CamelCase(name));
                }

                if (terms != null)
                {
                    _terms = terms;
                }
                else
                {
                    _terms = new TermsAttribute(Language.GenerateTerms((string.IsNullOrWhiteSpace(_description.Description) ? Language.CamelCase(name) : _description.Description), 3));
                }

                if (prompt != null)
                {
                    SetPrompt(prompt);
                }

                if (numeric != null)
                {
                    double oldMin, oldMax;
                    Limits(out oldMin, out oldMax);
                    SetLimits(numeric.Min, numeric.Max, numeric.Min != oldMin || numeric.Max != oldMax);
                }

                if (pattern != null)
                {
                    SetPattern(pattern.Pattern);
                }

                _optional = (optional != null);

                foreach (var attribute in (field == null ? prop.GetCustomAttributes<TemplateAttribute>() : field.GetCustomAttributes<TemplateAttribute>()))
                {
                    var template = (TemplateAttribute)attribute;
                    AddTemplate(template);
                }
            }
        }

        protected void ProcessEnumAttributes(Type type)
        {
            foreach (var enumField in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var enumValue = enumField.GetValue(null);
                if (_keepZero || (int)enumValue > 0)
                {
                    var describe = enumField.GetCustomAttribute<DescribeAttribute>();
                    var terms = enumField.GetCustomAttribute<TermsAttribute>();
                    if (describe != null && !_ignoreAnnotations)
                    {
                        if (describe.Description == null)
                        {
                            describe.Description = Language.CamelCase(enumValue.ToString());
                        }
                        _valueDescriptions.Add(enumValue, describe);
                    }
                    else
                    {
                        _valueDescriptions.Add(enumValue, new DescribeAttribute(Language.CamelCase(enumValue.ToString())));
                    }

                    if (terms != null && !_ignoreAnnotations)
                    {
                        _valueTerms.Add(enumValue, terms);
                    }
                    else
                    {
                        _valueTerms.Add(enumValue, new TermsAttribute(Language.GenerateTerms(Language.CamelCase(enumValue.ToString()), 4)));
                    }
                }
            }
        }

        /// <summary>   True to ignore annotations. </summary>
        protected bool _ignoreAnnotations;

        /// <summary>   Path to field value in state. </summary>
        protected List<object> _path = new List<object>();
        #endregion
    }
}
