using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    /// <summary>
    /// Collects information about how many interactions were missing when required by the sampling
    /// Calculates the time a new interaction queue will take.
    /// </summary>
    public class InteractionsStat
    {
        /// <summary>
        /// CategoryName and CategoryName_missing, number of interactions for this category
        /// </summary>
        static Dictionary<string, int> CategoryNInter = new Dictionary<string, int>();

        /// <summary>
        /// Category, Average time, Count
        /// </summary>
        static List<Tuple<string, float, int>> averageTimeOfInteractionPerCategory = new List<Tuple<string, float, int>>();
        //TODO: save information persistently between sessions 

        /// <summary>
        /// Based on last statistics it calculates the time the current interaction queue will take
        /// </summary>
        /// <returns>Time in seconds</returns>
        public static double TimeForecast()
        {
            // it should be run only if there are already some statistics about how much an interaction takes time

            float time = 0;

            foreach (var category in CategoryNInter)
            {
                if (averageTimeOfInteractionPerCategory.Exists(x => x.Item1 == category.Key))
                {
                    var categoryStat = averageTimeOfInteractionPerCategory.Find(x => x.Item1 == category.Key);

                    // N interactions scheduled * ( Average time per category + Average pause time)
                    time = time + category.Value * (categoryStat.Item2 + AveragePauseTime() );
                }
                else
                {
                    SharedHelper.Log("Unacounted category in TimeForecast'" + category + "'");
                }
            }

            // TODO: consider that reaction interactions take less pause time
            // TODO: reactions are not acounted (category "MakeStatement" for example)
            
            return time;
        }

        public static bool IsAvailable()
        {
            return averageTimeOfInteractionPerCategory.Count > 0;
        }

        public static int TotalMissingInteractions()
        {
            int count = 0;

            foreach (var category in CategoryNInter)
            {
                if (category.Key.EndsWith("_missing"))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Reset statistics before regeneration
        /// </summary>
        public static void Reset()
        {
            CategoryNInter = new Dictionary<string, int>();
        }

        public static void AddScheduledInteraction(string category)
        {
            if (CategoryNInter.ContainsKey(category))
                CategoryNInter[category]++;
            else CategoryNInter[category] = 0;
        }

        public static void AddMissingInteraction(string category)
        {
            if (CategoryNInter.ContainsKey(category + "_missing"))
                CategoryNInter[category + "_missing"]++;
            else CategoryNInter[category + "_missing"] = 0;
        }

        public static void AddInteractionTimeElapsed(string category, float time)
        {
            bool categoryExists = averageTimeOfInteractionPerCategory.Exists(x => x.Item1 == category);

            if (!categoryExists)
                averageTimeOfInteractionPerCategory.Add(new Tuple<string, float, int>(category, time, 1));
            else
            {
                var itemOld = averageTimeOfInteractionPerCategory.First(x => x.Item1 == category);

                //Calculate Incremental Averaging : https://math.stackexchange.com/questions/106700/incremental-averageing
                Tuple<string, float, int> itemNew = new Tuple<string, float, int>(category, itemOld.Item2 + (time - itemOld.Item2) / (itemOld.Item3 + 1), itemOld.Item3 + 1);

                averageTimeOfInteractionPerCategory.Remove(itemOld);
                averageTimeOfInteractionPerCategory.Add(itemNew);
            }

            SharedHelper.Log("The elapsed time of an interaction of type '" + category + "' is: " + time);
        }

        private static float AveragePauseTime()
        {
            //TODO: take automatically the mean and variance of the pause distribution
            float mean = 3.7f; //Pause time = Normal(mean, variance) = (3.7, 0.25)
            return mean;
        }

        public static void PrintSummary()
        {
            string result = "";
            int i = 0;
            foreach (var key in CategoryNInter.Keys)
            {
                i++;
                result += "|" + key + ": " + CategoryNInter[key] + Environment.NewLine;
            }
            SharedHelper.LogWarning("Interactions statistics: " + Environment.NewLine + result);

            TimeSpan time = TimeSpan.FromSeconds(TimeForecast());
            SharedHelper.Log("Time (hh:mm) forecast scheduled interactions will take: " + time.ToString(@"hh\:mm"));
        }
    }
}
