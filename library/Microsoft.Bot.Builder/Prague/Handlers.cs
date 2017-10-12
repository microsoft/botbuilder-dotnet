using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Prague
{    
    public class SimpleHandler : IHandler
    {
        Func<Task> _action;

        public SimpleHandler(Func<Task> action)
        {
            _action = action ?? throw new ArgumentNullException("action");
        }

        public SimpleHandler(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            // This is an Anti-Pattern, wrapping sync code in async code. 
            // However, w/o this, the simple stuff (which is what this class is about) means
            // constantly having to add "async" everything even for "a=b" type code. 
            _action = async () => { action(); };                        
        }

        public Task Execute()
        {
            return _action();
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
