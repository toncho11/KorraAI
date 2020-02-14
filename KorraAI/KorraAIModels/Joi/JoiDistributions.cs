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
        private static Queue<int>   allOutfitsChange = new Queue<int>();
        private static Queue<int>   allSmileVersions = new Queue<int>();

        private static Random pauseUniformAfterReaction = new Random(Guid.NewGuid().GetHashCode());

        //TODO: addt the ability the N distribution to be changed over time
        float IDistributions.GetNextInteactionPause(bool isReacting)
        {
            if (!isReacting)
            {
                //SharedHelper.Log("Pause time set following default distribution.");

                if (allInteractionPauses.Count == 0)
                {
                    var normalDist = from seconds in Normal(3.7, 0.25) //default 4.7, 1
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
            else //if it is a response, we try to react quickly
            {
                float value = pauseUniformAfterReaction.Next(1, 2) / 10f;
                SharedHelper.Log("Pause time set as a reaction to question: " + value);
                return value;
            }
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
            //SharedHelper.Log("singleStringAllOutfitsChanges:\r\n" + singleStringAllOutfitsChanges);
        }

        public int GetNextOutfit(int[] activeOutfitsIndexes, int lastOutfitUsed)
        {
            if (allOutfitsChange.Count == 0)
            {
                ForceGenerateOutfits(activeOutfitsIndexes, lastOutfitUsed);
            }

            return allOutfitsChange.Dequeue();
        }

        public PrimitiveDist<string> JokesDistribution(PureFact factEasilyOffended, bool romanticJokesFirst, bool romanticJokesLast)
        {
            var allJokes = from joke in JokesProvider.GetAll()
                           where joke.IsUsed == false && joke.IsPlanned == false
                           select joke;

            bool isUserEasilyOffended = (factEasilyOffended == null) || !factEasilyOffended.IsAnswered || (factEasilyOffended.IsAnswered && factEasilyOffended.Value == "Yes"); //NOT GOOD !!!!!!

            List<ItemProb<string>> itemProbs = new List<ItemProb<string>>();

            if (romanticJokesFirst && romanticJokesLast)
            {
                SharedHelper.LogError("Error generating Jokes Distribution: both 'romanticJokesFirst' and 'romanticJokesLast' are true");
            }

            if (allJokes.Count() == 0) return null;

            //assign probabilities 
            foreach (Joke j in allJokes)
            {
                float prob = 0.5f;

                if (j.ContentType != null)
                {
                    //romantic jokes will have a priority in the beginning
                    if (romanticJokesFirst)
                    {
                        if (j.ContentType.IsMildSexual || j.ContentType.IsRomanticReferrence)
                            prob = 0.8f;
                    }

                    //romatic jokes will be dealyed
                    if (romanticJokesLast)
                    {
                        if (j.ContentType.IsMildSexual || j.ContentType.IsRomanticReferrence)
                            prob = 0.3f;
                    }

                    //offensive jokes will be delayed in time
                    if (isUserEasilyOffended && j.ContentType.IsMildOffensive) //delay the mildly offensive jokes 
                    {
                        prob = 0.2f;
                    }

                    #region disabled
                    if (!Personality.UseMildOffensiveJokes && j.ContentType.IsMildOffensive)
                    {
                        prob = 0f;
                    }

                    if (!Personality.UseMildSexualThemes && j.ContentType.IsMildSexual)
                    {
                        prob = 0f;
                    }

                    if (!Personality.UseRomanticReferences && j.ContentType.IsRomanticReferrence)
                    {
                        prob = 0f;
                    }
                    #endregion
                }

                itemProbs.Add(new ItemProb<string>(j.Name, Prob(prob)));
            }

            var jokestDistF = CategoricalF(itemProbs.ToArray()).Normalize();

            //SharedHelper.Log("Jokes histogram:\r\n" + jokestDistF.Histogram());

            var jokesDist = jokestDistF.ToSampleDist();

            return jokesDist;
        }

        public int NextSmileVersion()
        {
            if (allSmileVersions.Count == 0)
            {
                Random r = new Random();

                for (var i = 0; i < 101; i++)
                {
                    int smileVersion = r.Next(2);

                    int[] arrSmileVersion = allSmileVersions.ToArray();
                    int length = allSmileVersions.Count;

                    if (allSmileVersions.Count > 2 && arrSmileVersion[length - 2] == smileVersion
                        && arrSmileVersion[length - 1] == smileVersion)
                    {
                        continue;
                    }
                    else allSmileVersions.Enqueue(smileVersion);
                }

                //string singleStringAllSmiles = "";

                //for (var i = 0; i < allSmileVersions.Count; i++)
                //{
                //    singleStringAllSmiles += allSmileVersions.ToArray()[i].ToString() + "\r\n";
                //}
                //SharedHelper.LogError("Smile sequence: " + singleStringAllSmiles);
            }

            return allSmileVersions.Dequeue();
        }
    }
}
