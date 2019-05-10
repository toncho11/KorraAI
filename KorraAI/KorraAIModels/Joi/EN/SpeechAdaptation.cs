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

        public Queue<CommItem> ProcessItems(Queue<CommItem> input, IPhrases phrases)
        {
            List<CommItem> list = input.ToList();

            if (!FlagsShared.InitialGreetingPerformed)
            {
                list.Insert(0, new CommItem { TextToSay = phrases.SayHello(), IsGreeting = true });
                FlagsShared.InitialGreetingPerformed = true;
            }

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
                if (list[i].Action == ActionsEnum.MakeSuggestion && list[i].Suggestion == SuggestionsEnum.ListenToSong)
                {
                    CommItem item = list[i];
                    item.TextToSay = AddSongAnnouncement(item.TextToSay, OneSongAlreadyPlanned);
                    list[i] = item;

                    OneSongAlreadyPlanned = true;
                }

                // add user name for the interaction
                if (list[i].Action == ActionsEnum.AskUncertanFactQuestion
                    || list[i].Action == ActionsEnum.AskPureFactQuestionAboutUser
                    || list[i].Action == ActionsEnum.ChangeVisualAppearance
                    || (list[i].Action == ActionsEnum.MakeSuggestion && list[i].Suggestion != "" && list[i].Suggestion != SuggestionsEnum.TellJoke)
                    )
                {
                    CommItem item = list[i];
                    item.TextToSay = AddCallByName(item.TextToSay);
                    list[i] = item;
                }
            }
            return new Queue<CommItem>(list);
        }

        //Avoid last one used? YES
        private string AddSongAnnouncement(string originalText, bool oneSongAlreadyPlanned)
        {
            string text = "";

            if (oneSongAlreadyPlanned || StatesShared.OneSongAlreadyPlayed)
            {
                string value = KorraModelHelper.GetChance(new string[] { "I have a new song for you: ", "Another song: ", "You are going to like this tune: ", "Listen to this song: " }, lastSongAnnouncementUsed);
                lastSongAnnouncementUsed = value;
                text += (value + originalText) + ".";
            }
            else
            {
                if (KorraModelHelper.GetChance(2))
                    text = ("I have a nice song for you: " + originalText + ".");
                else text = ("Listen to this song: " + originalText + ".");

                text += " Please note that pressing " + ((FlagsShared.IsMobile) ? "Back button" : "Escape") + " at anytime will stop playback.";
            }

            return text;
        }

        //public static string AddJokeAnnouncement(string originalText)
        //{
        //    string text = "";
        //    System.Random rnd = new System.Random();

        //    int value = rnd.Next(0, 4);

        //    switch (value)
        //    {
        //        case 0: text += ("Joke time. " + originalText); break; //TODO: add pause between the two
        //        case 1: text += ("I would like to tell you a joke. " + originalText); break; //TODO: add pause between the two
        //        case 2: text += (originalText + " That was a joke."); break; //TODO: add pause between the two
        //        case 3: text += originalText; break; //nothing is added
        //    }

        //    return text;
        //}

        private string AddCallByName(string originalText)
        {
            string Username = PureFacts.GetValueByName("UserName");

            if (!string.IsNullOrEmpty(Username) && (KorraModelHelper.GetChance(3))) //33% chance to use the name
            {
                return KorraModelHelper.FirstCharToUpper(Username) + ", " + KorraModelHelper.FirstCharToLower(originalText);
            }
            else return originalText;
        }

        
    }
}
