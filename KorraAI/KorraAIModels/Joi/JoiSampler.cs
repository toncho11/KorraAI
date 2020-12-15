using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion.KorraAI.Models.Joi
{
    public class JoiSampler : IKorraAISampler
    {
        ModelContext context;

        System.Random rnd = new System.Random();

        private Queue<string> allSuggestions = new Queue<string>();
        private Queue<string> allActions = new Queue<string>();

        Func<int, bool> AdjFunc;

        IJoiPhrases phrases;

        JoiDistributions dists;

        ItemManager[] providers;

        public void Init(ModelContext context, Func<int, bool> adjFunc, ItemManager[] providers)
        {
            this.context = context;
            this.AdjFunc = adjFunc;
            phrases = new PhrasesEN(providers);
            dists = new JoiDistributions();
            this.providers = providers;
        }

        public void ReGenerateMainSequence(ref Queue<CommItem> Interactions, ItemManager[] p_providers)
        {
            this.providers = p_providers;

            #region debug planned
            string availableItems = "Available items before sampling:\n";
            foreach (var manager in providers)
            {
                availableItems+="Available items for : " + manager.ToString() + " (" + manager.AvailableItems() + ")\n";
                if (!manager.AreAllUnPlanned())
                    SharedHelper.LogError("Flag planned not removed for: " + manager.ToString());
            }

            if ((JokesProvider.GetAll().Where(x => x.IsPlanned==false)).Count() != JokesProvider.GetAll().Count())
                SharedHelper.LogError("Flag planned not removed for: JokeProvider ");
            int availableJokes = JokesProvider.GetAll().Where(x => x.IsPlanned == false && x.IsUsed == false).Count();
            availableItems += "Available items for : " + "JokeProvider" + " (" + availableJokes + ")\n";
            SharedHelper.LogWarning(availableItems);
            #endregion

           PureFacts pfManager = (PureFacts)providers.SingleOrDefault(x => x is PureFacts);
            if (pfManager == null) SharedHelper.LogError("No PureFact manager in ReGenerateMainSequence.");

            //Creates a distribution over "EasilyOffended", IsRomanticJoke
            dists.InitJokesDistribution((PureFact)pfManager.GetByName("EasilyOffended"), false, true);

            //Interactions are built newely generated suggestions and actions
            allSuggestions = GenerateSuggestions();
            allActions = GenerateActions();

            while (allActions.Count > 0 && allSuggestions.Count > 0)
            {
                if (this.AdjFunc(Interactions.Count))
                    allActions = GenerateActions();

                //==================================================================================================

                CommItem citem = new CommItem();
                citem.Category = allActions.Dequeue();

                //SharedHelper.Log("action: " + action);

                string suggestion = "";
                if (citem.Category == ActionsEnum.MakeSuggestion)
                {
                    suggestion = allSuggestions.Dequeue();
                }
                citem.SubCategory = suggestion;

                //Correction
                if (citem.Category == ActionsEnum.AskPureFactQuestionAboutUser) { citem.Category = ActionsEnum.PureFact; citem.SubCategory = ActionsEnum.AskPureFactQuestionAboutUser; }
                if (citem.Category == ActionsEnum.SharePureFactInfoAboutBot) { citem.Category = ActionsEnum.PureFact; citem.SubCategory = ActionsEnum.SharePureFactInfoAboutBot; }

                #region Process All
                ItemManager manager = providers.SingleOrDefault(x => x.Is(citem));

                if (manager != null && !(manager is PureFacts) ) //TODO: to be improved
                {
                    Item item = manager.GetItem();
                    if (item != null)
                    {
                        manager.SetAsPlanned(item.Name);
                        citem = new CommItem(item);

                        if (manager is SongsProvider)
                            citem.TextToSay = ((Song)item).Name;

                        if (manager is MoviesProvider)
                            citem.TextToSay = phrases.MovieAnnouncement((Movie)item);

                        //if (manager is SportsProvider)
                        //    SharedHelper.Log("Sport item added in Joi sampler: " + citem.TextToSay);

                        InteractionsStat.AddScheduledInteraction(citem.Category + citem.SubCategory);
                        //SharedHelper.LogWarning("Added " + citem.Category + citem.SubCategory + " with text: " + citem.TextToSay);
                    }
                    else
                    {
                        InteractionsStat.AddMissingInteraction(citem.Category + citem.SubCategory);
                        SharedHelper.LogWarning("Not enough " + citem.Category + citem.SubCategory + " during planning.");
                    }
                }
                #endregion
                else
                {
                    #region Set Suggestion
                    if (citem.Category == ActionsEnum.MakeSuggestion)
                    {
                        //string suggestion = allSuggestions.Dequeue();

                        //SharedHelper.Log("suggestion: " + suggestion);

                        #region set joke
                        if (suggestion == SuggestionsEnum.TellJoke)
                        {
                            //the fact that joke has bee planned, does not mean it has been executed

                            Joke joke = dists.NextJoke();

                            if (joke != null)
                            {
                                citem = new CommItem(joke);

                                //Planned is handled by "dists"
                                //It gives a distribution from all unplanned and unused

                                JokesProvider.SetJokeAsPlanned(joke.Name);

                                //joke.IsPlanned = true;
                                //citem.Name = joke.Name;
                                //citem.TextToSay = joke.Text;
                                //citem.SubCategory = suggestion;
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

                        #region set Go Out
                        if (suggestion == SuggestionsEnum.GoOut)
                        {
                            citem.TextToSay = phrases.GoOutAnnoucement();
                            InteractionsStat.AddScheduledInteraction(SuggestionsEnum.GoOut);
                        }
                        #endregion

                        //#region set Watch movie
                        //if (suggestion == SuggestionsEnum.WatchMovie)
                        //{
                        //    Movie movie = MoviesProvider.Get();
                        //    if (movie != null)
                        //    {
                        //        movie.IsPlanned = true;
                        //        //citem.Name = movie.Name;
                        //        //citem.TextToSay = phrases.MovieAnnouncement(movie);
                        //        citem = new CommItem(movie);

                        //        //UnityEngine.Debug.LogWarning("Movie scheduled: " + citem.TextToSay);
                        //        InteractionsStat.AddScheduledInteraction(SuggestionsEnum.WatchMovie);
                        //    }
                        //    else
                        //    {
                        //        citem.TextToSay = "__nointeraction__";
                        //        SharedHelper.LogWarning("Not enough movies during planning.");
                        //        InteractionsStat.AddMissingInteraction(SuggestionsEnum.WatchMovie);
                        //    }
                        //}
                        //#endregion

                        #region set Weather forecast
                        if (suggestion == SuggestionsEnum.TellWeatherForecast)
                        {
                            citem.TextToSay = "The weather forecast should be good."; //TODO not translated
                        }
                        #endregion

                        //#region set Song
                        //if (suggestion == SuggestionsEnum.ListenToSong)
                        //{
                        //    //the fact that song has bee planned, does not mean it has been executed
                        //    Song song = SongsProvider.GetSong();
                        //    if (song != null)
                        //    {
                        //        song.IsPlanned = true;
                        //        citem.Name = song.Name;
                        //        citem.TextToSay = song.Name;

                        //        InteractionsStat.AddScheduledInteraction(SuggestionsEnum.ListenToSong);
                        //    }
                        //    else
                        //    {
                        //        citem.TextToSay = "__nointeraction__";
                        //        SharedHelper.LogWarning("Not enough songs during planning.");
                        //        InteractionsStat.AddMissingInteraction(SuggestionsEnum.ListenToSong);
                        //    }
                        //}
                        //#endregion
                        //else
                        //#region set Go to gym
                        //if (suggestion == SuggestionsEnum.DoSport)
                        //{
                        //    ItemManager manager = providers.Single(x => x.Is(citem));

                        //    if (manager == null) SharedHelper.LogWarning("Manager is null");

                        //    Sport sport = (Sport)manager.GetItem();
                        //    if (sport != null)
                        //    {
                        //        manager.SetAsPlanned(sport.Name);
                        //        citem = new CommItem(sport);

                        //        InteractionsStat.AddScheduledInteraction(SuggestionsEnum.DoSport);
                        //    }
                        //    else
                        //    {
                        //        citem.TextToSay = "__nointeraction__";
                        //        SharedHelper.LogWarning("Not enough sports during planning.");
                        //        InteractionsStat.AddMissingInteraction(SuggestionsEnum.DoSport);
                        //    }
                        //}
                        //#endregion

                    } //end action suggestions
                    #endregion
                    else if (citem.Category == ActionsEnum.AskUncertanFactQuestion)
                    {
                        //the actual question is selected at run time
                        citem.TextToSay = "###place holder for UncertanFactQuestion";
                        citem.IsPureFact = false;

                        InteractionsStat.AddScheduledInteraction(ActionsEnum.AskUncertanFactQuestion);
                    }
                    else if (citem.SubCategory == ActionsEnum.AskPureFactQuestionAboutUser) //ABOUT USER
                    {
                        int pfabul = PureFactsAboutUserLeftCount();

                        if (pfabul > 0)
                        {
                            PureFact sf = pfManager.GetPureFactAbouUser();

                            if (sf == null)
                                SharedHelper.LogError("There are pure facts about the user left, but no pure fact has been selected.");
                            else
                            {
                                citem.TextToSay = sf.Question;
                                //SharedHelper.Log("q name: " + q [0].Name);
                                citem.Name = sf.Name;
                                citem.IsPureFact = true;

                                pfManager.SetAsPlanned(sf.Name);

                                citem.UIAnswer = sf.UI;
                            }
                            InteractionsStat.AddScheduledInteraction(citem.Category+citem.SubCategory);
                        }
                        else InteractionsStat.AddMissingInteraction(citem.Category + citem.SubCategory);
                    }
                    else if (citem.SubCategory == ActionsEnum.SharePureFactInfoAboutBot) //ABOUT BOT
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

                                pfManager.SetAsPlanned(sf.Name);
                            }
                            InteractionsStat.AddScheduledInteraction(citem.Category + citem.SubCategory);
                        }
                        else
                            InteractionsStat.AddMissingInteraction(citem.Category + citem.SubCategory);
                    }
                    else if (citem.Category == ActionsEnum.ChangeVisualAppearance)
                    {
                        citem.TextToSay = context.BasePhrases.ChangeClothesAnnouncement();
                        InteractionsStat.AddScheduledInteraction(ActionsEnum.ChangeVisualAppearance);
                    }
                    else if (citem.Category == ActionsEnum.ExpressMentalState)
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
                    else
                    {
                        SharedHelper.LogError("Unknown action: " + citem.Category);
                    }
                }

                #region Add interaction item to queue

                if (string.IsNullOrEmpty(citem.TextToSay))
                    SharedHelper.LogError("Text is empty! Action was: " + citem.Category + "|" + citem.SubCategory);
                else
                {
                    Interactions.Enqueue(citem);
                }

                #endregion

                #region debug
                PureFact[] LeftPureFacts = (from item in pfManager.GetAll()
                                            let pf = (PureFact)item
                                            where (pf.Type == PureFactType.AboutBot || pf.Type != PureFactType.AboutUser)
                                            && pf.IsPlanned == false && pf.IsUsed == false
                                            select pf).ToArray();

                //if (!Flags.DecreaseDistributionOfAskPureFactQuestionAboutUserDone && Interactions.Count > 45 && (LeftPureFacts.Length - PersistentData.PureFactsLoadedOnStartup()) > 0)
                //    KorraBaseHelper.LogError("All pure facts should have been already planned. Left not used pure facts: " + LeftPureFacts.Length);

                #endregion
            }
        }

        private int PureFactsAboutUserLeftCount()
        {
            PureFacts pfManager = (PureFacts)providers.SingleOrDefault(x => x is PureFacts);

            if (pfManager != null)
            {
                var q = (from item in pfManager.GetAll()
                         let pf = item as PureFact
                         where pf.Type == PureFactType.AboutUser && pf.IsPlanned == false && pf.IsUsed == false
                         select pf).ToArray();

                return q.Length;
            }
            else
            {
                SharedHelper.LogError("No manager in PureFactsAboutUserLeftCount");
                return 0;
            }
        }

        private int PureFactsAboutBotLeftCount()
        {
            PureFacts pfManager = (PureFacts)providers.SingleOrDefault(x => x is PureFacts);

            if (pfManager != null)
            {
                var q = (from item in pfManager.GetAll()
                         let pf = item as PureFact
                         where pf.Type == PureFactType.AboutBot && pf.IsPlanned == false && pf.IsUsed == false
                         select pf).ToArray();

                return q.Length;
            }
            else
            {
                SharedHelper.LogError("No manager in PureFactsAboutBotLeftCount");
                return 0;
            }
        }

        private PureFact GetPureFactAbouBot()
        {
            PureFacts pfManager = (PureFacts)providers.SingleOrDefault(x => x is PureFacts);

            if (pfManager == null)
            {
                SharedHelper.LogError("No manager in GetPureFactAbouBot.");
                return null;
            }

            var q = (from item in pfManager.GetAll()
                     let pf = item as PureFact
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

                PureFact selectedPureFact = (PureFact)pfManager.GetByName(selectionName);

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

            #region Disable actions - NOT ADAPTED for category and subcategory
            if (BotConfigShared.DisableAskQuestions)
            {
                SharedHelper.LogError("NOT ADAPTED for category and subcategory");

                //two actions are removed
                //DisableAcion(ref items, ActionsEnum.AskPureFactQuestionAboutUser);
                //DisableAcion(ref items, ActionsEnum.AskUncertanFactQuestion);
            }

            //foreach (string da in disabledActions)
            //{
            //    DisableAcion(ref items, da);
            //}
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

        //NOT ADAPTED for category and subcategory
        private static void DisableAcion(ref ItemProb<string>[] items, string actionToRemove)
        {
            SharedHelper.Log("Error category PureFact");

            SharedHelper.Log("Action removed: " + actionToRemove.ToString());

            items = items.Where(x => x.Item != actionToRemove).ToArray();
        }

        private Queue<string> GenerateSuggestions()
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

                    ItemProb(SuggestionsEnum.DoSport, SharedHelper.GetProb(ProbVariables.Bot.SuggestDoSport))

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

    }
}
