using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    public static partial class Extensions
    {
        public static RelationshipType Swap(this RelationshipType original)
        {
            var relationship = original;
            switch (original)
            {
                case RelationshipType.Specializes:
                    relationship = RelationshipType.Generalizes;
                    break;
                case RelationshipType.Generalizes:
                    relationship = RelationshipType.Specializes;
                    break;
            }
            return relationship;
        }
    }
}
