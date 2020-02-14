using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public class Joke : Item
    {
        public Joke(string text, bool isDelayed) //TODO: add 'doesNeedAnnouncement'
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
            //IsDelayedJoke = isDelayed;
            IsPureFact = false;
        }

        public Joke(string text)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;

            //IsDelayedJoke = false;
            IsPureFact = false;
        }

        public Joke(string name, string text)
        {
            Name = name;
            Text = text;
            IsUsed = false;

            //IsDelayedJoke = false;
            IsPureFact = false;
        }

        public Joke(string name, string text, bool isDelayed)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            //IsDelayedJoke = isDelayed;
            IsPureFact = false;
        }

        public Joke(string name, string text, bool isDelayed, string faceExp)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            //IsDelayedJoke = isDelayed;
            IsPureFact = false;
            FaceExpression = faceExp;
        }

        public Joke(string name, string text, ContentType type, string faceExp)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            ContentType = type;
            IsPureFact = false;
            FaceExpression = faceExp;
        }

        public Joke(string name, string text, bool isDelayed, bool isPureFact, UIAnswer pureFactUI)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            //IsDelayedJoke = isDelayed;
            IsPureFact = isPureFact;
            PureFactUI = pureFactUI;
        }

        public Joke(string name, string text, ContentType type)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            ContentType = type;
            IsPureFact = false;
        }

        //public string Name { get; set; }
        public string Text;

        //For men
        //For women
        //Only for parents

        //Some jokes are not appropriate in the beginning
        //public bool IsDelayedJoke;

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

    public class Sport : Item
    {
        public Sport(string text)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
        }

        //public string Name { get; set; }
        public string Text;
    }

    public class Movie : Item
    {
        public Movie(string text, bool isTVSeries, bool isReleased)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
            IsTvSeries = isTVSeries;
            IsReleased = isReleased;
        }

        public Movie(string name, string text, bool isTVSeries, bool isReleased)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            IsTvSeries = isTVSeries;
            IsReleased = isReleased;
        }

        //public string Name { get; set; }
        public string Text;

        public bool IsTvSeries;
        public bool IsReleased;
    }

    public class Buy : Item
    {
        public Buy(string text)
        {
            Name = Guid.NewGuid().ToString();
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
        public string Text;

        //public string FaceExpression;
    }
}
