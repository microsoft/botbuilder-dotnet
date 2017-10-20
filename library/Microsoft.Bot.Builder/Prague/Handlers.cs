using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{    
    public class SimpleHandler : IHandler
    {
        public static implicit operator SimpleHandler(Action a)
        {
            return new SimpleHandler(a);
        }

        private readonly Func<Task> _userFunction;

        public SimpleHandler(Func<Task> function)
        {
            _userFunction = function ?? throw new ArgumentNullException(nameof(function));
        }

        public SimpleHandler(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action)); 

            // This is an Anti-Pattern, wrapping sync code in async code. 
            // However, w/o this, the simple stuff (which is what this class is about) means
            // constantly having to add "async" everything even for "a=b" type code. 
            _userFunction = async () => { action(); };                        
        }

        public Task Execute()
        {
            return _userFunction();
        }

        public static IHandler Create(Func<Task> a)
        {
            return new SimpleHandler(a);
        }
        public static IHandler Create(Action a)
        {
            return new SimpleHandler(a);
        }
    }
}
