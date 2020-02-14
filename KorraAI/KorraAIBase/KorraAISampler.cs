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

        Func<int, bool> AdjFunc;
        Func<string , Item> GetItemFunc;

        public void Init(ModelContext context, Func<int, bool> adjFunc, Func<string, Item> getItemFunc)
        {
            this.context = context;
            this.AdjFunc = adjFunc;
            this.GetItemFunc = getItemFunc;
        }

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

                    //Multiple times
                    ItemProb(SuggestionsEnum.ListenToSong, ProbVariables.Bot.PrSuggestListenToSong),

                    //Can be multiple times
                    ItemProb(SuggestionsEnum.TellJoke, SharedHelper.GetProb(ProbVariables.Bot.TellJoke)), //we can say several jokes in a row, no need to reduce the probability of saying a joke

                    //A few times per evening
                    ItemProb(SuggestionsEnum.GoOut, SharedHelper.GetProb(ProbVariables.Bot.SuggestGoOut)),

                    //A few times per evening
                    ItemProb(SuggestionsEnum.WatchMovie, SharedHelper.GetProb(ProbVariables.Bot.SuggestToWatchMovie)),

                    //NOT USED!
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
        public void ReGenerateMainSequence(ref Queue<CommItem> Interactions)
        {
            bool ClearOldInteractions = true;
            InteractionsStat.Reset();

            int oldInteractionsCount = Interactions.Count;
            DateTime start = DateTime.Now;
            SharedHelper.Log("**************** Regenerating interactions ****************");

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
                if (this.AdjFunc(Interactions.Count))
                    allActions = GenerateActions();

                //==================================================================================================

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

                        Joke joke = GetItemFunc(SuggestionsEnum.TellJoke) as Joke;

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
                            citem.FacialExpression = joke.FaceExpression;

                            InteractionsStat.AddScheduledInteraction(SuggestionsEnum.TellJoke);
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            //UnityEngine.Debug.LogWarning("Not enough jokes during planning.");
                            InteractionsStat.AddMissingInteraction(SuggestionsEnum.TellJoke);
                        }
                    }
                    #endregion

                    #region set Go out
                    if (suggestion == SuggestionsEnum.GoOut)
                    {
                        citem.TextToSay = context.Phrases.GoOutAnnoucement();
                        InteractionsStat.AddScheduledInteraction(SuggestionsEnum.GoOut);
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
                            InteractionsStat.AddScheduledInteraction(SuggestionsEnum.WatchMovie);
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            SharedHelper.LogWarning("Not enough movies during planning.");
                            InteractionsStat.AddMissingInteraction(SuggestionsEnum.WatchMovie);
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

                            InteractionsStat.AddScheduledInteraction(SuggestionsEnum.ListenToSong);
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            SharedHelper.LogWarning("Not enough songs during planning.");
                            InteractionsStat.AddMissingInteraction(SuggestionsEnum.ListenToSong);
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

                            InteractionsStat.AddScheduledInteraction(SuggestionsEnum.GoToGym);
                        }
                        else
                        {
                            citem.TextToSay = "__nointeraction__";
                            SharedHelper.LogWarning("Not enough sports during planning.");
                            InteractionsStat.AddMissingInteraction(SuggestionsEnum.GoToGym);
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

                    InteractionsStat.AddScheduledInteraction(ActionsEnum.AskUncertanFactQuestion);
                }
                else if (citem.Action == ActionsEnum.AskPureFactQuestionAboutUser) //ABOUT USER
                {
                    int pfabul = PureFactsAboutUserLeftCount();

                    if (pfabul > 0)
                    {
                        PureFact sf = PureFacts.GetPureFactAbouUser();

                        if (sf == null)
                            SharedHelper.LogError("There are pure facts about the user left, but no pure fact has been selected.");
                        else
                        {
                            citem.TextToSay = sf.Question;
                            //SharedHelper.Log("q name: " + q [0].Name);
                            citem.Name = sf.Name;
                            citem.IsPureFact = true;

                            PureFacts.SetAsPlanned(sf.Name);

                            citem.UIAnswer = sf.UI;
                        }
                        InteractionsStat.AddScheduledInteraction(ActionsEnum.AskPureFactQuestionAboutUser);
                    }
                    else InteractionsStat.AddMissingInteraction(ActionsEnum.AskPureFactQuestionAboutUser);
                }
                else if (citem.Action == ActionsEnum.SharePureFactInfoAboutBot) //ABOUT BOT
                {
                    int pfabbl = PureFactsAboutBotLeftCount();

                    if (pfabbl > 0)
                    {
                        PureFact sf = GetPureFactAbouBot();
                        if (sf == null)
                            SharedHelper.LogError("There are pure facts about the bot left, but no pure fact has been selected.");
                        else
                        {
                            //SharedHelper.Log("Found " + Actions.SharePureFactInfoAboutBot + ": " + q.Length.ToString());
                            citem.TextToSay = sf.Acknowledgement;
                            citem.IsPureFact = true;
                            citem.Name = sf.Name;

                            PureFacts.SetAsPlanned(sf.Name);
                        }
                        InteractionsStat.AddScheduledInteraction(ActionsEnum.SharePureFactInfoAboutBot);
                    }
                    else
                        InteractionsStat.AddMissingInteraction(ActionsEnum.SharePureFactInfoAboutBot);
                }
                else if (citem.Action == ActionsEnum.ChangeVisualAppearance)
                {
                    citem.TextToSay = context.Phrases.ChangeClothesAnnouncement();
                    InteractionsStat.AddScheduledInteraction(ActionsEnum.ChangeVisualAppearance);
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

                    InteractionsStat.AddScheduledInteraction(ActionsEnum.ExpressMentalState);
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

            InteractionsStat.PrintSummary();

            #endregion

            ProbVariables.PrintActionsProbabilities();

            SharedHelper.Log("Interactions added: " + (Interactions.Count - oldInteractionsCount) + ". Total Interactions:" + Interactions.Count);
            SharedHelper.Log("**************** Regenerating interactions ****************");
        }

        public static int PureFactsAboutUserLeftCount()
        {
            var q = (from pf in PureFacts.GetList()
                     where pf.Type == PureFactType.AboutUser && pf.IsPlanned == false && pf.IsUsed == false
                     select pf).ToArray();

            return q.Length;
        }

        public static int PureFactsAboutBotLeftCount()
        {
            var q = (from pf in PureFacts.GetList()
                     where pf.Type == PureFactType.AboutBot && pf.IsPlanned == false && pf.IsUsed == false
                     select pf).ToArray();

            return q.Length;
        }

        private static PureFact GetPureFactAbouBot()
        {
            var q = (from pf in PureFacts.GetList()
                     where pf.Type == PureFactType.AboutBot && pf.IsPlanned == false && pf.IsUsed == false
                     select pf).ToArray();

            if (q.Length > 0)
            {
                string[] group1 = { "BotName" }; //conversation should start with these
                string[] group2 = { "BotAge", "BotSex" }; //conversation should continue with these

                List<ItemProb<string>> itemProbs = new List<ItemProb<string>>();

                //assign probabilities 
                foreach (PureFact fact in q)
                {
                    if (group1.Contains(fact.Name)) //add only 1 item
                    {
                        itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.99)));
                        break;
                    }
                    else
                    if (group2.Contains(fact.Name))
                        itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.25)));
                    else
                        itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.8)));
                }

                var pureFactDistF = CategoricalF(itemProbs.ToArray()).Normalize();

                var pureFactDist = pureFactDistF.ToSampleDist();

                var selectionName = pureFactDist.Sample();

                PureFact selectedPureFact = PureFacts.GetFacfByName(selectionName);

                //SharedHelper.Log("GetPureFactAbouUser: selectionName " + selectionName);

                return selectedPureFact;
            }
            else
            {
                SharedHelper.LogError("GetPureFactAbouBot could not supply a pure fact about the bot.");
                return null;
            }
        }

        public Queue<string> GenerateActions()
        {
            return GenerateActions(new string[] { });
        }
    }
}
