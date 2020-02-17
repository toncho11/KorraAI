using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    public interface IPhrases
    {
        string SayHello();

        string SayGoodBye();

        string GreetAfterIntroduction(string value, out bool isValid, out string faceExpr);

        string BotName();

        string IamAWoman();

        string ChangeClothesAnnouncement();

        string GoOutAnnoucement();

        string MovieAnnouncement(Movie movie);

        string ExitQuestion();

        string VideoPlaybackError();

        bool IsYes(string input);

        bool IsNo(string input);

        string Yes();

        string No();

        string ProcessAge(string value, out bool isValid, out string faceExpr);

        bool IsAnsweredAndUserIsMan();

        bool IsAnsweredAndUserIsWoman();

        string Male();

        string Female();

        string ExpressBotGoodMood(); //express level of good mood, so it could be a bad mood also

        string ExplainChangeClothes();

        string ProcessNationality(string value, out bool isValid, out string faceExpr);

        string ProcessMarried(string value, out bool isValid, out string faceExpr);

        string MovieSuggestions();

        string SurpriseVideoGames(int surprise, bool LikesGamesAskedFirst);

        string ProcessValentinesDay(string value, out bool isValid, out string faceExpr);

        string ProcessRelationshipFirstStep(string value, out bool isValid, out string faceExpr);

        string ProcessHasKids(string value, out bool isValid, out string faceExpr);
    }
}
