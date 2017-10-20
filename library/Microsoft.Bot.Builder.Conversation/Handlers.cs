using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Conversation
{    
    public class Handler : RouterOrHandler
    {
        private readonly Func<Task> _userFunction;

        public Handler(Func<Task> function)
        {
            _userFunction = function ?? throw new ArgumentNullException(nameof(function));
        }

        public Handler(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action)); 

            _userFunction = async () => { action(); };                        
        }

        public Task Execute()
        {
            return _userFunction();
        }
    }
}
