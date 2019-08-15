using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    public class OrdinalEntityRecognizer : BaseEntityRecognizer
    {
        public OrdinalEntityRecognizer()
        {
        }

        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return NumberRecognizer.RecognizeOrdinal(text, culture);
        }
    }
}
