using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A lower case naming strategy.
    /// </summary>
    internal class LowerCaseNamingStrategy : NamingStrategy
    {
        internal LowerCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
        {
            ProcessDictionaryKeys = processDictionaryKeys;
            OverrideSpecifiedNames = overrideSpecifiedNames;
        }

        internal LowerCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
            : this(processDictionaryKeys, overrideSpecifiedNames)
        {
            ProcessExtensionDataNames = processExtensionDataNames;
        }

        internal LowerCaseNamingStrategy()
        {
        }

        protected override string ResolvePropertyName(string name)
        {
            return name.ToLower();
        }
    }
}
