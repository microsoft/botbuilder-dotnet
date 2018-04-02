using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Rest.TransientFaultHandling;

namespace Microsoft.Bot.Builder.Adapters
{
    /// <summary>
    /// ASP.Net WebApi specific functions for BotFrameworkAdapter.
    /// </summary>
    public partial class BotFrameworkAdapter
    {
        /// <summary>
        /// Gets the authentication header.
        /// </summary>
        /// <returns>Authentication header if present, null otherwise.</returns>
        public string GetAuthenticationHeader()
        {
            try
            {
                return HttpContext.Current.Request.Headers["Authorization"];
            }
            catch
            {
                return null;
            }
        }
    }
}
