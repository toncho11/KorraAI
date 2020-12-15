using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    public interface ISpeechAdaptation
    {
        Queue<CommItem> ProcessItems(Queue<CommItem> input, ItemManager[] managers);
    }
}
