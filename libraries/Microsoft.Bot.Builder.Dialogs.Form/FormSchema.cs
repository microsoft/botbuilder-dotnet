using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormSchema : JObject
    {
        public FormSchema(JObject schema)
            : base(schema)
        {
        }

        public FormSchema(string schema)
            : base(JsonConvert.DeserializeObject(schema))
        {
        }
    }
}
