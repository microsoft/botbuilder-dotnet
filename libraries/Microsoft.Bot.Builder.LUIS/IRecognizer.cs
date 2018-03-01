using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.LUIS
{
    public interface IRecognizer
    {
        Task<RecognizerResult> Recognize(string utterance, CancellationToken ct, bool verbose);
    }
}
