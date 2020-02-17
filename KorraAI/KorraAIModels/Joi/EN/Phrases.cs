using ProbCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Companion.KorraAI;

namespace Companion.KorraAI.Models.Joi
{
    public class PhrasesEN : IPhrases
    {
        #region last used
        private string lastClothesAnnouncementUsed = "";
        private string lastGoOutAnnouncementUsed = "";
        private string lastMusicFailure = "";
        private string lastBotGoodMood = "";
        private string lastILookGood = "";
        #endregion

        public string SayHello()
        {
            PureFact numberOfStarts = PureFacts.GetFacfByName("SystemStartsCount");
            int numberOfStartsInt = Convert.ToInt32(numberOfStarts.Value);

            string text = "";

            if (!string.IsNullOrEmpty(PureFacts.GetValueByName("UserName"))) //we know how the user's name
            {
                text = KorraModelHelper.GetChance(new string[] { "Hi", "Hello", "Hey", });

                if (KorraModelHelper.GetChance(3) && numberOfStartsInt > 1 && (text == "Hi" || text == "Hello")) //again version
                    text += " again";

                text += " " + PureFacts.GetValueByName("UserName");
            }
            else //we do NOT know how the user's name
            {
                if (KorraModelHelper.GetChance(3) && numberOfStartsInt > 1)
                    text = KorraModelHelper.GetChance(new string[] { "Hi again", "Hello again", }); //again version
                else
                    text = KorraModelHelper.GetChance(new string[] { "Hi", "Hello", "Hey", "Hello there", "Hey buddy" });
            }
            return text;
        }

        public string SayGoodBye()
        {
            string text = "OK, ";

            string Username = PureFacts.GetValueByName("UserName");
            if (!string.IsNullOrEmpty(Username)) Username = KorraModelHelper.FirstCharToUpper(Username);

            text += KorraModelHelper.GetChance(new string[] { (KorraModelHelper.GetChance(3)) ? "bye" : "goodbye", "See you soon", "Have a good day" });

            if (!string.IsNullOrEmpty(Username)) text += " " + Username;
            text += ".";

            return text;
        }

        public string BotName()
        {
            return "Joi";
        }

        public string IamAWoman()
        {
            return "<prosody pitch=\"+0%\">I am a nice looking lady.<break time=\"700ms\"/> Just admit it.</prosody>";
        }

        public string ChangeClothesAnnouncement()
        {
            string text = KorraModelHelper.GetChance(new string[] { "I am going to change my clothes. I feel better like this.", "Time for another outfit.", "Time for some change!", "I am in a mood to change my outfit." }, lastClothesAnnouncementUsed);
            lastClothesAnnouncementUsed = text;
            return text;
        }

        public string GoOutAnnoucement()
        {
            PureFact factJob = PureFacts.GetFacfByName("UserHasJob");

            bool userWorks = factJob.IsAnswered && this.IsYes(factJob.Value);

            var options = (new string[]
                         { "You should go out this evening with your friends.",
                           "Call somebody and go for a drink this evening.",
                           "Today is " + KorraModelHelper.DayOfWeek() + ". It is pizza day. You better have something to eat outside." //TODO: check if it evening, or lunch time
                         }).ToList();

            if (userWorks)
            {
                options.Add("Call some friends and go for a drink after work.");
                options.Add("Going to a bar after work sounds like a great idea");
                options.Add("Do you have plans for this evening after work? You better go out and have a drink at the local pub.");
            }
            else
            {
                options.Add("Call some friends and go for a drink.");
                options.Add("Going to a bar this evening sounds like a great idea.");
                options.Add("Do you have plans for this evening? You better go out and have a drink at the local pub.");
            }

            //choose a statement randomly
            string text = KorraModelHelper.GetChance(options.ToArray(), lastGoOutAnnouncementUsed);

            lastGoOutAnnouncementUsed = text;

            return text;
        }

        public string MovieAnnouncement(Movie movie)
        {
            string type = ((movie.IsTvSeries) ? "TV series" : "movie");
            string movieName = movie.Text;

            string text = KorraModelHelper.GetChance(new string[] {
                                                           "How about this " + type + " " + movieName + "? I recommend it.", //1
                                                           "I have a new " + type + " suggestion for you. It is called : " + movieName + ". You should try it.", //2
                                                           (KorraModelHelper.GetChance(3) ? "Do you feel bored? " : "") + "I recommend you to watch the " + type + " " + movieName + ".", //3
                                                           "Time for something interesting to watch. Try this one: " + movieName + ".", //4
                                                           "You are going to like this " + type + ": " + movieName + ". I highly recommend it." //5
                                                        });

            if (KorraModelHelper.GetChance(3))
                text += " Its score on the Internet movie database is high.";

            if (KorraModelHelper.GetChance(5))
                text += " It is really good. You will thank me later.";

            if (KorraModelHelper.GetChance(10))
                text += " It is one of my favorite.";

            return text;
        }

        public string ExitQuestion()
        {
            if (KorraModelHelper.GetChance(2))
                return "You want to go?";
            else return "You need to go?";
        }

        public string ILookGood() //currently not used
        {
            string value = KorraModelHelper.GetChance(new string[] { "I really look gorgeous!",
                                                                     "I look <prosody rate=\"-40%\">awe</prosody><prosody volume=\"loud\">some</prosody>.",
                                                                   }, lastILookGood);

            lastILookGood = value;
            if (KorraModelHelper.GetChance(3))
                value = "I think " + value;

            return value;
        }

        public string VideoPlaybackError()
        {
            string value = KorraModelHelper.GetChance(new string[] { "Sorry. Music playback did not work.", "Oops. There was a problem with music playback.", }, lastMusicFailure);

            lastMusicFailure = value;
            if (KorraModelHelper.GetChance(3))
            {
                if (KorraModelHelper.GetChance(2))
                    value += " I know it is a ridiculous question, but ";
                value += " Are you sure you are connected to the internet? ";
                if (KorraModelHelper.GetChance(2))
                    value += " Please, play some Youtube videos to verify that.";
            }
            return value;
        }

        public string TellHowToInvokeNewOutfit(int n)
        {
            return " Press " + n + " and I will put it on any time."; //click on the keyboard
        }

        public bool IsYes(string input)
        {
            if (input.ToLower() == "yes") return true; else return false;
        }

        public bool IsNo(string input)
        {
            if (input.ToLower() == "no") return true; else return false;
        }

        public string Yes()
        {
            return "Yes";
        }

        public string No()
        {
            return "No";
        }

        public string ProcessAge(string value, out bool isValid, out string faceExpr)
        {
            int result;
            isValid = Int32.TryParse(value, out result);
            faceExpr = "";

            if (!isValid)
            {
                //FlagsShared.RequestSurpriseExpression = true;
                faceExpr = FaceExp.SurpriseOnStartTalking;
                return "Sorry, I could not get your age.";
            }

            if (isValid && result <= 10)
            {
                //FlagsShared.RequestSurpriseExpression = true;
                //faceExpr = FaceExp.SurpriseOnStartTalking;
                faceExpr = FaceExp.SurpriseOnStartTalking;
                return "Wow, Aren't you a bit young?";
            }
            else if (isValid && result > 10 && result <= 19)
            {
                return "OK, a teenager.";
            }
            else if (isValid && result > 99) //TODO: put a surprise face
            {
                //FlagsShared.RequestSurpriseExpression = true;
                faceExpr = FaceExp.SurpriseOnStartTalking;
                return "No kidding... That sounds like a lot! Are you sure about that?";
            }

            return "";
        }

        public bool IsAnsweredAndUserIsMan()
        {
            PureFact fact = PureFacts.GetFacfByName("UserSex");

            if (fact.IsAnswered && fact.Value.ToLower() == Male().ToLower()) //should be changed for other languages
                return true;

            return false;
        }

        public bool IsAnsweredAndUserIsWoman()
        {
            PureFact fact = PureFacts.GetFacfByName("UserSex");

            if (fact.IsAnswered && fact.Value.ToLower() == Female().ToLower()) //should be changed for other languages
                return true;

            return false;
        }

        public string Male()
        {
            return "Male";
        }

        public string Female()
        {
            return "Female";
        }

        public string ExpressBotGoodMood()
        {
            //TODO: currently does not use probabilstic variable to encode the inAGoodMood

            string response = "";

            response = KorraModelHelper.GetChance(new string[] {
                       "I am doing OK.",
                       "I am in a good mood today."
                        }, lastBotGoodMood);

            lastBotGoodMood = response;

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">is the answer to question: UserRelationshipFirstStep</param>
        /// <returns></returns>
        public string ProcessNationality(string value, out bool isValid, out string faceExpr)
        {
            bool validNationality = KorraModelHelper.CheckValidNationality(value);

            isValid = true;
            faceExpr = "";

            bool validCountryName = false;
            if (!validNationality)
                validCountryName = KorraModelHelper.CheckValidCountryName(value);

            string[] uk = { "uk", "english", "scot", "welsh", "irish", "scottish", "united kingdom", "british" };
            string[] sc = { "zerg", "protoss", "terran" };
            string[] st = { "vulcan", "romulan", "klingon", "borg" };

            if (!validNationality && !validCountryName && !uk.Contains(value.ToLower()) && !sc.Contains(value.ToLower()))
            {
                //FlagsShared.RequestSurpriseExpression = true;
                faceExpr = FaceExp.SurpriseOnStartTalking;
                isValid = false;
                return "Sorry, I could not understand your nationality.";
            }

            else
            {
                if (value.ToLower() == "french" || value.ToLower() == "france")
                    return "Hands down, best wines and cheese.";
                else if (value.ToLower() == "american" || value.ToLower() == "us" || value.ToLower() == "usa")
                    return "<prosody pitch=\"+0%\">Yeah, the problem with political jokes is that <break time=\"800ms\"/>they get elected.</prosody>"; //maybe it should be in jokes list
                else if (value.ToLower() == "canadian" || value.ToLower() == "canada")
                    return "<prosody pitch=\"+0%\">What do you call a sophisticated American?<break time=\"900ms\"/> A canadian.</prosody>"; //maybe it should be in jokes list
                else if (uk.Contains(value.ToLower()))
                    return "<prosody pitch=\"+0%\">What does the Loch Ness monster eat?<break time=\"900ms\"/> Fish and ships.</prosody>"; //maybe it should be in jokes list
                else if (sc.Contains(value.ToLower()))
                    return "<prosody pitch=\"+0%\">Oh. A Starcraft fan. <break time=\"500ms\"/>Shields up, weapons online!</prosody>"; //maybe it should be in jokes list
                else if (st.Contains(value.ToLower()))
                    return "<prosody pitch=\"+0%\">Oh. A Star Trek fan. <break time=\"500ms\"/>Resistance is futile</prosody>"; //maybe it should be in jokes list

                return "";

            }
        }

        public string ProcessMarried(string value, out bool isValid, out string faceExpr)
        {
            isValid = true;
            faceExpr = "";

            if (IsNo(value))
            {
                faceExpr = FaceExp.FlirtingAfterTalking;
                //faceExpr = FaceExp.BlinkRightEyeAndSmile;
                return "<prosody pitch=\"+0%\">Good. <break time=\"700ms\"/> We will fix that.</prosody>";
            }
            else return "";
        }

        public string ExplainChangeClothes()
        {
            string text = "You can ask me to change clothes";
            if (FlagsShared.IsMobile)
                text += " by using the UI of this app.";
            else text += " by pressing 1 2 3 4 5 or 6 … on the keyboard.";
            return text;
        }

        public string MovieSuggestions()
        {
            string text = KorraModelHelper.GetChance(new string[] { "I will have some suggestions for you later today.", "Later I can recommend you a few good ones." });

            return text;
        }

        public string SurpriseVideoGames(int surprise, bool LikesGamesAskedFirst)
        {
            if (surprise == 1)
            {
                return "I had the impression, you will like video games.";
            }
            else if (surprise == 2)
            {
                //2
                //LVG No
                //Present Yes
                //inference on: LVG
                if (LikesGamesAskedFirst)
                    return "But you still do not like computer games.";
                //Present Yes
                //LVG No
                else return "Hmm, I thought you would like video games if you approve a video game as a present.";
            }
            else if (surprise == 3)
            {
                //3
                //LVG Yes
                //Present No
                //inference on: present

                if (LikesGamesAskedFirst)
                    return "Strange. I though you will like a video game as a present.";

                //Present No
                //LVG Yes
                //inference on: present
                else return "Hmm, But you still think a video game is not a good present.";
            }
            else
            {
                SharedHelper.LogError("Could not generate appropriate video game text to express surprise.");
                return "";
            }
        }

        public string ProcessHasKids(string value, out bool isValid, out string faceExpr)
        {
            isValid = true;
            faceExpr = "";

            if (IsYes(value))
                return "<prosody pitch=\"+0%\">Only a person who has kids knows how hard it is,<break time=\"800ms\"/> but it is also a lot of fun.</prosody>";
            else return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">is the answer to question: UserRelationshipFirstStep</param>
        /// <returns></returns>
        public string ProcessRelationshipFirstStep(string value, out bool isValid, out string faceExpr)
        {
            isValid = true;
            faceExpr = "";
            bool IsWoman = IsAnsweredAndUserIsWoman();
            bool IsMan = IsAnsweredAndUserIsMan();

            if (IsNo(value) && IsWoman)
                return "Right, it is their job afterall.";

            if (IsYes(value) && IsWoman)
                return "Men are dummies, so sometimes we need to take matters in our hands.";

            if (IsYes(value) && IsMan)
                return "Yes, but women are used the other way around.";

            if (IsMan == false && IsWoman == false)
                return "Usually that is a man's job.";

            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">is the answer to question: UserValentinesDay</param>
        /// <returns></returns>
        public string ProcessValentinesDay(string value, out bool isValid, out string faceExpr)
        {
            isValid = true;
            faceExpr = "";

            bool IsWoman = IsAnsweredAndUserIsWoman();
            bool IsMan = IsAnsweredAndUserIsMan();

            if (IsNo(value) && IsWoman)
                return "Right, it is our day.";

            if (IsYes(value) && IsWoman)
                return "Yeah, men also like gifts.";

            if (IsYes(value) && IsMan)
                return "I agree. We are talking about equality here.";

            if (IsMan == false && IsWoman == false)
                return "As a sign of love, both should get a present.";

            return "";
        }

        public string GreetAfterIntroduction(string value, out bool isValid, out string faceExpr)
        {
            isValid = !string.IsNullOrEmpty(value);
            faceExpr = "";

            string text = "Nice to meet you";

            if (isValid)
            {
                string Username = value;

                if (!string.IsNullOrEmpty(Username)) text += " " + Username;
                text += ".";
            }
            else
            {
                text = "Hmmm I think I did not get your name correctly.";
            }

            return text;
        }
    }
}
