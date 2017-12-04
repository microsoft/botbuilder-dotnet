using ImpromptuInterface;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace AlarmBot
{
    public static class FlexObjectExtensions
    {
        /// <summary>
        /// Project an interface view onto a FlexObject using the impromptu library.  
        /// </summary>
        /// <typeparam name="T">Interface</typeparam>
        /// <param name="flex">FlexObject</param>
        /// <returns></returns>
        public static T As<T>(this FlexObject flex) where T : class
        {
            dynamic x = flex;
            T result = Impromptu.ActLike(x);
            return result;
        }
    }
}
