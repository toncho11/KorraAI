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
        bool RomanticJokesFirst = false;
        bool RomanticJokesLast = true;

        /// <summary>
        /// After how many minutes to increase the music suggestions
        /// </summary>
        readonly int MinutesIncreaseMusic = 15; //default 15;

        KorraAISampler korraSampler;

        IDistributions cognitiveDist;

        public event EventHandler ContextLoaded;

        bool isInitialized = false;

        ModelContext context;

        IModelTrigger[] ModelTriggers;

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

        public void Init()
        {
            #region Configure character

            Personality.UseMildOffensiveJokes = true;
            Personality.UseMildSexualThemes = true;
            Personality.UseRomanticReferences = true;

            #endregion

            context = new ModelContext();

            if (BotConfigShared.Language == Lang.EN)
            {
                context.Phrases = new PhrasesEN();

                context.Items = new ItemsEN();

                context.SpeechAdaptation = new SpeechAdaptationEN();
            }
            //else
            //if (BotConfigShared.Language == Lang.FR)
            //{
            //    context.Phrases = new PhrasesFR();

            //    context.Items = new ItemsFR();

            //    context.SpeechAdaptation = new SpeechAdaptationFR();
            //}
            else
            {
                SharedHelper.LogError("Language pack not found.");
            }

            context.Items.LoadAll(context.Phrases);

            cognitiveDist = new JoiDistributions();

            ModelTriggers = new IModelTrigger[]
            {
                new MusicModelUpdateTrigger(MinutesIncreaseMusic),
                new MoviesModelUpdateTrigger(),
                new VideoGameSurpriseTrigger(),
            };

            RomanticJokesFirst = true;
            RomanticJokesLast = false;

            korraSampler = new KorraAISampler();
            korraSampler.Init(context, AdjustProbVariablesDuringPlanning, GetItem);

            //Delayed. These Uncertain Facts questions are automatically re-enabled after a certain period of time
            UncertainFacts.SetAsUsed("UserIsTired");
            UncertainFacts.SetAsUsed("UserGoodMood");

            //cache values
            cognitiveDist.NextSmilePauseTime();
            cognitiveDist.NextTimeDurationEyesStayFocusedOnCameraAfterStartedTalking();
            cognitiveDist.GetNextInteactionPause(false);
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
            if (!isInitialized) SharedHelper.LogError("Not initialized.");

            bool regenerationRequested = false;

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

        public IDistributions GetCognitiveDist()
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
            if (ProbVariables.Bot.PrSharePureFactInfoAboutBot[(int)PV.Current].Value != 0 && KorraModelHelper.GetItemsLeftForCategory(ActionsEnum.SharePureFactInfoAboutBot) == 0)
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
            if (ProbVariables.Bot.PrAskPureFactQuestionAboutUser[(int)PV.Current].Value != 0 && KorraModelHelper.GetItemsLeftForCategory(ActionsEnum.AskPureFactQuestionAboutUser) == 0)
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

        private Joke GetJoke()
        {
            Joke selectedJoke;

            PureFact fact = PureFacts.GetFacfByName("EasilyOffended");

            if (fact == null) return null;

            var dist = cognitiveDist.JokesDistribution(fact, this.RomanticJokesFirst, this.RomanticJokesLast);

            if (dist == null) return null;

            var selectionName = dist.Sample();

            selectedJoke = JokesProvider.GetJokeByName(selectionName);

            //SharedHelper.Log("GetPureFactAbouUser: selectionName " + selectionName);

            return selectedJoke;
        }

        /// <summary>
        /// This method is provided to the sampler
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public Item GetItem(string category)
        {
            if (category == SuggestionsEnum.TellJoke)
            {
                var joke = GetJoke();

                return joke;
            }

            return null;
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
            if (item.IsReactionToUser)
            {
                //If it is a reaction, we check if a facial expression was already supplied
                if (!string.IsNullOrEmpty(item.FacialExpression))
                    KorraModelHelper.SetFacialExpressionFlag(item.FacialExpression);
                else
                    //Default to smile if not set in the reaction itself
                    FlagsShared.RequestSmileAfterTalkingDone = true;
            }
            else
            if (item.IsPureFact)
            {
                PureFact fact = PureFacts.GetFacfByName(item.Name);

                if (fact != null && fact.Type == PureFactType.AboutBot) KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
            }
            else
            if (item.Action == ActionsEnum.ChangeVisualAppearance)
                KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
            else
            if (item.Action == ActionsEnum.ExpressMentalState)
                KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);
            else
            if (item.Suggestion == SuggestionsEnum.TellJoke)
            {
                Joke joke = JokesProvider.GetJokeByName(item.Name);

                if (joke != null && !string.IsNullOrEmpty(item.FacialExpression))
                {
                    KorraModelHelper.SetFacialExpressionFlag(joke.FaceExpression);
                }
            }
            else
            if (item.Suggestion == SuggestionsEnum.ListenToSong ||
                item.Suggestion == SuggestionsEnum.GoToGym
                || item.Suggestion == SuggestionsEnum.WatchMovie
                || item.Suggestion == SuggestionsEnum.GoOut)
                KorraModelHelper.SetFacialExpressionFlag(FaceExp.SmileAfterTalking);

        }
    }
}
