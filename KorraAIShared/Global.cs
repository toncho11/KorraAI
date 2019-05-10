using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion
{
    public struct ActionsEnum
    {
        public static string MakeSuggestion { get { return "MakeSuggestion"; } }

        public static string AskUncertanFactQuestion { get { return "AskUncertanFactQuestion"; } }

        public static string AskPureFactQuestionAboutUser { get { return "AskPureFactQuestionAboutUser"; } }

        public static string SharePureFactInfoAboutBot { get { return "SharePureFactInfoAboutBot"; } }

        public static string ChangeVisualAppearance { get { return "ChangeVisualAppearance"; } }

        public static string MakeGeneralStatement { get { return "MakeGeneralStatement"; } } //currently not sampled, only manually created

        public static string MakeBuyStatement { get { return "MakeBuyStatement"; } } //currently not sampled, only manually created

        public static string ExpressMentalState { get { return "ExpressingMentalState"; } }
    }

    public struct SuggestionsEnum
    {
        public static string GoOut { get { return "GoOut"; } }

        public static string TellJoke { get { return "TellJoke"; } }

        public static string WatchMovie { get { return "SuggestToWatchMovie"; } }

        public static string TellWeatherForecast { get { return "TellWeatherForecast"; } }

        public static string ListenToSong { get { return "SuggestListenToSong"; } }

        public static string GoToGym { get { return "SuggestGoToGym"; } }
    }

    public enum PureFactType { AboutBot, AboutUser, UIQuestion, JokeQuestion, BuyQuestion };

    public enum Lang { JA, EN, AUTOMATIC };

    /// <summary>
    /// Probabilistic variable 
    /// </summary>
    public enum PV { Default=0, Current=1, Descreased=2, Increased=3};
}
