using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Runtime.Tests.Plugins.TestComponents
{
    public class AdventureWorksAdapterOptions
    {
        public string AdventureWorksSkillId { get; set; }

        public string AdventureWorksSecret { get; set; }
    }
}
