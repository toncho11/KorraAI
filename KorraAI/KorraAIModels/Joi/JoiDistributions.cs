using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion.KorraAI.Models.Joi
{
    public class JoiDistributions : IDistributions
    {
        private static Queue<float> allInteractionPauses = new Queue<float>();
        private static Queue<float> allPausesSmiles = new Queue<float>();
        private static Queue<float> allQuestionTimeouts = new Queue<float>();
        private static Queue<float> allTimesEyesFocusedOnCameraAfterStartedTalking = new Queue<float>();
        private static Queue<int> allOutfitsChange = new Queue<int>();

        float IDistributions.GetNextInteactionPause()
        {
            if (allInteractionPauses.Count == 0)
            {

                var normalDist = from seconds in Normal(4.7, 1) //16, 4
                                 select seconds;

                string singleStringAllPauses = "";

                for (var i = 0; i < 101; i++)
                {
                    float value = (float)normalDist.Sample(); //sampling
                    singleStringAllPauses = singleStringAllPauses + "," + value;
                    allInteractionPauses.Enqueue(value);
                }
            }

            return allInteractionPauses.Dequeue();
        }

        float IDistributions.NextQuestionTimeout()
        {
            if (allQuestionTimeouts.Count == 0)
            {
                var normalDist = from seconds in Normal(25, 4) //16, 4
                                 select seconds;

                for (var i = 0; i < 101; i++)
                {
                    float value = (float)normalDist.Sample(); //sampling
                    allQuestionTimeouts.Enqueue(value);
                }
            }

            return allQuestionTimeouts.Dequeue();
        }

        float IDistributions.NextSmilePauseTime()
        {
            if (allPausesSmiles.Count == 0)
            {
                var normalDist = from seconds in Normal(12, 3)
                                 select seconds;

                //string singleStringAllPauses = "";

                for (var i = 0; i < 101; i++)
                {
                    float value = (float)normalDist.Sample(); //sampling
                    //singleStringAllPauses = singleStringAllPauses + "," + value;
                    allPausesSmiles.Enqueue(value);
                }
                //SharedHelper.Log("singleStringAll Smile Pauses: " + singleStringAllPauses);
            }

            return allPausesSmiles.Dequeue();
        }

        float IDistributions.NextTimeDurationEyesStayFocusedOnCameraAfterStartedTalking()
        {
            if (allTimesEyesFocusedOnCameraAfterStartedTalking.Count == 0)
            {
                var normalDist = from seconds in Normal(7, 1.2)
                                 select seconds;

                string singleStringAllPauses = "";

                for (var i = 0; i < 101; i++)
                {
                    float value = (float)normalDist.Sample(); //sampling
                    singleStringAllPauses = singleStringAllPauses + "," + value;
                    allTimesEyesFocusedOnCameraAfterStartedTalking.Enqueue(value);
                }

                //SharedHelper.Log("singleStringAll EyesFocusedOnCamera: " + singleStringAllPauses);
            }

            return allTimesEyesFocusedOnCameraAfterStartedTalking.Dequeue();
        }

        public void ForceGenerateOutfits(int[] values, int lastOutfitUsed)
        {
            allOutfitsChange.Clear();

            int[] sequence = SharedHelper.GeneratePermutationArray(values, 18, lastOutfitUsed);

            for (int i = 0; i < sequence.Length; i++)
            {
                allOutfitsChange.Enqueue(sequence[i]);
            }

            #region debug
            string singleStringAllOutfitsChanges = "";
            for (var i = 0; i < allOutfitsChange.Count; i++)
            {
                singleStringAllOutfitsChanges += allOutfitsChange.ToArray()[i] + ",";
            }
            #endregion
            //UnityEngine.Debug.Log("singleStringAllOutfitsChanges:\r\n" + singleStringAllOutfitsChanges);
        }

        public int GetNextOutfit(int[] activeOutfitsIndexes, int lastOutfitUsed)
        {
            if (allOutfitsChange.Count == 0)
            {
                ForceGenerateOutfits(activeOutfitsIndexes, lastOutfitUsed);
            }

            return allOutfitsChange.Dequeue();
        }
    }
}
