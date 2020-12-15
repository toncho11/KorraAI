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

        void Init(DateTime? LastSessionDateTime);

        event EventHandler ContextLoaded;

        //event EventHandler Surprise; //if triggered the bot will express suprise

        IKorraAISampler GetSampler();

        ModelContext GetContext();

        IBaseDistributions GetCognitiveDist();

        /// <summary>
        /// Allows to change behavior based on: the time that has passed or if a pure or uncertain fact has been updated (by for example user's response)
        /// </summary>
        /// <param name="timeSinceStart"></param>
        /// <param name="pureFactUpdated"></param>
        /// <param name="uncertainFactUpdated"></param>
        /// <returns></returns>
        bool ModelUpdate(TimeSpan timeSinceStart, bool pureFactUpdated, bool uncertainFactUpdated);

        /// <summary>
        /// It gives you access to all upcoming interactions
        /// Here you can insert a new interaction or modify an existing one
        /// </summary>
        /// <param name="interactions"></param>
        void InteractionsUpdate(TimeSpan timeSinceStart, int interactionsDoneSinceStart, ref Queue<CommItem> interactions);

        /// <summary>
        /// Here you can modify the response to the user (for special cases)
        /// </summary>
        /// <param name="fact"></param>
        void BeforeAnalyseUserResponse(PureFact fact);

        /// <summary>
        /// Inspect the next interaction
        /// </summary>
        /// <param name="nextInteraction"></param>
        void InspectNextInteraction(CommItem nextInteraction);

        /// <summary>
        /// Describes how distributions change while performing sampling and a certain category is depleted from items
        /// </summary>
        /// <returns></returns>
        bool AdjustProbVariablesDuringPlanning(int bufferedInteractionsCount);

        IModelTrigger[] GetModelTriggers { get; }

        bool SmileOnNoReactionToUserResponse
        {
            get;
        }

        void SetFacialExpression(CommItem item);

        void FilterStoredFacts(ref List<Item> items);

        ItemManager[] ItemProviders
        {
            get;
        }
    }
}
