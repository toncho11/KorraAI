using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    public interface IKorraAIModel
    {
        string Name { get; }

        string Version { get; }

        void Init();

        event EventHandler ContextLoaded;

        KorraAISampler GetSampler();

        ModelContext GetContext();

        bool ModelUpdate(TimeSpan timeSinceStart);

        void BeforeAnalyseUserResponse(PureFact fact);
    }
}
