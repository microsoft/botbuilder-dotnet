using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Contoso.CustomConfigAdapter
{
    public class ContosoController : AdapterControllerBase<ContosoAdapter>
    {
        public ContosoController(ContosoAdapter adapter, IBot bot)
            : base(adapter, bot)
        {
        }
    }
}
