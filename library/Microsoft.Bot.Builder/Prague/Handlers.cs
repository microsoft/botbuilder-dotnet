using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Prague
{
    public interface IHandler
    {
        void Execute();
    }

    public class SimpleHandler : IHandler
    {
        Action _action;

        public SimpleHandler(Action action)
        {
            _action = action ?? throw new ArgumentNullException("action");
        }
        public void Execute()
        {
            _action();
        }

        public static SimpleHandler Create(Action a)
        {
            return new SimpleHandler(a);
        }
    }
}
