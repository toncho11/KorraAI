using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models.Joi
{
    public interface IJoiPhrases : IBasePhrases
    {
        string SayHello();

        string GreetAfterIntroduction(string value, out bool isValid, out string faceExpr);

        string BotName();

        string IamAWoman();

        string GoOutAnnoucement();

        string MovieAnnouncement(Movie movie);

        string ProcessAge(string value, out bool isValid, out string faceExpr);

        bool IsAnsweredAndUserIsMan();

        bool IsAnsweredAndUserIsWoman();

        string Male();

        string Female();

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
