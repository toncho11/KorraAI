using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion.KorraAI.Models.Joi
{
    /// <summary>
    /// Music Suggestions rate is increased after a fixed period of time
    /// </summary>
    public class MusicModelUpdateTrigger : IModelUpdateTrigger
    {
        private int executedCount = 0;
        readonly int MinutesIncreaseMusic;

        public string Name
        {
            get
            {
                return "MusicModelUpdateTrigger";
            }
        }

        public MusicModelUpdateTrigger(int minutesIncreaseMusic)
        {
            MinutesIncreaseMusic = minutesIncreaseMusic;
        }

        public bool IsTimeBased
        {
            get
            {
                return true;
            }
        }

        public bool IsUserResponseBased
        {
            get
            {
                return false;
            }
        }

        public int TriggeredCount
        {
            get
            {
                return executedCount;
            }
        }

        public bool IsOneTimeTrigger
        {
            get
            {
                return true;
            }
        }

        public bool Process(bool isPureFactUpdated, TimeSpan timeSinceStart, IKorraAIModel model)
        {
            //SharedHelper.LogWarning("Inside music trigger");

            // We increase the music suggestions after a fixed amount of time
            if (timeSinceStart.TotalMinutes > MinutesIncreaseMusic) // && !IncreaseDistributionSuggestMusicDone)
            {
                executedCount = executedCount + 1;

                ProbVariables.Bot.PrSuggestListenToSong = Prob(0.35);
                SharedHelper.LogWarning("PrSuggestListenToSong updated to: " + ProbVariables.Bot.PrSuggestListenToSong.Value);
            }

            return false; //no re-sampling
        }

        public void SetExecutedCount(int count)
        {
            executedCount = count;
        }
    }

    /// <summary>
    /// Changes the rate of Movies Suggestions based on user's responses (ex: is the user working) and the time of the day (evening)
    /// </summary>
    class MoviesModelUpdateTrigger : IModelUpdateTrigger
    {
        private int executedCount = 0;

        bool wasEveningBefore = StatesShared.IsEvening;
        bool wasWeekendBefore = StatesShared.IsWeekend;

        public string Name
        {
            get
            {
                return "MoviesModelUpdateTrigger";
            }
        }

        public bool IsTimeBased
        {
            get
            {
                return true; //depends not on elapsed time, but time of the day
            }
        }

        public bool IsUserResponseBased
        {
            get
            {
                return true;
            }
        }

        public bool IsOneTimeTrigger
        {
            get
            {
                return false;
            }
        }

        public int TriggeredCount
        {
            get
            {
                return executedCount;
            }
        }

        private bool IsTimeOfDayUpdated
        {
            get
            {
                bool updated = false;

                if (StatesShared.IsWeekend != wasWeekendBefore)
                {
                    wasWeekendBefore = StatesShared.IsWeekend;
                    updated = true;
                }

                if (StatesShared.IsEvening != wasEveningBefore)
                {
                    wasEveningBefore = StatesShared.IsEvening;
                    updated = true;
                }

                return updated;
            }
        }

        public bool Process(bool isPureFactUpdated, TimeSpan timeSinceStart, IKorraAIModel model)
        {
            //SharedHelper.LogWarning("Inside movies trigger");
            #region Get Fact Manager
            PureFacts pfManager = (PureFacts)model.ItemProviders.SingleOrDefault(x => x is PureFacts);
            if (pfManager == null)
            {
                SharedHelper.LogError("No manager in Process in MoviesModelUpdateTrigger.");
                return false;
            }
            #endregion

            var context = model.GetContext();

            PureFact factJob = (PureFact)pfManager.GetByName("UserHasJob");
            PureFact factWatchedMovieYesterday = (PureFact)pfManager.GetByName("UserMovieYesterday");

            if (factJob == null || factWatchedMovieYesterday == null) SharedHelper.LogError("factJob or factWatchedMovieYesterday is NULL in MoviesModelUpdateTrigger.");

            if (!IsTimeOfDayUpdated && !isPureFactUpdated) return false;

            SharedHelper.Log("Inside MoviesModelUpdateTrigger");

            var oldSuggestToWatchMovie = SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie).Value;

            //TODO: this model can be replaced by a Bayesian network, because of the too many IFs

            //if no job or weekend 
            if ((factJob.IsAnswered && context.BasePhrases.IsNo(factJob.Value)) || StatesShared.IsWeekend)
            {
                ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.18));
                //KorraBaseHelper.Log("Prob SuggestToWatchMovie changed to: 0.18, no job or weekend");
            }
            else if (factJob.IsAnswered && context.BasePhrases.IsYes(factJob.Value)) //if has job
            {
                #region working and evening
                if (StatesShared.IsEvening /*TODO: or after work hours*/)
                {
                    //has NOT watched movie yesterday (is working and evening)
                    if (factWatchedMovieYesterday.IsAnswered && context.BasePhrases.IsNo(factJob.Value))
                    {
                        ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.18));
                        //KorraBaseHelper.Log("Prob SuggestToWatchMovie changed to: 0.18, evening");
                    }
                    //has watched movie yesterday (is working and evening)
                    else if (factWatchedMovieYesterday.IsAnswered && context.BasePhrases.IsYes(factJob.Value))
                    {
                        ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.12));
                        // KorraBaseHelper.Log("Prob SuggestToWatchMovie changed to: 0.12. Watched movie yesterday.");
                    }
                }
                #endregion
                else //is working and not evening, no time because working and not evening
                {
                    ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.05));
                    //KorraBaseHelper.Log("Prob SuggestToWatchMovie changed to: 0.05");
                }
            }
            else //has job is unknown
            {
                ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.10));
            }

            double newSuggestToWatchMovie = SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie).Value;

            if (Math.Abs(oldSuggestToWatchMovie - newSuggestToWatchMovie) > (1 / 1000))
            {
                executedCount = executedCount + 1;

                SharedHelper.Log("Prob SuggestToWatchMovie changed from " + oldSuggestToWatchMovie + " to: " + SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie));
                //return new ModelUpdateTriggerReturn { IsTriggered = true, IsResamplingRequired = true, Value = "" };
                return true; //re-sampling requested
            }
            else
                return false; //no re-sampling
        }

        public void SetExecutedCount(int count)
        {
            executedCount = count;
        }
    }

    /// <summary>
    /// Based on user's responses and some a priori knowledge it calculates the bot should express surprise on the topic of video games
    /// </summary>
    public class VideoGameSurpriseTrigger : IModelEvaluateTrigger
    {
        private int executedCount = 0;

        IJoiPhrases phrases;

        public VideoGameSurpriseTrigger(IJoiPhrases phrases)
        {
            this.phrases = phrases;//new PhrasesEN(model.);
        }

        public string Name
        {
            get
            {
                return "VideoGameSurpriseTrigger";
            }
        }

        bool IModelTrigger.IsTimeBased
        {
            get
            {
                return false;
            }
        }

        bool IModelTrigger.IsUserResponseBased
        {
            get
            {
                return true;
            }
        }

        bool IModelTrigger.IsOneTimeTrigger
        {
            get
            {
                return true; //surprise on video game subjects is expressed just once
            }
        }

        int IModelTrigger.TriggeredCount
        {
            get
            {
                return executedCount;
            }
        }

        public CommItem? Process(IKorraAIModel model)
        {
            #region Get Fact Manager
            PureFacts pfManager = (PureFacts)model.ItemProviders.SingleOrDefault(x => x is PureFacts);
            if (pfManager == null)
            {
                SharedHelper.LogError("No manager in Facts Manager in MoviesModelUpdateTrigger.");
                return null;
            }
            #endregion

            var context = model.GetContext();
            string text = "";

            PureFact factLikesVideoGames = (PureFact)pfManager.GetByName("UserLikesVideoGames");
            PureFact factThinksVideoGameIsGoodPresent = (PureFact)pfManager.GetByName("UserThinksVideoGameIsGoodPresent");
            PureFact userAge = (PureFact)pfManager.GetByName("UserAge");
            PureFact userSex = (PureFact)pfManager.GetByName("UserSex");

            if (factLikesVideoGames == null || factThinksVideoGameIsGoodPresent == null || userAge == null || userSex == null) SharedHelper.LogError("factJob or factWatchedMovieYesterday is NULL in MoviesModelUpdateTrigger.");

            bool UserLikesVideoGamesAnswered = factLikesVideoGames.IsAnswered;
            bool UserThinksVideoGameIsGoodPresentAnswered = factThinksVideoGameIsGoodPresent.IsAnswered;

            bool userAgeAnswered = userAge.IsAnswered;
            bool userSexAnswered = userSex.IsAnswered;

            if (!UserLikesVideoGamesAnswered) return null;

            bool UserLikesVideoGamesAnsweredYES = context.BasePhrases.IsYes(factLikesVideoGames.Value); //only if answered 

            //Calcualte 3 differerent surprises:

            //1. surprise because statistically people from this sex and age like computer games
            //Should like Video Games, but user answered No
            if (userAgeAnswered && userSexAnswered &&
                factLikesVideoGames.IsAnswered &&
                !UserLikesVideoGamesAnsweredYES //DOES NOT LIKE VIDEO GAMES
                )
            {
                int Age = Convert.ToInt32(userAge.Value);
                bool IsMale = userSex.Value.ToLower() == phrases.Male().ToLower();

                var likesVideoGamesInferred = BernoulliF(ProbVariables.User.playsGames(Age, IsMale)); //our estimation based on Age and Sex

                //Threshold check
                if (likesVideoGamesInferred.ProbOf(e => e == true).Value > 0.8)
                {
                    SharedHelper.LogWarning("VG S1 surprise inferred.");
                    text = phrases.SurpriseVideoGames(1, false); //second parameter is not used
                    executedCount = executedCount + 1;

                    return new CommItem
                    {
                        Category = ActionsEnum.MakeGeneralStatement,
                        Name = "ExpressVideoGamesSurprise",

                        TextToSay = text,
                        FacialExpression = FaceExp.SurpriseOnStartTalking,
                    };
                }
                else SharedHelper.LogWarning("No VG S1 surprise, because it is below threshold.");
            }

            if (!UserThinksVideoGameIsGoodPresentAnswered) return null;
            bool UserThinksVideoGameIsGoodPresentYES = context.BasePhrases.IsYes(factThinksVideoGameIsGoodPresent.Value); //only if answered

            //2. Surprise
            //Thinks games are good present - YES
            //Likes games in general - NO
            //surprise: LikesGames rendered more than 70% positive, but still answered NO by user
            //backward inference
            if (UserLikesVideoGamesAnswered &&
                !UserLikesVideoGamesAnsweredYES &&  //DOES NOT LIKE VIDEO GAMES
                UserThinksVideoGameIsGoodPresentAnswered &&
                UserThinksVideoGameIsGoodPresentYES //LIKES VG AS PRESENT
               )
            {
                var conditionThinksVideogameIsGoodPresent = ProbVariables.User.VideoGameModel.ConditionHard(e => e.ThinksVideogameIsGoodPresent);

                //for this query we are not using Age and Sex, because we already know that the user LikesVideoGames (answered by the user)
                double probLikesGamesConditionedOnThinksGamesAreGoodPresentInferred = conditionThinksVideogameIsGoodPresent.ProbOf(e => e.LikesVideoGames).Value;

                SharedHelper.LogWarning("S2 probLikesGamesConditionedOnThinksGamesAreGoodPresent: " + probLikesGamesConditionedOnThinksGamesAreGoodPresentInferred);

                //Threshold check
                if (probLikesGamesConditionedOnThinksGamesAreGoodPresentInferred > 0.6)
                {
                    SharedHelper.LogWarning("VG S2 surprise inferred.");
                    bool IsLikesGamesFirstAnswered = factLikesVideoGames.LastUpdated < factThinksVideoGameIsGoodPresent.LastUpdated;
                    text = phrases.SurpriseVideoGames(2, IsLikesGamesFirstAnswered);
                    executedCount = executedCount + 1;

                    return new CommItem
                    {
                        Category = ActionsEnum.MakeGeneralStatement,
                        Name = "ExpressVideoGamesSurprise",

                        TextToSay = text,
                        IsReactionToUser = true,
                        FacialExpression = FaceExp.SurpriseOnStartTalking,
                    };

                }
                else SharedHelper.LogWarning("No VG S2 surprise, because it is below threshold.");
            }

            // 3. Surprise 
            //Thinks games are a good present - NO
            //Likes games in general - YES
            //surprise: ThinkGamesGoodPresent rendered more than 70% positive, but still answered NO by user
            if (UserLikesVideoGamesAnswered &&
                UserThinksVideoGameIsGoodPresentAnswered &&
                UserLikesVideoGamesAnsweredYES &&     //LIKES VIDEO GAMES
                !UserThinksVideoGameIsGoodPresentYES) //DOES NOT LIKE VG AS PRESENT
            {
                //TODO: it should not be here, but attached to the item
                //THIS IS NEEDED FOR THE INFERRENCE BELOW ON: probThinksVideoGameIsGoodPresentInferred
                if (UserLikesVideoGamesAnsweredYES)
                    ProbVariables.User.LikesVideoGames = BernoulliF(Prob(0.98));

                var probThinksVideoGameIsGoodPresentInferred = ProbVariables.User.VideoGameModel.ProbOf(e => e.ThinksVideogameIsGoodPresent).Value;

                SharedHelper.LogWarning("S3 probThinksVideoGameIsGoodPresent: " + probThinksVideoGameIsGoodPresentInferred);

                //Threshold check
                if (probThinksVideoGameIsGoodPresentInferred >= 0.7)
                {
                    SharedHelper.LogWarning("VG S3 surprise inferred.");
                    bool IsLikesGamesFirstAnswered = factLikesVideoGames.LastUpdated < factThinksVideoGameIsGoodPresent.LastUpdated;
                    text = phrases.SurpriseVideoGames(3, IsLikesGamesFirstAnswered);
                    executedCount = executedCount + 1;

                    return new CommItem
                    {
                        Category = ActionsEnum.MakeGeneralStatement,
                        Name = "ExpressVideoGamesSurprise",

                        TextToSay = text,
                        FacialExpression = FaceExp.SurpriseOnStartTalking,
                    };
                }
                else SharedHelper.LogWarning("No VG S3 surprise, because it is below threshold.");
            }

            return null;
        }

        public void SetExecutedCount(int count)
        {
            executedCount = count;
        }
    }
}

