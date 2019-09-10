using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Bot.Builder.GrammarChecker.SyntaxFeatures;

namespace Microsoft.Bot.Builder.GrammarChecker.SyntaxnetModel
{
    public class SyntaxnetPosTagger : IPosTagger
    {
        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "InitializeSyntaxnet", CharSet = CharSet.Unicode)]
        private static extern bool InitializeSyntaxnet(IntPtr pSyntax, [MarshalAs(UnmanagedType.LPStr)] string strModelDirectory);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "CreateSyntaxnet")]
        private static extern IntPtr CreateSyntaxnet();

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "DeleteSyntaxnet")]
        private static extern void DeleteSyntax(IntPtr pSyntax);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "DoPosTagging", CharSet = CharSet.Unicode)]
        private static extern IntPtr DoPosTagging(
            IntPtr pSyntax,
            [MarshalAs(UnmanagedType.LPStr)] string strInputSentence);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "DoDependencyParsing", CharSet = CharSet.Unicode)]
        private static extern IntPtr DoDependencyParsing(
            IntPtr pSyntax,
            [MarshalAs(UnmanagedType.LPStr)] string strInputSentence);

        [DllImport(@"Lib\syntaxnet.dll", EntryPoint = "FreeSyntaxResult")]
        private static extern void FreeSyntaxResult(IntPtr pResult);

        private IntPtr pSyntax;

        public SyntaxnetPosTagger(string path = "")
        {
            this.pSyntax = CreateSyntaxnet();
            string modelDirectory = Path.Combine(path, "SyntaxnetModel/ModelData").Replace(@"\", "/");

            InitializeSyntaxnet(this.pSyntax, modelDirectory);
        }

        ~SyntaxnetPosTagger()
        {
            DeleteSyntaxnet();
        }

        public List<PosFeature> PosTagging(string sentence)
        {
            if (this.pSyntax == IntPtr.Zero)
            {
                throw new Exception("Syntaxnet model engine is null");
            }
            
            var responsePtr = DoPosTagging(this.pSyntax, sentence);
            if (responsePtr != IntPtr.Zero)
            {
                var posStr = Marshal.PtrToStringAnsi(responsePtr);
                var tags = posStr.Split('\n').ToList();
                FreeSyntaxResult(responsePtr);
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
            else
            {
                throw new Exception("Syntaxnet POS tagging failed");
            }
        }

        private void DeleteSyntaxnet()
        {
            if (this.pSyntax != IntPtr.Zero)
            {
                DeleteSyntax(this.pSyntax);
                this.pSyntax = IntPtr.Zero;
            }
        }
    }
}
