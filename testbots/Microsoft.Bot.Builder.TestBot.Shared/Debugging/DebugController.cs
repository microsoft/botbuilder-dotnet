using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Bot.Builder.TestBot.Shared.Debugging
{
    [Route("api/debug")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private DebugAdapter _adapter;
        private DebugBot _bot;

        public DebugController(DebugAdapter adapter, DebugBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
    }
}
