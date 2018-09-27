using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// Enum represents the slot value type.
    /// </summary>
    internal enum SlotTypeEnum
    {
        StringType = 0,
        IntType = 1,
        FloatType = 2,
        BooleanType = 3,
        DateTimeType = 4,
        UnknownType = 5,
    }

    /// <summary>
    /// Slot class is the object carrying the entity values that will be passed to the service to substitute entity references in template resolutions, and infers entity type.
    /// example :
    /// #TemplateHelloUser
    ///     - Hey there {userName}
    ///     - How are you Today {userName}
    ///  a user can pass a slot like :
    ///  Slot userNameSlot = new Slot
    ///  {
    ///     KeyValue = new KeyValuePair<string, object>("userName", "Amr");
    ///  };
    ///  
    /// then language generation resolution service would evaluate the template [TemplateHelloUser] when referenced to one ofe the following values :
    ///     - Hey there Amr
    ///     - How are you Today Amr
    /// </summary>
    internal class Slot
    {
        /// <summary>
        /// KeyValue pair property that will contain entity key and entity value.
        /// </summary>
        public KeyValuePair<string, object> KeyValue { get; set; }

        /// <summary>
        /// Type property to return entity/slot value data type.
        /// </summary>
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
