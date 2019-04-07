using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public static class Policy
    {
        public static string NameFor(object item) => item.GetType().Name;
        public static bool ShowToDebugger(object value)
        {
            if (value == null)
            {
                return true;
            }

            var type = value.GetType();
            return type != typeof(CancellationToken);
        }
        public static bool ShowAsScalar(object value)
        {
            if (value == null)
            {
                return true;
            }

            var type = value.GetType();
            if (type.IsPrimitive || type == typeof(string))
            {
                return true;
            }

            return false;
        }
        public static string ScalarJsonValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (ShowAsScalar(value))
            {
                return value.ToString();
            }

            if (value is ICollection collection)
            {
                return $"Count = {collection.Count}";
            }

            return null;
        }
    }
}
