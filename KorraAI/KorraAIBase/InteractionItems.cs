using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    /// <summary>
    /// A simple text to uttered by the bot
    /// </summary>
    public class Statement : Item
    {
        public string Text;

        public Statement()
        {
            Name = Guid.NewGuid().ToString();
            IsUsed = false;
            Category = ActionsEnum.MakeGeneralStatement;
        }

        public Statement(string text)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
            Category = ActionsEnum.MakeGeneralStatement;
        }
    }

    public class Joke : Statement
    {
        void Init()
        {
            Name = Guid.NewGuid().ToString();
            Category = ActionsEnum.MakeSuggestion;
            SubCategory = SuggestionsEnum.TellJoke;
        }

        public Joke(string text, bool isDelayed)
        {
            Init();
            Text = text;
            IsUsed = false;
            IsPureFact = false;
            
        }

        public Joke(string text)
        {
            Init();
            Text = text;
            IsUsed = false;
            IsPureFact = false;
            
        }

        public Joke(string name, string text)
        {
            Init();
            Name = name;
            Text = text;
            IsUsed = false;
            IsPureFact = false;
        }

        public Joke(string name, string text, bool isDelayed)
        {
            Init();
            Name = name;
            Text = text;
            IsUsed = false;
            IsPureFact = false;
        }

        public Joke(string name, string text, bool isDelayed, string faceExp)
        {
            Init();

            Name = name;
            Text = text;
            IsUsed = false;
            IsPureFact = false;
            FaceExpression = faceExp;
        }

        public Joke(string name, string text, ContentType type, string faceExp)
        {
            Init();

            Name = name;
            Text = text;
            IsUsed = false;
            ContentType = type;
            IsPureFact = false;
            FaceExpression = faceExp;;
        }

        public Joke(string name, string text, bool isDelayed, bool isPureFact, UIAnswer pureFactUI)
        {
            Init();

            Name = name;
            Text = text;
            IsUsed = false;
            IsPureFact = isPureFact;
            PureFactUI = pureFactUI;
        }

        public Joke(string name, string text, ContentType type)
        {
            Init();

            Name = name;
            Text = text;
            IsUsed = false;
            ContentType = type;
            IsPureFact = false;
        }

        //For men
        //For women
        //Only for parents
        //public bool DoesNeedAnnouncement;

        #region Pure Fact Joke
        public bool IsPureFact;

        public UIAnswer PureFactUI;
        #endregion

        public string FaceExpression;
    }

    public class Song : Item
    {
        public Song(string name, string url)
        {
            Name = name;
            Url = url;

            Category = ActionsEnum.MakeSuggestion;
            SubCategory = SuggestionsEnum.ListenToSong;
        }

        //public string Name { get; set; }
        public string Url;

        public string GetID
        {
            get
            {
                int p = Url.IndexOf("=");
                if (p > 0)
                {
                    string id = Url.Substring(p + 1);
                    return id;
                }
                else return "";
            }
        }
    }

    public class Sport : Statement
    {
        void Init()
        {
            Name = Guid.NewGuid().ToString();
            Category = ActionsEnum.MakeSuggestion;
            SubCategory = SuggestionsEnum.DoSport;
        }

        public Sport(string text)
        {
            Init();

            Text = text;
            IsUsed = false;
        }

        //public string Name { get; set; }
        //public string Text;
    }

    public class Movie : Statement
    {
        void Init()
        {
            Name = Guid.NewGuid().ToString();
            Category = ActionsEnum.MakeSuggestion;
            SubCategory = SuggestionsEnum.WatchMovie;
        }

        public Movie(string text, bool isTVSeries, bool isReleased)
        {
            Init();

            Text = text;
            IsUsed = false;
            IsTvSeries = isTVSeries;
            IsReleased = isReleased;
        }

        public Movie(string name, string text, bool isTVSeries, bool isReleased)
        {
            Init();

            Name = name;
            Text = text;
            IsUsed = false;
            IsTvSeries = isTVSeries;
            IsReleased = isReleased;
        }

        //public string Name { get; set; }
        //public string Text;

        public bool IsTvSeries;
        public bool IsReleased;
    }

    public class Buy : Statement
    {
        void Init()
        {
            Name = Guid.NewGuid().ToString();
            Category = ActionsEnum.ConvinceBuyStatement;
        }

        public Buy(string text)
        {
            Init();

            Text = text;
            IsUsed = false;
        }

        //public Buy(string text, string faceExp)
        //{
        //    Name = Guid.NewGuid().ToString();
        //    Text = text;
        //    IsUsed = false;
        //    FaceExpression = faceExp;
        //}

        //public string Name { get; set; }
        //public string Text;

        //public string FaceExpression;
    }
}
