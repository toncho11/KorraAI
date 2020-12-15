using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Companion.KorraAI;

namespace Companion.KorraAI.Models
{
    public class ModelContext
    {
        public IItemsLoader Items;
        public IBasePhrases BasePhrases;
        //public IExtendedPhrases ExtendedPhrases;
        public ISpeechAdaptation SpeechAdaptation;
    }
}
