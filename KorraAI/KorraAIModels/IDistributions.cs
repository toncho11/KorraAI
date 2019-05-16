using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    public interface IDistributions
    {
        /// <summary>
        /// Returns the pause (time interval) between two interactions (of any type)
        /// </summary>
        /// <returns>the time in seconds</returns>
        float GetNextInteactionPause();

        /// <summary>
        /// Returns the pause (time interval) between two smiles
        /// </summary>
        /// <returns>the time in seconds</returns>
        float NextSmilePauseTime();

        /// <summary>
        /// Returns how much time the eyes will stay focused on the camera after started talking
        /// </summary>
        /// <returns></returns>
        float NextTimeDurationEyesStayFocusedOnCameraAfterStartedTalking();

        /// <summary>
        /// After what amount of time the question has timed out
        /// </summary>
        /// <returns></returns>
        float NextQuestionTimeout();

        /// <summary>
        /// Returns the next clothing outfit following a distribution or specific logic
        /// </summary>
        /// <param name="lastOutfitUsed"></param>
        /// <returns></returns>
        int GetNextOutfit(int[] activeOutfitsIndexes, int lastOutfitUsed);

        /// <summary>
        /// It does the actual job of generating a sequence of outfits, this method is called automatically from GetNextOutfit
        /// This method allows to add IDs of new outfits
        /// </summary>
        /// <param name="values"></param>
        /// <param name="lastOutfitUsed"></param>
        void ForceGenerateOutfits(int[] values, int lastOutfitUsed);
    }
}
