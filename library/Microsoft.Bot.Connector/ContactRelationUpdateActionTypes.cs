using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public static class ContactRelationUpdateActionTypes
    {
        /// <summary>
        /// Bot added to user contacts
        /// </summary>
        public const string Add = "add";

        /// <summary>
        /// Bot removed from user contacts
        /// </summary>
        public const string Remove = "remove";
    }
}
