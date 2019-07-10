using System.Collections;

namespace Microsoft.Bot.Builder.Adapters.WeChat
{
    public class DictionarySort : IComparer
    {
        public int Compare(object oLeft, object oRight)
        {
            var leftString = oLeft as string;
            var rightString = oRight as string;
            var leftLength = leftString.Length;
            var rightLength = rightString.Length;
            var index = 0;
            while (index < leftLength && index < rightLength)
            {
                if (leftString[index] < rightString[index])
                {
                    return -1;
                }
                else if (leftString[index] > rightString[index])
                {
                    return 1;
                }
                else
                {
                    index++;
                }
            }

            return leftLength - rightLength;
        }
    }
}
