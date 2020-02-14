using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    /// <summary>
    /// Model is updated and evaluated, inference is performed and as a consequence:
    /// - a re-sampling is performed
    /// - new interactions are added 
    /// </summary>
    public interface IModelTrigger
    {
        bool IsTimeBased { get; }
        bool IsUserResponseBased { get; }
        bool IsOneTimeTrigger { get; }

        /// <summary>
        /// How many times the trigger has been activated
        /// </summary>
        int  TriggeredCount { get; }
    }

    public struct ModelUpdateTriggerReturn
    {
        public bool IsTriggered;

        public bool IsResamplingRequired;

        /// <summary>
        /// Used by some triggers to return text
        /// </summary>
        public string Value;
    }

    interface IModelUpdateTrigger : IModelTrigger
    {
        /// <summary>
        /// Performs inference and updates probabilistic variables  
        /// </summary>
        /// <param name="timeElapsed"></param>
        /// <param name="model"></param>
        /// <returns>Is re-sampling required</returns>
        bool Process(bool isPureFactUpdated, TimeSpan timeElapsed, IKorraAIModel model);

    }

    interface IModelEvaluateTrigger : IModelTrigger
    {
        /// <summary>
        /// Evaluate a sub-model such as 'Surprise' and add a new interaction
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        CommItem? Process(IKorraAIModel model);
    }
}
