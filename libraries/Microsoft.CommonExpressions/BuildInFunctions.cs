using System;

namespace Microsoft.Expressions
{
    public static class BuildinFunctions
    {
        public static EvaluationDelegate Add = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 + double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 + int1) :
                operands[0] is string string0 && operands[1] is string string1 ? string0 + string1 :
                throw new Exception();

        public static EvaluationDelegate Sub = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 - double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 - int1) :
                throw new Exception();

        public static EvaluationDelegate Mul = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 * double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 * int1) :
                throw new Exception();


        public static EvaluationDelegate Div = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 / double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 / int1) :
                throw new Exception();

        public static EvaluationDelegate Equal = operands => 
                operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? 
                operand0.CompareTo(operand1) == 0 : operands[0] == null && operands[1] == null ? 
                true : (operands[0] == null || operands[1] == null) ? 
                false : throw new Exception();

        public static EvaluationDelegate NotEqual = operands =>
                operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                operand0.CompareTo(operand1) != 0 : throw new Exception();

        public static EvaluationDelegate Min = parameters =>
                         parameters[0] is IComparable c0 && parameters[1] is IComparable c1 ? (c0.CompareTo(c1) < 0 ? c0 : c1) :
                         throw new Exception();

        public static EvaluationDelegate Max = parameters =>
                         parameters[0] is IComparable c0 && parameters[1] is IComparable c1 ? (c0.CompareTo(c1) > 0 ? c0 : c1) :
                         throw new Exception();

        public static EvaluationDelegate LessThan = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) < 0 : throw new Exception();

        public static EvaluationDelegate LessThanOrEqual = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) <= 0 : throw new Exception();

        public static EvaluationDelegate GreaterThan = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) > 0 : throw new Exception();

        public static EvaluationDelegate GreaterThanOrEqual = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) >= 0 : throw new Exception();



    }
}
