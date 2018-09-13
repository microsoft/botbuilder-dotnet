using System;

namespace Chronic.Tags.Repeaters
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException(string message) 
            : base(message)
        {
            
        }
    }
}