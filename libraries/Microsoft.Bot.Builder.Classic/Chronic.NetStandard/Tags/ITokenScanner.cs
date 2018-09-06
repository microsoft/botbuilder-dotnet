using System.Collections.Generic;

namespace Chronic
{
    public interface ITokenScanner
    {
        IList<Token> Scan(IList<Token> tokens, Options options);
    }
}