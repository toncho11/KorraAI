using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Companion.KorraAI;

namespace Companion.KorraAI.Models.Joi
{
    public class SpeechAdaptationEN : ISpeechAdaptation
    {
        #region variables
        private static bool OneSongAlreadyPlanned = false;
        private static string lastSongAnnouncementUsed = "";
        #endregion

        IJoiPhrases phrases;

        public SpeechAdaptationEN(IJoiPhrases phrases)
        {
            this.phrases = phrases;// new PhrasesEN();
        }

        public Queue<CommItem> ProcessItems(Queue<CommItem> input, ItemManager[] managers)
        {
            PureFacts pfManager = (PureFacts)managers.SingleOrDefault(x => x is PureFacts);
            if (pfManager == null) { SharedHelper.LogError("No Pure Fact Manager in SpeechAdaptationEN."); return null; }

            List<CommItem> list = input.ToList();

            if (!FlagsShared.InitialGreetingPerformed)
            {
                list.Insert(0, new CommItem { TextToSay = phrases.SayHello(), IsGreeting = true });
                FlagsShared.InitialGreetingPerformed = true;
            }

            string username = pfManager.GetValueByName("UserName");

            for (int i = 0; i < list.Count; i++)
            {
                //add joke announcement
                //if (list[i].Action == Actions.MakeSuggestion && list[i].Suggestion == Suggestions.TellJoke)
                //{
                //    CommItem item = list[i];
                //    item.TextToSay = AddJokeAnnouncement(item.TextToSay);
                //    list[i] = item;
                //}

                //add song announcement
                if (list[i].Category == ActionsEnum.MakeSuggestion && list[i].SubCategory == SuggestionsEnum.ListenToSong)
                {
                    CommItem item = list[i];
                    item.TextToSay = AddSongAnnouncement(item.TextToSay, OneSongAlreadyPlanned);
                    list[i] = item;

                    OneSongAlreadyPlanned = true;
                }

                // add user name for the interaction
                if (list[i].Category == ActionsEnum.AskUncertanFactQuestion
                    || list[i].SubCategory == ActionsEnum.AskPureFactQuestionAboutUser
                    || list[i].Category == ActionsEnum.ChangeVisualAppearance
                    || (list[i].Category == ActionsEnum.MakeSuggestion && list[i].SubCategory != "" && list[i].SubCategory != SuggestionsEnum.TellJoke)
                    )
                {
                    CommItem item = list[i];
                    item.TextToSay = AddCallByName(item.TextToSay, username);
                    list[i] = item;
                }
            }

            DisablePlayMusicAftertInitialGreeting(ref list, managers);

            KorraModelHelper.CoupleTwoInteractionsTogether(ref list, "UserName", "BotName");

            KorraModelHelper.CoupleTwoInteractionsTogether(ref list, "UserLikesVideoGames", "UserThinksVideoGameIsGoodPresent");

            return new Queue<CommItem>(list);
        }

        //Avoid last one used? YES
        private string AddSongAnnouncement(string originalText, bool oneSongAlreadyPlanned)
        {
            string text = "";

            if (oneSongAlreadyPlanned || StatesShared.OneSongAlreadyPlayed)
            {
                string value = KorraModelHelper.GetChance(new string[] { "I have a new song for you: ", "Another song: ", "You are going to like this tune: ", "Listen to this song: ", "I am going to play some music: " }, lastSongAnnouncementUsed);
                lastSongAnnouncementUsed = value;
                text += (value + originalText) + ".";
            }
            else
            {
                text += KorraModelHelper.GetChance(new string[] { "I have a nice song for you: ", "Listen to this song: ", "I am going to play some music: " });
                text += originalText + ".";

                text += " Please note that pressing " + ((FlagsShared.IsMobile) ? "Back button" : "Escape") + " at anytime will stop playback.";
            }

            return text;
        }

        public static string AddJokeAnnouncement(string originalText)
        {
            string text = "";

            string start = "<prosody pitch=\"+0%\">";
            string pause = "<break time=\"800ms\"/>"; //maybe it should not be fixed
            string end = "</prosody>";

            text = KorraModelHelper.GetChance(new string[] {
                                       start + "Joke time. " + pause + originalText + end,
                                       start + "I would like to tell you a joke. " + pause + originalText + end,
                                       start + originalText + pause + " OK, that was a joke." + end,
                                       originalText
                                      });

            return text;
        }

        private string AddCallByName(string originalText, string username)
        {
            if (!string.IsNullOrEmpty(username) && (KorraModelHelper.GetChance(3))) //33% chance to use the name
            {
                return KorraModelHelper.FirstCharToUpper(username) + ", " + KorraModelHelper.FirstCharToLower(originalText);
            }
            else return originalText;
        }

        private void DisablePlayMusicAftertInitialGreeting(ref List<CommItem> list, ItemManager[] managers)
        {
            if (list.Count > 0
                && list[0].IsGreeting //to make sure it is the start of the session
                && list[1].Category == ActionsEnum.MakeSuggestion
                && list[1].SubCategory == SuggestionsEnum.ListenToSong)
            {
                KorraModelHelper.RemoveInteraction(ref list, 1, managers);
            }
        }
    }
}

