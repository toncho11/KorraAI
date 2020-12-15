using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion.KorraAI
{
    public static class JokesProvider 
    {
        static List<Joke> jokes = new List<Joke>();
        static System.Random r = new System.Random();

        //private static int CountJokesPlanned;

        public static void AddJoke(Joke joke)
        {
            jokes.Add(joke);
        }

        public static void AddJokeRange(IEnumerable<Joke> newjokes)
        {
            jokes.AddRange(newjokes);
        }

        public static Joke GetJoke()
        {
            var allJokes = (from joke in GetAll()
                            where joke.IsUsed == false && joke.IsPlanned == false
                            select joke).ToArray();

            if (allJokes.Count() > 0)
                return allJokes[r.Next(allJokes.Count())];
            else return null;
        }


        public static Joke GetJokeByName(string Name) //by ID
        {
            var q = jokes.Where(j => j.Name == Name).SingleOrDefault();

            return q;
        }

        public static void RemovePlannedFlagForAllJokes()
        {
            //CountJokesPlanned = 0;

            for (int i = 0; i < jokes.Count; i++)
            {
                jokes[i].IsPlanned = false;
            }
        }

        public static void SetJokeAsUsed(string Name)
        {
            for (int i = 0; i < jokes.Count; i++)
            {
                if (jokes[i].Name == Name)
                {
                    jokes[i].IsUsed = true;

                    SharedHelper.Log("Joke '" + Name + "' is set to used. Joke text: '" + jokes[i].Text + "'");

                    FlagsShared.RequestSavePersistentData = true;

                    return;
                }
            }

            SharedHelper.LogError("Joke '" + Name + "' not found in SetJokeAsUsed!");
        }

        public static void SetJokeAsPlanned(string Name)
        {
            for (int i = 0; i < jokes.Count; i++)
            {
                if (jokes[i].Name == Name)
                {
                    jokes[i].IsPlanned = true;

                    //SharedHelper.Log("Joke '" + Name + "' is set to planned. Joke text: '" + jokes[i].Text + "'");

                    return;
                }
            }

            SharedHelper.LogError("Joke '" + Name + "' not found in SetJokeAsUsed!");
        }

        public static string[] GetJokesIDsAlreadyUsed()
        {
            return (from joke in jokes
                    where joke.IsUsed == true
                    select joke.Name).ToArray();
        }

        public static bool IDsAreDistinct()
        {
            var list = jokes.Select(s => s.Name).Distinct().ToArray();
            return jokes.Count == list.Length;
        }

        public static Joke[] GetAll()
        {
            return jokes.ToArray();
        }


    }

    public class SongsProvider : ItemManager
    {
        public SongsProvider(Song song) : base(song)
        {
        }
    }

    public class SportsProvider : ItemManager
    {
        public SportsProvider(Item sport) : base(sport)
        {
        }
    }

    public class MoviesProvider : ItemManager
    {
        public MoviesProvider(Item movie) : base(movie)
        {
        }

        public string[] GetMoviesIDsAlreadyUsed()
        {
            return (from movie in items
                    where movie.IsUsed == true
                    select movie.Name).ToArray();
        }

        public override bool IsAllowedResetUsageOnEmptyCategory()
        {
            return true;
        }
    }

    public class ConvinceBuysManager : ItemManager
    {
        public ConvinceBuysManager(Item buy) : base(buy)
        {
        }

        public override bool IsAllowedResetUsageOnEmptyCategory()
        {
            return true;
        }
        //private static List<Buy> allbuys = new List<Buy>();
        //private static readonly System.Random r = new System.Random();

        //public static void AddBuy(Buy buy)
        //{
        //    allbuys.Add(buy);
        //}

        //public static Buy GetConvinceBuy()
        //{
        //    Buy[] q;

        //    q = allbuys.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

        //    if (q.Length > 0) //at least one found
        //    {
        //        Buy s = q.ElementAt(r.Next(0, q.Count()));

        //        return s;
        //    }
        //    else //0 found
        //    {
        //        //remove all used , because there are no more buy statements to use, so we start again
        //        for (int i = 0; i < allbuys.Count; i++)
        //        {
        //            allbuys[i].IsUsed = false;
        //        }

        //        q = allbuys.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

        //        Buy s = q.ElementAt(r.Next(0, q.Count()));

        //        return s;
        //    }
        //}

        //public static void RemovePlannedFlagForAllBuys()
        //{
        //    for (int i = 0; i < allbuys.Count; i++)
        //    {
        //        allbuys[i].IsPlanned = false;
        //    }
        //}

        //public static void SetBuyAsUsed(string Name)
        //{
        //    for (int i = 0; i < allbuys.Count; i++)
        //    {
        //        if (allbuys[i].Name == Name)
        //        {
        //            allbuys[i].IsUsed = true;

        //            SharedHelper.LogWarning("Buy '" + Name + "' is set to used. Buy text: '" + allbuys[i].Text + "'");

        //            return;
        //        }
        //    }

        //    SharedHelper.LogError("Buy '" + Name + "' not found in SetBuyAsUsed!");
        //}

        //public static Buy GetBuyByName(string Name) //by ID
        //{
        //    var q = allbuys.Where(j => j.Name == Name).SingleOrDefault();

        //    return q;
        //}

        //public static Buy[] GetAll()
        //{
        //    return allbuys.ToArray();
        //}
    }
}
