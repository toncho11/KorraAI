using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;
using Companion.KorraAI.Models;

namespace Companion.KorraAI
{
    public class KorraAISampler
    {
        ModelContext context;

        System.Random rnd = new System.Random();

        private Queue<string> allSuggestions = new Queue<string>();
        private Queue<string> allActions = new Queue<string>();

        public void Init(ModelContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// To set pobabilities but also connections between them 
        /// </summary>
        //public void SetProbVariables(string name)
        //{

        //}
        /// <summary>
        /// To set pobabilities but also connections between them 
        /// </summary>
        //public void UpdateProbVariables(string name)
        //{

        //}

        private Queue<string> GenerateActions(string[] disabledActions)
        {
            allActions.Clear();

            #region Create a list of all possible actions

            ItemProb<string>[] items =
            {
                   ItemProb(ActionsEnum.MakeSuggestion, ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current]),

                   ItemProb(ActionsEnum.AskUncertanFactQuestion, ProbVariables.Bot.PrAskUncertanFactQuestion),

                   ItemProb(ActionsEnum.AskPureFactQuestionAboutUser, ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current]), //to be packed as one

                   ItemProb(ActionsEnum.SharePureFactInfoAboutBot, ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current]), //to be packed as one

                   ItemProb(ActionsEnum.ChangeVisualAppearance, SharedHelper.GetProb(ProbVariables.Bot.ChangeVisualAppearance)),

                   ItemProb(ActionsEnum.ExpressMentalState, SharedHelper.GetProb(ProbVariables.Bot.ExpressMentalState)),
            };

            #endregion

            #region Disable actions
            if (BotConfigShared.DisableAskQuestions)
            {
                //two actions are removed
                DisableAcion(ref items, ActionsEnum.AskPureFactQuestionAboutUser);
                DisableAcion(ref items, ActionsEnum.AskUncertanFactQuestion);
            }

            foreach (string da in disabledActions)
            {
                DisableAcion(ref items, da);
            }
            #endregion

            #region Sampling

            var Action = CategoricalF(items).Normalize();

            SharedHelper.Log("Actions Histogram:\r\n" + Action.Histogram());

            var actionDist = Action.ToSampleDist();

            for (var i = 0; i < 100; i++)
            {
                string sample = actionDist.Sample();

                allActions.Enqueue(sample);
            }
            #endregion

            return allActions;
        }

        private static void DisableAcion(ref ItemProb<string>[] items, string actionToRemove)
        {
            SharedHelper.Log("Action removed: " + actionToRemove.ToString());

            items = items.Where(x => x.Item != actionToRemove).ToArray();
        }

        public Queue<string> GenerateSuggestions()
        {
            #region Update suggstions based on new information

            allSuggestions.Clear();

            ProbVariables.PrintSuggestionsProbabilities();

            var Suggestion = CategoricalF(

                    //Can be multiple times
                    ItemProb(SuggestionsEnum.TellJoke, SharedHelper.GetProb(ProbVariables.Bot.TellJoke)), //we can say several jokes in a row, no need to reduce the probability of saying a joke

                    //Multiple times
                    ItemProb(SuggestionsEnum.ListenToSong, ProbVariables.Bot.PrSuggestListenToSong),

                    //A few times per evening
                    ItemProb(SuggestionsEnum.GoOut, SharedHelper.GetProb(ProbVariables.Bot.SuggestGoOut)),

                    //A few times per evening
                    ItemProb(SuggestionsEnum.WatchMovie, SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie)),

                    //Once per Morning (current day) / evening (tomorrow)
                    //ItemProb(Suggestions.TellWeatherForecast, ProbVariables.PrTellWeatherForecast),

                    ItemProb(SuggestionsEnum.GoToGym, SharedHelper.GetProb(ProbVariables.Bot.SuggestGoToGym))

                ).Normalize();

            SharedHelper.Log("Suggestion Histogram:\r\n" + Suggestion.Histogram());

            var suggestionDist = Suggestion.ToSampleDist();

            for (var i = 0; i < 100; i++)
            {
                string sample = suggestionDist.Sample();

                allSuggestions.Enqueue(sample);
            }
            #endregion

            return allSuggestions;

        }

        /// <summary>
        /// Generates the final output used for interaction with the user
        /// </summary>
        /// <param name="ClearOldInteractions">
        /// Answer to questions or other events that happen at runtime require that the old Interactions items are cleared.
        /// When items are generated in advance, regeneation is not requires, as we need to follow the old distribution and then the new one.
        /// </param>
        public void ReGenerateMainSequence(bool ClearOldInteractions, ref Queue<CommItem> Interactions)
        {
            Dictionary<string, int> interStats = new Dictionary<string, int>();

            int oldInteractionsCount = Interactions.Count;
            DateTime start = DateTime.Now;
            SharedHelper.Log("**************** Regenerating interactions: currently generated interactions: " + oldInteractionsCount);

            /*Old interactions are not cleared because they followed an old distribution that was correct
              Due to changes in environment/questions or simply lask items for certain category forces a new distribution.
              This means that new interactions items are appended.
            */

            if (ClearOldInteractions)
            {
                JokesProvider.RemovePlannedFlagForAllJokes(); //so that we can re-schedule them
                SongsProvider.RemovePlannedFlagForAllSongs(); //so that we can re-schedule them
                SportsProvider.RemovePlannedFlagForAllSports(); //so that we can re-schedule them
                MoviesProvider.RemovePlannedFlagForAllMovies(); //so that we can re-schedule them
                PureFacts.RemovePlannedFlagForAllPureFacts(); //so that we can re-schedule them
                BuysProvider.RemovePlannedFlagForAllBuys(); //so that we can re-schedule them

                Interactions.Clear();
                SharedHelper.Log("Old buffered interactions cleared.");
            }

            //Interactions are built newely generated suggestions and actions
            allSuggestions = GenerateSuggestions();
            allActions = GenerateActions();

            while (allActions.Count > 0 && allSuggestions.Count > 0)
            {
                CommItem citem = new CommItem();
                citem.Action = allActions.Dequeue();

                //SharedHelper.Log("action: " + action);

                #region Set Suggestion
                if (citem.Action == ActionsEnum.MakeSuggestion)
                {
                    string suggestion = allSuggestions.Dequeue();

                    //SharedHelper.Log("suggestion: " + suggestion);

                    citem.Suggestion = suggestion;

                    #region set joke
                    if (suggestion == SuggestionsEnum.TellJoke)
                    {
                        //the fact that joke has bee planned, does not mean it has been executed
                        Joke joke = JokesProvider.GetJoke();
                        if (joke != null)
                        {
                            joke.IsPlanned = true;
                            //SharedHelper.LogError("Joke name: " + joke.Name);
                            citem.Name = joke.Name;
                            //SharedHelper.LogError("Joke text: " + joke.Text);
                            citem.TextToSay = joke.Text;
                            citem.Suggestion = suggestion;
                            citem.IsJokePureFact = joke.IsPureFact; //these jokes come from the PureFacts collection
                            citem.UIAnswer = joke.PureFactUI;

                            if (interStats.ContainsKey(SuggestionsEnum.TellJoke)) interStats[SuggestionsEnum.TellJoke]++; else interStats[SuggestionsEnum.TellJoke] = 0;
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            //UnityEngine.Debug.LogWarning("Not enough jokes during planning.");
                            if (interStats.ContainsKey(SuggestionsEnum.TellJoke + "_missing")) interStats[SuggestionsEnum.TellJoke + "_missing"]++; else interStats[SuggestionsEnum.TellJoke + "_missing"] = 0;
                        }
                    }
                    #endregion

                    #region set Go out
                    if (suggestion == SuggestionsEnum.GoOut)
                    {
                        citem.TextToSay = context.Phrases.GoOutAnnoucement();
                        if (interStats.ContainsKey(SuggestionsEnum.GoOut)) interStats[SuggestionsEnum.GoOut]++; else interStats[SuggestionsEnum.GoOut] = 0;
                    }
                    #endregion

                    #region set Watch movie
                    if (suggestion == SuggestionsEnum.WatchMovie)
                    {
                        Movie movie = MoviesProvider.GetMovie();
                        if (movie != null)
                        {
                            movie.IsPlanned = true;
                            citem.Name = movie.Name;
                            citem.TextToSay = context.Phrases.MovieAnnouncement(movie);

                            //UnityEngine.Debug.LogWarning("Movie scheduled: " + citem.TextToSay);
                            if (interStats.ContainsKey(SuggestionsEnum.WatchMovie)) interStats[SuggestionsEnum.WatchMovie]++; else interStats[SuggestionsEnum.WatchMovie] = 0;
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            SharedHelper.LogWarning("Not enough movies during planning.");
                            if (interStats.ContainsKey(SuggestionsEnum.WatchMovie + "_missing")) interStats[SuggestionsEnum.WatchMovie + "_missing"]++; else interStats[SuggestionsEnum.WatchMovie + "_missing"] = 0;
                        }
                    }
                    #endregion

                    #region set Weather forecast
                    if (suggestion == SuggestionsEnum.TellWeatherForecast)
                    {
                        citem.TextToSay = "The weather forecast should be good."; //TODO not translated
                    }
                    #endregion

                    #region set Song
                    if (suggestion == SuggestionsEnum.ListenToSong)
                    {
                        //the fact that song has bee planned, does not mean it has been executed
                        Song song = SongsProvider.GetSong();
                        if (song != null)
                        {
                            song.IsPlanned = true;
                            citem.Name = song.Name;
                            citem.TextToSay = song.Name;
                            if (interStats.ContainsKey(SuggestionsEnum.ListenToSong)) interStats[SuggestionsEnum.ListenToSong]++; else interStats[SuggestionsEnum.ListenToSong] = 0;
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            SharedHelper.LogWarning("Not enough songs during planning.");
                            if (interStats.ContainsKey(SuggestionsEnum.ListenToSong + "_missing")) interStats[SuggestionsEnum.ListenToSong + "_missing"]++; else interStats[SuggestionsEnum.ListenToSong + "_missing"] = 0;
                        }
                    }
                    #endregion

                    #region set Go to gym
                    if (suggestion == SuggestionsEnum.GoToGym)
                    {
                        Sport sport = SportsProvider.GetSport();
                        if (sport != null)
                        {
                            sport.IsPlanned = true;
                            citem.Name = sport.Name;
                            citem.TextToSay = sport.Text;

                            if (interStats.ContainsKey(SuggestionsEnum.GoToGym)) interStats[SuggestionsEnum.GoToGym]++; else interStats[SuggestionsEnum.GoToGym] = 0;
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            SharedHelper.LogWarning("Not enough sports during planning.");
                            if (interStats.ContainsKey(SuggestionsEnum.GoToGym + "_missing")) interStats[SuggestionsEnum.GoToGym + "_missing"]++; else interStats[SuggestionsEnum.GoToGym + "_missing"] = 0;
                        }
                    }
                    #endregion

                } //end action suggestions
                #endregion
                else if (citem.Action == ActionsEnum.AskUncertanFactQuestion)
                {
                    //the actual question is selected at run time
                    citem.TextToSay = "###place holder for UncertanFactQuestion";
                    citem.IsPureFact = false;

                    if (interStats.ContainsKey(ActionsEnum.AskUncertanFactQuestion)) interStats[ActionsEnum.AskUncertanFactQuestion]++; else interStats[ActionsEnum.AskUncertanFactQuestion] = 0;
                }
                else if (citem.Action == ActionsEnum.AskPureFactQuestionAboutUser) //ABOUT USER
                {
                    PureFact[] q = PureFactsAboutUserLeft();

                    if (q.Length > 0)
                    {
                        #region Randomize the first element
                        int r = rnd.Next(q.Length);
                        SharedHelper.Swap(q, 0, r);
                        #endregion

                        //SharedHelper.LogWarning("Found total " + Actions.AskPureFactQuestionAboutUser + ": " + q.Length.ToString());

                        citem.TextToSay = q[0].Question;
                        //SharedHelper.Log("q name: " + q [0].Name);
                        citem.Name = q[0].Name;
                        citem.IsPureFact = true;

                        PureFacts.SetAsPlanned(q[0].Name);

                        citem.UIAnswer = q[0].UI;

                        if (interStats.ContainsKey(ActionsEnum.AskPureFactQuestionAboutUser)) interStats[ActionsEnum.AskPureFactQuestionAboutUser]++; else interStats[ActionsEnum.AskPureFactQuestionAboutUser] = 0;
                    }
                    else if (interStats.ContainsKey(ActionsEnum.AskPureFactQuestionAboutUser + "_missing")) interStats[ActionsEnum.AskPureFactQuestionAboutUser + "_missing"]++; else interStats[ActionsEnum.AskPureFactQuestionAboutUser + "_missing"] = 0;

                    AdjustForPureFactsAvailabilityUserAndBot(q.Length, false, true);
                }
                else if (citem.Action == ActionsEnum.SharePureFactInfoAboutBot) //ABOUT BOT
                {
                    var q = PureFactsAboutBotLeft();

                    if (q.Length > 0)
                    {
                        //SharedHelper.Log("Found " + Actions.SharePureFactInfoAboutBot + ": " + q.Length.ToString());
                        citem.TextToSay = q[0].Acknowledgement;
                        citem.IsPureFact = true;
                        citem.Name = q[0].Name;

                        PureFacts.SetAsPlanned(q[0].Name);

                        if (interStats.ContainsKey(ActionsEnum.SharePureFactInfoAboutBot)) interStats[ActionsEnum.SharePureFactInfoAboutBot]++; else interStats[ActionsEnum.SharePureFactInfoAboutBot] = 0;
                    }
                    else if (interStats.ContainsKey(ActionsEnum.SharePureFactInfoAboutBot + "_missing")) interStats[ActionsEnum.SharePureFactInfoAboutBot + "_missing"]++; else interStats[ActionsEnum.SharePureFactInfoAboutBot + "_missing"] = 0;

                    AdjustForPureFactsAvailabilityUserAndBot(q.Length, true, false);
                }
                else if (citem.Action == ActionsEnum.ChangeVisualAppearance)
                {
                    citem.TextToSay = context.Phrases.ChangeClothesAnnouncement();
                    if (interStats.ContainsKey(ActionsEnum.ChangeVisualAppearance)) interStats[ActionsEnum.ChangeVisualAppearance]++; else interStats[ActionsEnum.ChangeVisualAppearance] = 0;
                }
                else if (citem.Action == ActionsEnum.ExpressMentalState)
                {
                    //choose mental state to share using a distribution (uniform?)
                    //currently only one state is processed
                    string selectedMentalState = "InAGoodMood";

                    if (selectedMentalState == "InAGoodMood")
                    {
                        //Take into account the: ProbVariables.Bot.InAGoodMood;
                        //class Statement where the constructor takes prob variable as or UncertainFact
                        //or just phrase that internally takes into account current values of the prob variable and other normal variables
                        citem.TextToSay = "###place holder for InAGoodMood";
                    }

                    if (interStats.ContainsKey(ActionsEnum.ExpressMentalState)) interStats[ActionsEnum.ExpressMentalState]++; else interStats[ActionsEnum.ExpressMentalState] = 0;
                }
                else SharedHelper.LogError("Unknown action: " + citem.Action);

                #region Add interaction item to queue

                if (string.IsNullOrEmpty(citem.TextToSay))
                    SharedHelper.LogError("Text is empty! Action was: " + citem.Action);
                else
                {
                    Interactions.Enqueue(citem);
                }

                #endregion

                #region debug
                PureFact[] LeftPureFacts = (from pf in PureFacts.GetList()
                                            where (pf.Type == PureFactType.AboutBot || pf.Type != PureFactType.AboutUser)
                                            && pf.IsPlanned == false && pf.IsUsed == false
                                            select pf).ToArray();

                //if (!Flags.DecreaseDistributionOfAskPureFactQuestionAboutUserDone && Interactions.Count > 45 && (LeftPureFacts.Length - PersistentData.PureFactsLoadedOnStartup()) > 0)
                //    KorraBaseHelper.LogError("All pure facts should have been already planned. Left not used pure facts: " + LeftPureFacts.Length);

                #endregion
            }

            Interactions = context.SpeechAdaptation.ProcessItems(Interactions, context.Phrases);

            DateTime end = DateTime.Now;
            SharedHelper.Log("Time regeneration took: " + (end - start).TotalMilliseconds + " ms.");

            #region Debug
            string DebugItem = "";
            int i = 0;
            foreach (string e in Interactions.Select(e => e.TextToSay))
            {
                i++;
                DebugItem += "|" + i + ". " + e;
            }
            SharedHelper.LogWarning("Interactions list: " + DebugItem);

            DebugItem = "";
            i = 0;
            foreach (var key in interStats.Keys)
            {
                i++;
                DebugItem += "|" + key + ": " + interStats[key] + Environment.NewLine;
            }
            SharedHelper.LogWarning("Interactions statistics: " + Environment.NewLine + DebugItem);

            #endregion

            ProbVariables.PrintActionsProbabilities();

            SharedHelper.Log("Iteractions added: " + (Interactions.Count - oldInteractionsCount) + ". Total Interactions:" + Interactions.Count);
        }

        public static PureFact[] PureFactsAboutUserLeft()
        {
            var q = (from pf in PureFacts.GetList()
                     where pf.Type == PureFactType.AboutUser && pf.IsPlanned == false && pf.IsAnswered == false
                     select pf).ToArray();

            return q;
        }

        public static PureFact[] PureFactsAboutBotLeft()
        {
            var q = (from pf in PureFacts.GetList()
                     where pf.Type == PureFactType.AboutBot && pf.IsPlanned == false && pf.IsUsed == false
                     select pf).ToArray();

            return q;
        }

        private static void DisablePureFactsAboutBot()
        {
            ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current] = Prob(0);
            SharedHelper.LogWarning("Disabled all pure facts about bot, because there were no items.");
        }

        private static void DisablePureFactQuestionAboutUser()
        {
           ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current] = Prob(0);
            SharedHelper.LogWarning("Disabled all pure facts about user, because there were no items.");
        }

        /// <summary>
        /// Adjusts the probabilities of Pure Facts about Bot and User
        /// If there are no more facts about the Bot, we disable this category, but we boost the other
        /// If both contain no more items then both are disabled, but then we need to boost suggestions a bit
        /// </summary>
        /// <param name="factsLeft"></param>
        /// <param name="isAboutBot"></param>
        /// <param name="isAboutUser"></param>
        public void AdjustForPureFactsAvailabilityUserAndBot(int factsLeft, bool isAboutBot, bool isAboutUser)
        {
            bool regenerate = false;

            if (isAboutUser)
            {
                if (factsLeft <= 1) //1 because 1 is the only currently processed
                {
                    //adjust pure facts probabilities: disable one and boost the other 
                    if (ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value > 0)
                        ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current] = ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Increased]; //we reinfoce the other one so that it is stronger than suggestion action

                    DisablePureFactQuestionAboutUser();//because there are no more facts for it 
                    regenerate = true;
                }
            }

            if (isAboutBot)
            {
                //exclude this action because there are no items for it
                if (factsLeft <= 1)
                {
                    //adjust pure facts probabilities: disable one and boost the other 
                    if (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value > 0) //the other is boosted only if it was not disabled
                        ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current] = ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Increased]; //we re-infoce the other one so that it is stronger than suggestion action

                    DisablePureFactsAboutBot(); //because there are no more facts for it
                    regenerate = true;
                }
            }

            //if no more pure facts we also boost suggestions
            if (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value == 0 && ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value == 0)
            {
                ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] = ProbVariables.Bot.PrMakeSuggestion[(int)PV.Default];
                SharedHelper.LogWarning("All pure facts used. PrMakeSuggestion changed to: " + ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current]);
                regenerate = true;
            }

            if (regenerate)
                allActions = GenerateActions(); //we get new actions considering the above change of probability (actions are replaced)


        }

        public Queue<string> GenerateActions()
        {
            return GenerateActions(new string[] { });
        }
    }
}
