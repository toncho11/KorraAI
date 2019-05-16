using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Companion.KorraAI;
using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion.KorraAI.Models.Joi
{
    public class JoiModel : IKorraAIModel
    {
        bool DecreaseDistributionOfAskPureFactQuestionAboutUserDone;

        /// <summary>
        /// After how many minutes to decrease the distribution for AskPureFactQuestionAboutUser
        /// </summary>
        int MinutesDecreaseAskPureFactQuestionAboutUser = 15; //default 15;

        KorraAISampler korraSampler;

        IDistributions cognitiveDist;

        public event EventHandler ContextLoaded;

        //Set ProbVariables here
        //accessible directly
        bool isInitialized = false;

        ModelContext CC; //CurrentContext

        public string Name
        {
            get
            {
                return "Joi";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public void Init()
        {
            CC = new ModelContext();

            if (BotConfigShared.Language == Lang.EN)
            {
                CC.Phrases = new PhrasesEN();

                CC.Items = new ItemsEN();

                CC.SpeechAdaptation = new SpeechAdaptationEN();
            }
            else
            {
                //if (BotConfig.Language == Lang.JA)
                //{
                //    CC.Phrases = new PhrasesJP();

                //    CC.Items = new ItemsJP();

                //    CC.SpeechAdaptation = new SpeechAdaptationJP();
                //}
            }

            CC.Items.LoadAll(CC.Phrases);

            korraSampler = new KorraAISampler();
            korraSampler.Init(CC);

            cognitiveDist = new JoiDistributions();
            //cache values
            cognitiveDist.NextSmilePauseTime();
            cognitiveDist.NextTimeDurationEyesStayFocusedOnCameraAfterStartedTalking();
            cognitiveDist.GetNextInteactionPause();
            cognitiveDist.NextQuestionTimeout();

            isInitialized = true;
            ContextLoaded(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public KorraAISampler GetModel()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return korraSampler;
        }

        public KorraAISampler GetSampler()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return korraSampler;
        }

        public ModelContext GetContext()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return CC;
        }

        public bool ModelUpdate(TimeSpan timeSinceStart)
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");

            bool stateChange = false;

            #region Update Actions

            if (timeSinceStart.TotalMinutes > MinutesDecreaseAskPureFactQuestionAboutUser && !DecreaseDistributionOfAskPureFactQuestionAboutUserDone)
            {
                bool thereAreUnAnsweredQuestionsAboutUser = KorraAISampler.PureFactsAboutUserLeft().Length > 0;
                if (thereAreUnAnsweredQuestionsAboutUser)
                {
                    DecreaseDistributionOfAskPureFactQuestionAboutUserDone = true;

                    ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current] = ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Descreased];

                    SharedHelper.LogWarning("Prob AskPureFactQuestionAboutUser changed to: " + ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value);

                    stateChange = true;
                }
            }

            //regeneration occurred and there are no pure facts then MakeSuggestion is high/default, otherwise we keep it down
            if (KorraAISampler.PureFactsAboutUserLeft().Length == 0 && KorraAISampler.PureFactsAboutBotLeft().Length == 0)
                ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] = ProbVariables.Bot.PrMakeSuggestion[(int)PV.Default];
            else
            {   //some pure facts were left unanswered
                //if the chance for the PrAskPureFactQuestionAboutUser is too low, we will not 
                //select these items and switch back to higher PrMakeSuggestion
                if (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value != ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Descreased].Value)
                    ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] = ProbVariables.Bot.PrMakeSuggestion[(int)PV.Descreased];
            }

            #endregion

            #region Update suggestions

            //TODO: program WatchedMovieYesterday, AlreadyWatchedMovieToday, IsWeatherForecastGood

            //TODO: adjust movies suggestion based if the user works and the work hours

            #region weather
            //if (Sates.IsWeatherForecastGood)
            //    ProbVariables.TellWeatherForecast = BernoulliF(Prob(0.9));
            //else
            //ProbVariables.TellWeatherForecast = from igm in ProbVariables.InAGoodMood
            //                                    from twf in BernoulliF(Prob(igm ? 0.4 : 0.7)) //if in a good mood and
            //                                    select twf;
            #endregion

            #region adjust movies
            PureFact factJob = PureFacts.GetFacfByName("UserHasJob");
            PureFact factWatchedMovieYesterday = PureFacts.GetFacfByName("UserMovieYesterday");

            var oldSuggestToWatchMovie = SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie).Value;

            //no job: suggest during the entire day 0.18, more time means more movie suggestions
            if ((factJob.IsAnswered && CC.Phrases.IsNo(factJob.Value)) || StatesShared.IsWeekend)
            {
                ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.18));
                //KorraBaseHelper.Log("Prob SuggestToWatchMovie changed to: 0.18, no job or weekend");
            }
            else if (factJob.IsAnswered && CC.Phrases.IsYes(factJob.Value)) //has job
            {
                #region working and evening
                if (StatesShared.IsEvening /*TODO: or after work hours*/)
                {
                    //has NOT watched movie yesterday (is working and evening)
                    if (factWatchedMovieYesterday.IsAnswered && CC.Phrases.IsNo(factJob.Value))
                    {
                        ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.18));
                        //KorraBaseHelper.Log("Prob SuggestToWatchMovie changed to: 0.18, evening");
                    }
                    //has watched movie yesterday (is working and evening)
                    else if (factWatchedMovieYesterday.IsAnswered && CC.Phrases.IsYes(factJob.Value))
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
            else //has job unknown
            {
                ProbVariables.Bot.SuggestToWatchMovie = BernoulliF(Prob(0.10));
            }

            if (oldSuggestToWatchMovie != SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie).Value)
            {
                stateChange = true;
                SharedHelper.Log("Prob SuggestToWatchMovie changed to: " + SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie));
            }

            #endregion

            #region adjust sport
            //PureFact factSport = PureFacts.GetFacfByName("UserDoesSport");
            //if (factSport.IsAnswered && Phrases.IsNo(factSport.Value))
            //{
            //    ProbVariables.SuggestGoToGym = BernoulliF(Prob(0.15));
            //    UnityEngine.Debug.LogWarning("Prob SuggestGoToGym changed from 0.10 to 0.15, no sport done by user");
            //}
            #endregion

            #endregion

            return stateChange;
        }

        public void BeforeAnalyseUserResponse(PureFact fact)
        {
            bool isAnswerd = fact.IsAnswered;
            string responseValue = fact.Value;
        }

        public IDistributions GetCognitiveDist()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return cognitiveDist;
        }
    }
}
