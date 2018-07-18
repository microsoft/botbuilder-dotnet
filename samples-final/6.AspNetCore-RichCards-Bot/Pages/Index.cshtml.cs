using System.IO;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Extensions.Options;

namespace AspNetCore_RichCards_Bot
{
    public class IndexModel : PageModel
    {
        private IHostingEnvironment _hostingEnv;
        private IOptions<BotFrameworkOptions> _options;

        public string DebugLink { get; private set; }

        public string EmulatorDeepLink { get; private set; }

        public IndexModel(IHostingEnvironment hostingEnv, IOptions<BotFrameworkOptions> options)
        {
            _hostingEnv = hostingEnv;
            _options = options;
        }

        public void OnGet()
        {
            string botUrl = $"{ Request.Scheme }://{ Request.Host }{ _options.Value.Paths.BasePath }{ _options.Value.Paths.MessagesPath }";
            DebugLink = botUrl;

            // construct emulator protocol URI
            string botFilePath = Path.Combine(_hostingEnv.ContentRootPath, "AspNetCore-RichCards-Bot.bot");
            string protocolUri = $"bfemulator://bot.open?path={ HttpUtility.UrlEncode(botFilePath) }&endpoint={ HttpUtility.UrlEncode(botUrl) }";
            EmulatorDeepLink = protocolUri;
        }
    }
}
