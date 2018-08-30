using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal enum SlotTypeEnum
    {
        StringType = 0,
        IntType = 1,
        FloatType = 2,
        BooleanType = 3,
        DateTimeType = 4,
        UnknownType = 5,
    }

    internal class Slot
    {
        public KeyValuePair<string, object> KeyValue { get; set; }
        public SlotTypeEnum Type
        {
            get
            {
                if (KeyValue.Value is string)
                {
                    return SlotTypeEnum.StringType;
                }

                if (KeyValue.Value is int)
                {
                    return SlotTypeEnum.IntType;
                }

                if (KeyValue.Value is float)
                {
                    return SlotTypeEnum.FloatType;
                }

                if (KeyValue.Value is bool)
                {
                    return SlotTypeEnum.BooleanType;
                }

                if (KeyValue.Value is DateTime)
                {
                    return SlotTypeEnum.DateTimeType;
                }

                else
                {
                    return SlotTypeEnum.UnknownType;
                }
            }
        }
    }
}
