using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public class Joke
    {
        public Joke(string text, bool isDelayed) //TODO: add 'doesNeedAnnouncement'
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
            IsDelayedJoke = isDelayed;
            IsPureFact = false;
        }

        public Joke(string text)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;

            IsDelayedJoke = false;
            IsPureFact = false;
        }

        public Joke(string name, string text)
        {
            Name = name;
            Text = text;
            IsUsed = false;

            IsDelayedJoke = false;
            IsPureFact = false;
        }

        public Joke(string name, string text, bool isDelayed)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            IsDelayedJoke = isDelayed;
            IsPureFact = false;
        }

        public Joke(string name, string text, bool isDelayed, bool isPureFact, UIAnswer pureFactUI)
        {
            Name = name;
            Text = text;
            IsUsed = false;
            IsDelayedJoke = isDelayed;
            IsPureFact = isPureFact;
            PureFactUI = pureFactUI;
        }

        public string Name;
        public string Text;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled

        //For men
        //For women
        //Only for parents

        //Some jokes are not appropriate in the beginning
        public bool IsDelayedJoke;

        //public bool DoesNeedAnnouncement;
        #region Pure Fact Joke
        public bool IsPureFact;

        public UIAnswer PureFactUI;
        #endregion
    }

    public class Song
    {
        public Song(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name;
        public string Url;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled

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

    public class Sport
    {
        public Sport(string text)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
        }

        public string Name;
        public string Text;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled
    }

    public class Movie
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

        public string Name;
        public string Text;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled
        public bool IsTvSeries;
        public bool IsReleased;
    }

    public class Buy
    {
        public Buy(string text)
        {
            Name = Guid.NewGuid().ToString();
            Text = text;
            IsUsed = false;
        }

        public string Name;
        public string Text;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled
    }
}
