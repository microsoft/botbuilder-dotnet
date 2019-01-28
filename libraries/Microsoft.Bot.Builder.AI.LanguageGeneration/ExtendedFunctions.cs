using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Expressions;
using System.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class ExtendedFunctions
    {
        public static GetMethodDelegate ExtendedMethod = (string name) =>
        {
            switch (name)
            {
                case "count":
                    return ExtendedFunctions.Count;
                case "join":
                    return ExtendedFunctions.Join;
                default:
                    return MethodBinder.All(name);
            }
        };

        public static object Count(IReadOnlyList<object> parameters)
        {
            if (parameters[0] is IList li)
            {
                return li.Count;
            }
            throw new NotImplementedException();
        }

        public static object Join(IReadOnlyList<object> paramters)
        {
            if (paramters.Count == 2 && 
                paramters[0] is IList &&
                paramters[1] is String sep)
            {
                return String.Join(sep + " ", paramters[0]); // "," => ", " 
            }

            if (paramters.Count == 3 &&
                paramters[0] is IList li &&
                paramters[1] is String sep1 &&
                paramters[2] is String sep2)
            {
                sep1 = sep1 + " "; // "," => ", "
                sep2 = " " + sep2 + " "; // "and" => " and "

                if (li.Count < 3)
                {
                    return String.Join(sep2, li.OfType<object>().Select(x => x.ToString()));
                }
                else
                {
                    var firstPart = String.Join(sep1, li.OfType<object>().SkipLast(1));
                    return firstPart + sep2 + li.OfType<object>().Last().ToString();
                }
            }
            throw new NotImplementedException();
        }

    }

}
