using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillHost.Controllers
{
    public class ChannelAPIArgs
    {
        public ChannelAPIMethod Method { get; set; }

        public object[] Args { get; set; }

        public object Result { get; set; }
    }
}
