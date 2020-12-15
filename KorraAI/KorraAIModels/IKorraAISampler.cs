using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    public interface IKorraAISampler
    {
        void Init(ModelContext context, Func<int, bool> adjFunc,ItemManager[] providers);
        void ReGenerateMainSequence(ref Queue<CommItem> Interactions, ItemManager[] providers);
    }
}
