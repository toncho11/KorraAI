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
        /// <summary>
        /// After how many minutes to increase the music suggestions
        /// </summary>
        readonly int MinutesIncreaseMusic = 15; //default 15;

        IKorraAISampler korraSampler;

        IBaseDistributions cognitiveDist;

        public event EventHandler ContextLoaded;

        bool isInitialized = false;

        ModelContext context;

        IModelTrigger[] ModelTriggers;

        ItemManager[] itemProviders;

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
                return "1.3";
            }
        }

        public void Init(DateTime? LastSessionDateTime)
        {

            #region Configure character

            Personality.UseMildOffensiveJokes = true;
            Personality.UseMildSexualThemes = false;
            Personality.UseRomanticReferences = false;

            #endregion

            context = new ModelContext();

            //Select data providers
            itemProviders = new ItemManager[] { new SportsProvider(new Sport("")),
                                                new PureFacts(new PureFact()),
                                                new MoviesProvider(new Movie("",false,false)),
                                                new SongsProvider(new Song("",""))
                                              };

            if (BotConfigShared.Language == Lang.EN)
            {
                context.BasePhrases = new PhrasesEN(itemProviders);

                context.Items = new ItemsEN((PhrasesEN)context.BasePhrases);

                context.SpeechAdaptation = new SpeechAdaptationEN((IJoiPhrases)context.BasePhrases);
            }
            else
            if (BotConfigShared.Language == Lang.FR)
            {
                //context.BasePhrases = new PhrasesFR(itemProviders);

                //context.Items = new ItemsFR();

                //context.SpeechAdaptation = new SpeechAdaptationFR((IJoiPhrases)context.BasePhrases);
            }
            else
            {
                SharedHelper.LogError("Language pack not found.");
            }

            context.Items.LoadAll(itemProviders);

            foreach (var provider in itemProviders)
            {
                if (provider.Count() == 0) SharedHelper.LogError("Detected an item manager without items.");
            }

            //ReuseInteractions(PersistentData.GetLastSession()); //some already used interactions are re-enabled

            cognitiveDist = new JoiDistributions();

            ModelTriggers = new IModelTrigger[]
            {
                new MusicModelUpdateTrigger(MinutesIncreaseMusic),
                new MoviesModelUpdateTrigger(),
                new VideoGameSurpriseTrigger((IJoiPhrases)context.BasePhrases),
            };

            korraSampler = new JoiSampler();
            korraSampler.Init(context, AdjustProbVariablesDuringPlanning, itemProviders);

            //Delayed. These Uncertain Facts questions are automatically re-enabled after a certain period of time
            UncertainFacts.SetAsUsed("UserIsTired");
            UncertainFacts.SetAsUsed("UserGoodMood");
            UncertainFacts.SetAsUsed("BotAsksIfJokeRateIsOK"); 
            UncertainFacts.SetAsUsed("BotChangeOutfit");                                              

            //cache values
            cognitiveDist.NextSmilePauseTime();
            cognitiveDist.NextTimeDurationEyesStayFocusedOnCameraAfterStartedTalking();
            cognitiveDist.GetNextInteactionPause(false);
            cognitiveDist.NextQuestionTimeout();

            isInitialized = true;
            ContextLoaded(this, EventArgs.Empty);
        }

        public IKorraAISampler GetSampler()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return korraSampler;
        }

        public ModelContext GetContext()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return context;
        }

        public IModelTrigger[] GetModelTriggers
        {
            get
            {
                if (!isInitialized) SharedHelper.LogError("Not initialized.");
                return ModelTriggers;
            }
        }

        public bool ModelUpdate(TimeSpan timeSinceStart, bool isPureFactUpdated, bool isUncertainFactUpdated)
        {
            bool regenerationRequested = false;

            if (!isInitialized) { SharedHelper.LogError("Not initialized."); return regenerationRequested; }

            //Execute triggers that perform inference and update probabilistic variables 
                foreach (var trigger in ModelTriggers)
            {
                int oldExecuteCount = trigger.TriggeredCount;

                if (
                       (trigger is IModelUpdateTrigger)
                       &&
                       (
                         (trigger.IsTimeBased || (isPureFactUpdated && trigger.IsUserResponseBased))
                         &&
                         ((trigger.IsOneTimeTrigger && trigger.TriggeredCount == 0) || !trigger.IsOneTimeTrigger)

                       )
                   )
                {
                    //Updating model
                    regenerationRequested = ((IModelUpdateTrigger)trigger).Process(isPureFactUpdated, timeSinceStart, this);

                    if (trigger.TriggeredCount > oldExecuteCount)
                        SharedHelper.Log("Trigger executed: '" + trigger.ToString() + "'");
                }
            }

            if (isUncertainFactUpdated)
                regenerationRequested = true;

            return regenerationRequested;
        }

        public void InteractionsUpdate(TimeSpan timeSinceStart, int interactionsDoneSinceStart, ref Queue<CommItem> interactions)
        {
            if (!isInitialized) { SharedHelper.LogError("Not initialized."); return; }

            //Evaluate triggers that represent 'Surprise' for example
            //These triggers evaluate a model and add a new interaction
            foreach (var trigger in ModelTriggers)
            {
                int oldExecuteCount = trigger.TriggeredCount;

                if (trigger is IModelEvaluateTrigger &&
                    (
                      (trigger.IsOneTimeTrigger && trigger.TriggeredCount == 0)
                      ||
                      (!trigger.IsOneTimeTrigger)
                    )
                   )
                {
                    CommItem? newInteraction = ((IModelEvaluateTrigger)trigger).Process(this);

                    if (newInteraction != null)
                    {
                        if (interactions.Peek().Name != newInteraction.Value.Name)
                        {
                            KorraModelHelper.InsertFirstInteractionList(ref interactions, newInteraction.Value);

                            //TODO: this custom code should be moved to another place
                            if (trigger is VideoGameSurpriseTrigger)
                                FlagsShared.RequestSurpriseExpression = true;

                            SharedHelper.Log("Trigger executed: '" + trigger.ToString() + "'");
                        }
                    }
                }
            }
        }

        public void BeforeAnalyseUserResponse(PureFact pfact)
        {
            //Here you change the responses that were encoded when creating this PureFact, even replace the function responsible for the answers.
            //This can be useful if you have new information about the user and its environment and this could not be encoded in advance.

            bool isAnswerd = pfact.IsAnswered;
            string responseValue = pfact.Value;

            //if (context.Phrases.IsYes(responseValue))
            //{
            //    fact.StatementOnPositiveResponse = "";
            //}
            SharedHelper.Log("Pure fact updated: " + pfact.Name);
        }

        public IBaseDistributions GetCognitiveDist()
        {
            if (!isInitialized) SharedHelper.LogError("Not initialized.");
            return cognitiveDist;
        }

        public void InspectNextInteraction(CommItem nextInteraction)
        {

        }

        public bool AdjustProbVariablesDuringPlanning(int bufferedInteractionsCount)
        {
            bool regenerate = false;

            //1. About bot
            //exclude this action because there are no items for it
            if (ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value != 0 && KorraModelHelper.GetItemsLeftForSubCategory(ActionsEnum.SharePureFactInfoAboutBot, ItemProviders) == 0)
            {
                //adjust pure facts probabilities: disable AboutBot and boost AboutUser 

                ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current] = Prob(0);
                SharedHelper.LogWarning("Disabled all pure facts about bot, because there were no items.");

                //we re-inforce AboutUser so that it is stronger than Suggestion action
                if (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value > 0)
                    ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current] = ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Increased];

                regenerate = true;
            }

            //2. About User
            //exclude this action because there are no items for it
            if (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value != 0 && KorraModelHelper.GetItemsLeftForSubCategory(ActionsEnum.AskPureFactQuestionAboutUser, ItemProviders) == 0)
            {
                //adjust pure facts probabilities: disable AboutUser and boost AboutBot

                ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current] = Prob(0);
                SharedHelper.Log("Disabled all pure facts about user, because there were no items.");

                //we re-inforce AboutBot so that it is stronger than Suggestion action
                if (ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value > 0)
                    ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current] = ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Increased];

                regenerate = true;
            }

            //3. Suggestions
            //if there are no Pure or Uncertain facts left then we boost suggestions
            bool noFactsLeft = (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value == 0 && ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value == 0);

            if (noFactsLeft && ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] != ProbVariables.Bot.PrMakeSuggestion[(int)PV.Default])
            {
                ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] = ProbVariables.Bot.PrMakeSuggestion[(int)PV.Default];
                SharedHelper.Log("All pure facts used. PrMakeSuggestion changed to: " + ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current]);
            }

            //Keep suggestions decreased if there are more pure facts 
            if (!noFactsLeft && ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] != ProbVariables.Bot.PrMakeSuggestion[(int)PV.Descreased])
            {
                SharedHelper.Log("PrMakeSuggestion changed decreased.");
                ProbVariables.Bot.PrMakeSuggestion[(int)PV.Current] = ProbVariables.Bot.PrMakeSuggestion[(int)PV.Descreased];
            }

            return regenerate;
        }

        public bool SmileOnNoReactionToUserResponse
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Configures when facial expressions are used
        /// </summary>
        /// <param name="item"></param>
        public void SetFacialExpression(CommItem item)
        {
            #region Get Fact Manager
            PureFacts pfManager = (PureFacts)ItemProviders.SingleOrDefault(x => x is PureFacts);
            if (pfManager == null)
            {
                SharedHelper.LogError("No manager in Facts Manager in SetFacialExpression for model " + this.Name);
                return;
            }
            #endregion

            //PureFacts and Jokes will facial expression set at this stage or by Dynamic Function
            //it updates the facial expression, if it is empty

            if (!string.IsNullOrEmpty(item.FacialExpression))
            {
                KorraModelHelper.SetFacialExpressionFlag(item.FacialExpression);
            }
            else //here in most cases we default to smile
            {
                if (item.IsReactionToUser) //default to smile on reaction
                {
                    KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
                }
                else
                //Handle PureFacts AboutBot
                if (item.IsPureFact && !item.IsJokePureFact)
                {
                    PureFact fact = (PureFact)pfManager.GetByName(item.Name);

                    if (fact != null && fact.Type == PureFactType.AboutBot) KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
                }
                else
                if (item.Category == ActionsEnum.ChangeVisualAppearance)
                    KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
                else
                if (item.Category == ActionsEnum.ConvinceBuyStatement)
                    KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
                else
                if (item.Category == ActionsEnum.ExpressMentalState)
                    KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
                else
                //Hnadle Jokes (Normal or PureFacts)
                if (item.SubCategory == SuggestionsEnum.TellJoke && !item.IsJokePureFact)
                {
                    if (!item.IsJokePureFact) //Normal joke
                    {
                        Joke joke = JokesProvider.GetJokeByName(item.Name);

                        if (joke != null && !string.IsNullOrEmpty(joke.FaceExpression)) //check for custom facial expression
                        {
                            KorraModelHelper.SetFacialExpressionFlag(joke.FaceExpression);
                        }
                        else
                            KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking); //default to smile
                    }
                    else //PureFact joke that had no facial expression set
                    {
                        KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking); //default to smile
                    }
                }
                else
                if (item.SubCategory == SuggestionsEnum.ListenToSong ||
                    item.SubCategory == SuggestionsEnum.DoSport
                    || item.SubCategory == SuggestionsEnum.WatchMovie
                    || item.SubCategory == SuggestionsEnum.GoOut)
                    KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
            }

        }

        //private static void ReuseInteractions(DateTime? lastSessionDateTime)
        //{
        //    int threshold = 5;

        //    //Pure facts about the bot are re-enabled (used=false), 
        //    //because the user forgot about the bot, so the bot will present itself again: name, age, etc
        //    if (KorraModelHelper.UseAgainAlreadyUsedInteractions(lastSessionDateTime, threshold))
        //    {
        //        if (BotConfig.TEST) UnityEngine.Debug.Log(threshold + " days apart policy applied on PureFacts");

        //        UnityEngine.Debug.Log("ReuseInteractions disabled");
        //        #region re-enable facts
        //        //var q = from fact in PureFacts.GetAll()
        //        //        where fact.Type == PureFactType.AboutBot
        //        //        select fact;

        //        //foreach (PureFact fact in q.ToArray())
        //        //{
        //        //    PureFacts.SetAsUsed(fact.Name, false);
        //        //}
        //        #endregion
        //    }

        //    //re-enable som jokes if the last time the bot was used is long time ago
        //    threshold = 40;
        //    if (KorraModelHelper.UseAgainAlreadyUsedInteractions(lastSessionDateTime, threshold))
        //    {
        //        if (BotConfig.TEST) UnityEngine.Debug.Log(threshold + " days apart policy applied on Jokes");

        //        foreach (Joke joke in JokesProvider.GetAll())
        //        {
        //            joke.IsUsed = false;
        //        }
        //    }
        //}

        public void FilterStoredFacts(ref List<Item> items)
        {
        }

        public ItemManager[] ItemProviders
        {
            get
            {
                return itemProviders;
            }
        }

    }
}
