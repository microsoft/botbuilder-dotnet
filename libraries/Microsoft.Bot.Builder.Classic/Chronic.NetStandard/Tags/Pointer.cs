namespace Chronic
{
    public class Pointer : Tag<Pointer.Type>
    {
        public Pointer(Type value) : base(value)
        {
            
        }

        public enum Type
        {
            Past = -1,
            Future = 1,
            None = 0
        }
    }
}