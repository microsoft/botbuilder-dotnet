using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker.Tests
{
    public class MockPosTagger : IPosTagger
    {
        public List<PosFeature> PosTagging(string sentence)
        {
            var posStr = MockData.SyntaxDict[sentence];
            var tags = posStr.Split('\n').ToList();
            var posFeatures = new List<PosFeature>();
            foreach (var tag in tags)
            {
                var features = tag.Split('\t');
                var posFeature = new PosFeature();

                int wordIdx = -1;
                if (int.TryParse(features[0], out wordIdx))
                {
                    posFeature.WordIndex = wordIdx - 1;
                }
                else
                {
                    break;
                }

                posFeature.WordText = features[1];

                var posTagStr = features[3];
                BasicPosTag posTag;
                if (Enum.TryParse(posTagStr, true, out posTag))
                {
                    posFeature.BasicPosTag = posTag;
                }
                else
                {
                    posFeature.OtherBasicTag = posTagStr;
                }

                var subTagStr = features[4];
                VerbPosTag verbTag;
                AdjectivePosTag adjTag;
                NounPosTag nounTag;
                PronPosTag pronTag;
                NumPosTag numTag;

                if (Enum.TryParse(subTagStr, true, out verbTag))
                {
                    posFeature.VerbPosTag = verbTag;
                }
                else if (Enum.TryParse(subTagStr, true, out adjTag))
                {
                    posFeature.AdjPosTag = adjTag;
                }
                else if (Enum.TryParse(subTagStr, true, out nounTag))
                {
                    posFeature.NounPosTag = nounTag;
                }
                else if (Enum.TryParse(subTagStr, true, out pronTag))
                {
                    posFeature.PronPosTag = pronTag;
                }
                else if (Enum.TryParse(subTagStr, true, out numTag))
                {
                    posFeature.NumPosTag = numTag;
                }
                else
                {
                    posFeature.OtherSubTag = subTagStr;
                }

                posFeatures.Add(posFeature);
            }

            return posFeatures;
        }
    }
}
