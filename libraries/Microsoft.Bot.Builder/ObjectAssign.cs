using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    public static class Object
    {
        /// <summary>
        /// Equivalent to javascripts Object.Assign, creates a new object from startObject overlaying any non-null values from the overlay object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startObject">intial object</param>
        /// <param name="overlayObject">overlay object</param>
        /// <returns>merged object</returns>
        public static T Merge<T>(this T startObject, T overlayObject)
            where T : class
        {
            return Object.Assign<T>(startObject, overlayObject);
        }


        /// <summary>
        /// Equivalent to javascripts Object.Assign, creates a new object from startObject overlaying any non-null values from the overlay object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startObject">intial object of any type</param>
        /// <param name="overlayObject">overlay object of any type</param>
        /// <returns>merged object</returns>
        public static T Assign<T>(object startObject, object overlayObject)
            where T : class
        {
            return (T)Assign(startObject, overlayObject, typeof(T));
        }

        public static object Assign(object startObject, object overlayObject, Type type)
        {
            if (startObject != null && overlayObject != null)
            {
                // make a deep clone JObject of the startObject
                JObject jsMerged = (startObject is JObject) ? (JObject)(startObject as JObject).DeepClone() : JObject.FromObject(startObject);

                // get a JObject of the overlay object
                JObject jsOverlay = (overlayObject is JObject) ? (overlayObject as JObject) : JObject.FromObject(overlayObject);

                jsMerged.Merge(jsOverlay, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                });

                return jsMerged.ToObject(type);
            }

            var singleObject = startObject ?? overlayObject;
            if (singleObject != null)
            {
                if (singleObject is JObject)
                {
                    return (singleObject as JObject).ToObject(type);
                }

                return singleObject;
            }

            return (Type)Activator.CreateInstance(type);
        }
    }

}
