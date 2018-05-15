using System;
using System.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AspNetCore_EchoBot_With_State
{
    public class IndexModel : PageModel
    {
        private IOptions<ApplicationConfiguration> _OptionsApplicationConfiguration;

        public string DebugLink { get; private set; }

        public string EmulatorDeepLink { get; private set; }

        public IndexModel(IOptions<ApplicationConfiguration> OptionsApplicationConfiguration)
        {
            this._OptionsApplicationConfiguration = OptionsApplicationConfiguration;
        }

        public void OnGet()
        {
            string botUrl = $"{ Request.Scheme }://{ Request.Host }/api/messages";
            DebugLink = botUrl;

            // construct emulator protocol URI
            string protocolUri = $"bfemulator://livechat.open?botUrl={ HttpUtility.UrlEncode(botUrl) }";
            string msaAppId = this._OptionsApplicationConfiguration.Value.MicrosoftAppId;
            string msaAppPw = this._OptionsApplicationConfiguration.Value.MicrosoftAppPassword;

            if (!String.IsNullOrEmpty(msaAppId))
            {
                protocolUri += $"&msaAppId={ HttpUtility.UrlEncode(msaAppId) }";
            }
            if (!String.IsNullOrEmpty(msaAppPw))
            {
                protocolUri += $"&msaPassword={ HttpUtility.UrlEncode(msaAppPw) }";
            }
            EmulatorDeepLink = protocolUri;
        }
    }
}
