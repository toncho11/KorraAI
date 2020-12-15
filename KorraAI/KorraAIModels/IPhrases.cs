using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    public interface IBasePhrases
    {
        bool IsYes(string input);

        bool IsNo(string input);

        string Yes();

        string No();

        string SayGoodBye();

        string ExitQuestion();

        string ChangeClothesAnnouncement();

        string VideoPlaybackError();

        string ExpressBotGoodMood();
    }
}
