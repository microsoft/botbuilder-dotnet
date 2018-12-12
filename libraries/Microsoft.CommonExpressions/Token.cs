namespace Microsoft.Expressions
{
    /// <summary>
    /// Token from lexical analysis.
    /// </summary>
    public class Token
    {
        public string Input { get; private set; }
        public object Value { get; private set; }
        public override string ToString() => Input;
        public static Token From(string input, object value) => new Token() { Input = input, Value = value };
    }
}
