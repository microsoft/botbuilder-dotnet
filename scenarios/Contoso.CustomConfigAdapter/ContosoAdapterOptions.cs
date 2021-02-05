using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Contoso.CustomConfigAdapter
{
    public class ContosoAdapterOptions
    { 
        public string ContosoSkillId { get; set; }

        public string ContosoSecretKey { get; set; }

        public bool AllowSuggestedActions { get; set; } = true;

        public string ApiPath { get; set; } = "contoso";
    }
}
