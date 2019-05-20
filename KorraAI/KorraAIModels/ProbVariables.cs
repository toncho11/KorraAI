using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;
using Companion.KorraAI;

namespace Companion.KorraAI.Models
{
    /// <summary>
    /// A collection of all cognitive probabilistic variables 
    /// </summary>
    public static class ProbVariables
    {
        #region List of variables

        /// <summary>
        /// These are variables that explain the behavior of the user
        /// </summary>
        public static class User
        {
            #region States
            //updatable
            public static FiniteDist<bool> Tired = BernoulliF(Prob(0.4)); //tuned by a natural question

            //updatable
            public static FiniteDist<bool> InAGoodMood = BernoulliF(Prob(0.6)); //tuned by a natural question
            #endregion

            #region Preferences
            //updatable
            public static FiniteDist<bool> JokeRate = BernoulliF(Prob(0.7)); //tuned by a natural question "Do you prefer that I tell jokes more often?"
            #endregion
        }

        /// <summary>
        /// These are variables that explain the behavior of the bot
        /// </summary>
        public static class Bot
        {
            #region Suggestions
            //not updatable - depends on tired
            public static FiniteDist<bool> SuggestGoOut = from t in User.Tired
                                                          from sgo in BernoulliF(Prob(t ? 0.02 : 0.06)) //If tired suggest less going out 0.05
                                                          select sgo;
            //updatable 
            public static Prob PrSuggestListenToSong = Prob(0.35);

            //updatable 
            public static FiniteDist<bool> SuggestGoToGym = BernoulliF(Prob(0.10));

            //updatable
            public static FiniteDist<bool> TellWeatherForecast = BernoulliF(Prob(0.05)); //currently not used

            //updatable
            public static FiniteDist<bool> SuggestToWatchMovie = BernoulliF(Prob(0.1));

            //function used only for the "TellJoke" below
            //describes the distribution over "JokeRate" and "InAGoodMood"
            private static Func<bool, bool, Prob> TellJokeProb = (likesJoke, inGoodMood) => //used below in "TellJoke"
            {
                if (likesJoke && inGoodMood) return Prob(0.4);  //good mood translates to average amount of jokes
                if (likesJoke && !inGoodMood) return Prob(0.9); //not good mood requires more jokes

                return Prob(0.2); //does not like jokes, then the mood does not matter, the result is less jokes
            };

            //not updatable - its dependencies are updatable "likesJoke" and "inGoodMood"
            public static FiniteDist<bool> TellJoke =
                        from like in User.JokeRate   //tuned by natural question
                        from mood in User.InAGoodMood //tuned by natural question
                        from joke in BernoulliF(TellJokeProb(like, mood))
                        select joke;
            #endregion

            #region Actions
            //updatable
            public static Prob PrAskUncertanFactQuestion = Prob(0.02); //Ex. "Are you in a good mood (or tired)"
                                                                       //public static Prob PrAskUncertanFactQuestion = Prob(0.99); //Ex. "Are you in a good mood (or tired)"

            //updatable
            public static Prob[] PrMakeSuggestion = { Prob(0.95), Prob(0.95), Prob(0.3) }; //Tell joke, suggest watch movie, default: 0.7

            //updatable
            public static Prob[] PrAskPureFactQuestionAboutUser = { Prob(0.70), Prob(0.70), Prob(0.02), Prob(0.86) }; //Ex. user's name, user's age

            //updatable
            public static Prob[] PrSharePureFactInfoAboutBot = { Prob(0.80), Prob(0.80), Prob(0.02), Prob(0.86) }; //Ex. bot's name, sex or age

            //updatable
            public static FiniteDist<bool> ChangeVisualAppearance = BernoulliF(Prob(0.03)); //default 0.03

            //updatable
            public static FiniteDist<bool> ExpressMentalState = BernoulliF(Prob(0.03)); //default 0.03 Ex. InAGoodMood, Sleepy, Tired
            #endregion

            #region States

            public static FiniteDist<bool> InAGoodMood = BernoulliF(Prob(0.8)); //depends on ThingsBought / MusicPlayback
            
            #endregion
        }
        #endregion

        public static void PrintSuggestionsProbabilities()
        {
            SharedHelper.Log("SUGGESTIONS Probabilities: \r\n"
                                                      + "Suggest Go out: " + SharedHelper.GetProb(Bot.SuggestGoOut).Value.ToString() + "\r\n"
                                                      + "Tell Joke: " + SharedHelper.GetProb(Bot.TellJoke).Value.ToString() + "\r\n"
                                                      + "Suggest To WatchMovie: " + SharedHelper.GetProb(Bot.SuggestToWatchMovie).Value.ToString() + "\r\n"
                                                      + "Tell Weather Forecast: " + SharedHelper.GetProb(Bot.TellWeatherForecast).Value.ToString() + "\r\n" //not used
                                                      + "Suggest Listen To Song: " + Bot.PrSuggestListenToSong.Value.ToString() + "\r\n"
                                                    );

        }

        public static void PrintActionsProbabilities()
        {
            SharedHelper.Log("ACTIONS Probabilities: \r\n"
                                                      + "PrMakeSuggestion: " + Bot.PrMakeSuggestion[(int)PV.Current].Value.ToString() + "\r\n"
                                                      + "PrAskUncertanFactQuestion: " + Bot.PrAskUncertanFactQuestion.Value.ToString() + "\r\n"
                                                      + "PrAskPureFactQuestionAboutUser: " + Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value.ToString() + "\r\n"
                                                      + "PrSharePureFactInfoAboutBot: " + Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value.ToString() + "\r\n"
                                                      + "PrChangeVisualAppearance: " + SharedHelper.GetProb(Bot.ChangeVisualAppearance).Value.ToString() + "\r\n"
                                                      + "PrExpressMentalState: " + SharedHelper.GetProb(Bot.ExpressMentalState).Value.ToString() + "\r\n"
                                                    );

        }
    }
}
