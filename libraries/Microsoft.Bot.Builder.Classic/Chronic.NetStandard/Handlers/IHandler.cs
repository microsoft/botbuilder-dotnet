using System.Collections.Generic;
using Chronic.Handlers;

namespace Chronic.Handlers
{
    public interface IHandler
    {
        Span Handle(IList<Token> tokens, Options options);
    }



}