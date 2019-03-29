using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
     public static partial class Extensions
    {
        public static bool DeepEquals(this Expression expr, Expression other)
        {
            bool eq = true;
            if (expr != null && other != null)
            {
                eq = expr.NodeType == other.NodeType && expr.Type == other.Type;
                if (eq)
                {
                    switch (expr.NodeType)
                    {
                        case ExpressionType.Call:
                            {
                                var call = (MethodCallExpression)expr;
                                var callOther = (MethodCallExpression)other;
                                eq = call.Object.DeepEquals(callOther.Object)
                                    && call.Method == callOther.Method
                                    && call.Arguments.Count == callOther.Arguments.Count;
                                for (var i = 0; eq && i < call.Arguments.Count; ++i)
                                {
                                    eq = call.Arguments[i].DeepEquals(callOther.Arguments[i]);
                                }
                            }
                            break;
                        case ExpressionType.Conditional:
                            {
                                var cond = (ConditionalExpression)expr;
                                var condOther = (ConditionalExpression)other;
                                eq = cond.Test.DeepEquals(condOther.Test) &&
                                    cond.IfFalse.DeepEquals(condOther.IfFalse) &&
                                    cond.IfTrue.DeepEquals(condOther.IfTrue);
                            }
                            break;
                        case ExpressionType.Constant:
                            {
                                var constant = (ConstantExpression)expr;
                                var constantOther = (ConstantExpression)other;
                                if (constant.Value == null)
                                {
                                    eq = constantOther.Value == null;
                                }
                                else
                                {
                                    eq = constant.Value.Equals(constantOther.Value);
                                }
                            }
                            break;
                        case ExpressionType.MemberAccess:
                            {
                                var member = (MemberExpression)expr;
                                var memberOther = (MemberExpression)other;
                                eq = member.Member.Equals(memberOther.Member)
                                    && member.Expression.DeepEquals(memberOther.Expression);
                            }
                            break;
                        case ExpressionType.MemberInit:
                            {
                                var member = (MemberInitExpression)expr;
                                var memberOther = (MemberInitExpression)other;
                                eq = member.DeepEquals(memberOther);
                            }
                            break;
                        case ExpressionType.New:
                            {
                                var newExpr = (NewExpression)expr;
                                var newOther = (NewExpression)other;
                                eq = newExpr.Constructor.Equals(newOther.Constructor)
                                    && newExpr.Arguments.Count == newOther.Arguments.Count;
                                for (var i = 0; eq && i < newExpr.Arguments.Count; ++i)
                                {
                                    eq = newExpr.Arguments[i].DeepEquals(newOther.Arguments[i]);
                                }
                                eq = eq && newExpr.Members?.Count == newOther.Members?.Count;
                                if (newExpr.Members != null)
                                {
                                    for (var i = 0; eq && i < newExpr.Members.Count; ++i)
                                    {
                                        eq = newExpr.Members[i].Equals(newOther.Members[i]);
                                    }
                                }
                            }
                            break;
                        case ExpressionType.NewArrayInit:
                            {
                                var init = (NewArrayExpression)expr;
                                var initOther = (NewArrayExpression)other;
                                eq = init.Expressions.Count == initOther.Expressions.Count;
                                for (var i = 0; eq && i < init.Expressions.Count; ++i)
                                {
                                    eq = init.Expressions[i].DeepEquals(initOther.Expressions[i]);
                                }

                            }
                            break;
                        case ExpressionType.Parameter:
                            {
                                var parameter = (ParameterExpression)expr;
                                var parameterOther = (ParameterExpression)other;
                                // NOTE: This assumes that all parameters of the same type are the same. 
                                // This is so you can use different names for the parameter across expressions.
                                eq = parameter.Type == parameterOther.Type;
                            }
                            break;
                        default:
                            {
                                if (expr is UnaryExpression unary && other is UnaryExpression unaryOther)
                                {
                                    eq = unary.Operand.DeepEquals(unaryOther.Operand);
                                }
                                else
                                {
                                    if (expr is BinaryExpression binary && other is BinaryExpression binaryOther)
                                    {
                                        eq = binary.Left.DeepEquals(binaryOther.Left)
                                          && binary.Right.DeepEquals(binaryOther.Right);
                                    }
                                    else
                                    {
                                        throw new ArgumentException($"{expr.NodeType} is not handled");
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            return eq;
        }

        public static bool DeepEquals(this MemberInitExpression init, MemberInitExpression other)
        {
            var eq =
                init.Bindings.Count == other.Bindings.Count
                && init.NewExpression.DeepEquals(other.NewExpression);
            for (var i = 0; eq && i < init.Bindings.Count; ++i)
            {
                eq = init.Bindings[i].DeepEquals(other.Bindings[i]);
            }
            return eq;
        }

        public static bool DeepEquals(this MemberBinding binding, MemberBinding bindingOther)
        {
            var eq = binding.Member.Equals(bindingOther.Member)
                 && binding.BindingType.Equals(bindingOther.BindingType);
            if (eq)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        {
                            var assignment = (MemberAssignment)binding;
                            var assignmentOther = (MemberAssignment)bindingOther;
                            eq = assignment.Expression.DeepEquals(assignmentOther.Expression);
                        }
                        break;
                    case MemberBindingType.ListBinding:
                        {
                            var list = (MemberListBinding)binding;
                            var listOther = (MemberListBinding)bindingOther;
                            eq = list.Initializers.Count == listOther.Initializers.Count;
                            for (var j = 0; eq && j < list.Initializers.Count; ++j)
                            {
                                var initializer = list.Initializers[j];
                                var initializerOther = listOther.Initializers[j];
                                eq = initializer.Arguments.Count == initializerOther.Arguments.Count;
                                for (var k = 0; eq && k < initializer.Arguments.Count; ++k)
                                {
                                    eq = initializer.Arguments[k].DeepEquals(initializerOther.Arguments[k]);
                                }
                            }
                        }
                        break;
                    case MemberBindingType.MemberBinding:
                        {
                            eq = binding.DeepEquals(bindingOther);
                        }
                        break;
                }
            }
            return eq;
        }
    }
}
