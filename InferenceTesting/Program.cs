using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace InferenceTest
{
    class Program
    {
        public static class ProbVariables
        {
            public class TVGIPEvent
            {
                public TVGIPEvent(bool p_LikesVideoGames, bool p_ThinksVideogameIsGoodPresent)
                {
                    LikesVideoGames = p_LikesVideoGames;
                    ThinksVideogameIsGoodPresent = p_ThinksVideogameIsGoodPresent;
                }

                public TVGIPEvent(bool p_LikesVideoGames)
                {
                    LikesVideoGames = p_LikesVideoGames;
                }

                //public int Age;
                //public bool IsMale;

                public bool LikesVideoGames;
                public bool ThinksVideogameIsGoodPresent;
            }

            public static FiniteDist<bool> LikesVideoGames = BernoulliF(Prob(0.4)); //average person

            public static FiniteDist<bool> ThinksVideoGameIsGoodPresent = BernoulliF(Prob(0.5)); //0.4 Average person

            /// <summary>
            /// If you like video games then you also think that a video game is a good present with probability of 70%
            /// </summary>
            public static FiniteDist<TVGIPEvent> VideoGameModel = from lvg in LikesVideoGames
                                                                  from tvgp in BernoulliF(Prob(lvg ? 0.7 : 0.3))
                                                                  select new TVGIPEvent(lvg, tvgp);
        }

        static void Main(string[] args)
        {
            int Age = 24;
            bool IsMale = true;

            bool UserLikesVideoGamesAnswered = true;
            bool UserLikesVideoGamesAnsweredYES = true;

            bool UserThinksVideoGameIsGoodPresentAnswered = true;
            bool UserThinksVideoGameIsGoodPresentYES = false;

            #region Set variables based on available information
            if (!UserLikesVideoGamesAnswered)
            {
                Func<int, bool, Prob> playsGames = (age, isMale) =>
                {
                    if (isMale && age > 11 && age < 28) return Prob(0.9);
                    if (!isMale && age > 11 && age < 28) return Prob(0.7);

                    if (isMale && age >= 28 && age < 45) return Prob(0.4);
                    if (!isMale && age >= 28 && age < 45) return Prob(0.3);

                    if (isMale && age >= 45) return Prob(0.1);
                    if (!isMale && age >= 45) return Prob(0.1);

                    return Prob(0.05);
                };

                var playsGamesModel = from pg in BernoulliF(playsGames(Age, IsMale))
                                      select new ProbVariables.TVGIPEvent(pg);

                ProbVariables.LikesVideoGames = BernoulliF(playsGamesModel.ProbOf(e => e.LikesVideoGames));
            }

            #endregion
            //=================================================================================================================================================

            //1. surprise because statistically people from this sex and age like computer games
            Console.WriteLine(ProbVariables.LikesVideoGames.ProbOf(e => e == true).Value);
            if (ProbVariables.LikesVideoGames.ProbOf(e => e == true).Value > 0.7 && !UserLikesVideoGamesAnsweredYES)
            {
                Console.WriteLine("S1: I thought you would like computer games.");
            }

            //2. Surprise
            //Thinks games are good present - YES
            //Likes games in general - NO
            //surprise: likes games rendered more than 70%, but still answered NO bu user
            //backward inference
            if (!UserLikesVideoGamesAnsweredYES &&
                  UserThinksVideoGameIsGoodPresentAnswered && UserThinksVideoGameIsGoodPresentYES)
            {
                var conditionThinksVideogameIsGoodPresent = ProbVariables.VideoGameModel.ConditionHard(e => e.ThinksVideogameIsGoodPresent);

                double probLikesGamesConditionedOnThinksGamesAreGoodPresent = conditionThinksVideogameIsGoodPresent.ProbOf(e => e.LikesVideoGames).Value;


                Console.WriteLine("probLikesGamesConditionedOnThinksGamesAreGoodPresent: " + probLikesGamesConditionedOnThinksGamesAreGoodPresent);
                Console.WriteLine("probLikesVideoGames: " + ProbVariables.LikesVideoGames.ProbOf(e => e == true));

                //2.
                if (probLikesGamesConditionedOnThinksGamesAreGoodPresent > 0.6)
                {
                    //Console.WriteLine("Surprise: Answered NO on LikeGames, but we calculated >0.6 positve response instead.");
                    Console.WriteLine("S2: I thought you would like computer games.");
                }
            }

            // 3. Surprise 
            //Thinks games are a good present - NO
            //Likes games in general - YES
            //surprise: think games are good present in general rendered more than 70%, but still answered NO by user
            if (UserLikesVideoGamesAnswered && UserThinksVideoGameIsGoodPresentAnswered &&
                UserLikesVideoGamesAnsweredYES && !UserThinksVideoGameIsGoodPresentYES)
            {
                var conditionLikesVideoGames = ProbVariables.VideoGameModel.ConditionHard(e => e.LikesVideoGames);

                double probThinksVideoGameIsGoodPresentConditionedOnLikesGames = conditionLikesVideoGames.ProbOf(e => e.ThinksVideogameIsGoodPresent).Value;

                Console.WriteLine("S3: "+ probThinksVideoGameIsGoodPresentConditionedOnLikesGames);

                if (probThinksVideoGameIsGoodPresentConditionedOnLikesGames >= 0.7)
                {
                    Console.WriteLine("S3: I though you will like a video game as a present.");
                }
            }
        }
    }
}

