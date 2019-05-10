using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public static class JokesProvider
    {
        static List<Joke> jokes = new List<Joke>();
        static System.Random r = new System.Random();

        private static int CountJokesPlanned;

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
            Joke[] q = null;

            //warning: there should be enough non-delayed jokes, otherwise all jokes will be blocked
            
            //if possible use the non-delayed jokes
            if ((StatesShared.ActualJokesProvided + CountJokesPlanned) <= BotConfigShared.MinimumJokesBeforeAllowingDelayedJokes) //less than 9 jokes, do not use delayed jokes
            {
                q = jokes.Where(j => j.IsUsed == false && j.IsPlanned == false && j.IsDelayedJoke == false).ToArray(); //do not use delayed jojes in the beginning
            }

            if (q == null || (q!=null && q.Length==0))
            {
                q = jokes.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray(); //use all jokes delayed / not delayed
            }

            if (q.Length > 0)
            {
                Joke joke = q.ElementAt(r.Next(0, q.Count()));

                CountJokesPlanned = CountJokesPlanned + 1;

                return joke;
            }
            else return null;
        }

        public static Joke GetJokeByName(string Name) //by ID
        {
            var q = jokes.Where(j => j.Name == Name).SingleOrDefault();

            return q;
        }

        public static void RemovePlannedFlagForAllJokes()
        {
            CountJokesPlanned = 0;

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

            SharedHelper.LogError("Joke '"+Name+"' not found in SetJokeAsUsed!");
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
    }

    public static class SongsProvider
    {
        static List<Song> songs = new List<Song>();
        static System.Random r = new System.Random();

        public static void AddSong(Song song)
        { 
            songs.Add(song);
        }

        public static Song GetSong()
        {
            var q = songs.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

            if (q.Length > 0)
            {
                Song song = q.ElementAt(r.Next(0, q.Count()));

                return song;
            }
            else return null;
        }

        public static Song GetSongByName(string Name) //by ID
        {
            var q = songs.Where(j => j.Name == Name).ToArray();

            if (q.Length == 0)
            {
                SharedHelper.LogError("Song " + Name + " not found in GetSongByName!");
                return (songs.Count >0) ? songs[0] : null; //return first available song
            } 
            else if (q.Length > 1)
            {
                SharedHelper.LogError("Song " + Name + " has a duplicate in GetSongByName!");
            }

            return q[0]; // > 1
        }

        public static void RemovePlannedFlagForAllSongs()
        {
            for (int i = 0; i < songs.Count; i++)
            {
                songs[i].IsPlanned = false;
            }
        }

        public static void SetSongAsUsed(string Name)
        {
            for (int i = 0; i < songs.Count; i++)
            {
                if (songs[i].Name == Name)
                {
                    songs[i].IsUsed = true;

                    SharedHelper.Log("Song '" + Name + "' is set to used.");

                    return;
                }
            }

            SharedHelper.LogError("Song '"+ Name +"' not found in SetSongAsUsed!");
        }
    }

    public static class SportsProvider
    {
        static List<Sport> sports = new List<Sport>();
        static System.Random r = new System.Random();

        static SportsProvider()
        {
           
        }

        public static void AddSport(Sport sport)
        {
            sports.Add(sport);
        }

        public static Sport GetSport()
        {
            Sport[] q;

            q = sports.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

            if (q.Length > 0)
            {
                Sport s = q.ElementAt(r.Next(0, q.Count()));

                return s;
            }
            else return null;
        }

        public static Sport GetSportByName(string Name) //by ID
        {
            var q = sports.Where(j => j.Name == Name).SingleOrDefault();

            return q;
        }

        public static void RemovePlannedFlagForAllSports()
        {
            for (int i = 0; i < sports.Count; i++)
            {
                sports[i].IsPlanned = false;
            }
        }

        public static void SetSportAsUsed(string Name)
        {
            for (int i = 0; i < sports.Count; i++)
            {
                if (sports[i].Name == Name)
                {
                    sports[i].IsUsed = true;

                    SharedHelper.LogWarning("Sport " + Name + " is set to used. Sport text: '" + sports[i].Text + "'");

                    return;
                }
            }

            SharedHelper.LogError("Sport "+Name+" not found in SetMovieAsUsed!");
        }
    }


    public static class MoviesProvider
    {
        private static List<Movie> movies = new List<Movie>();
        private static readonly System.Random r = new System.Random();

        public static void AddMovie(Movie movie)
        {
            movies.Add(movie);
        }

        public static Movie GetMovie()
        {
            Movie[] q;

            q = movies.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

            if (q.Length > 0)
            {
                Movie s = q.ElementAt(r.Next(0, q.Count()));

                return s;
            }
            else return null;
        }

        public static Movie GetMovieByName(string Name) //by ID
        {
            var q = movies.Where(j => j.Name == Name).SingleOrDefault();

            return q;
        }

        public static void RemovePlannedFlagForAllMovies()
        {
            for (int i = 0; i < movies.Count; i++)
            {
                movies[i].IsPlanned = false;
            }
        }

        public static void SetMovieAsUsed(string Name)
        {
            for (int i = 0; i < movies.Count; i++)
            {
                if (movies[i].Name == Name)
                {
                    movies[i].IsUsed = true;

                    SharedHelper.Log("Movie '" + Name + "' is set to used. Movie text: '" + movies[i].Text + "'");

                    FlagsShared.RequestSavePersistentData = true;

                    return;
                }
            }

            SharedHelper.LogError("Movie '"+ Name +"' not found in SetMovieAsUsed!");
        }

        public static string[] GetMoviesIDsAlreadyUsed()
        {
            return (from movie in movies
                    where movie.IsUsed == true
                    select movie.Name).ToArray();
        }

    }

    public static class BuysProvider
    {
        private static List<Buy> allbuys = new List<Buy>();
        private static readonly System.Random r = new System.Random();

        public static void AddBuy(Buy buy)
        {
            allbuys.Add(buy);
        }

        public static Buy GetBuy()
        {
            Buy[] q;

            q = allbuys.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

            if (q.Length > 0) //at least one found
            {
                Buy s = q.ElementAt(r.Next(0, q.Count()));

                return s;
            }
            else //0 found
            {
                //remove all used , because there are no more buy statements to use, so we start again
                for (int i = 0; i < allbuys.Count; i++)
                {
                    allbuys[i].IsUsed = false;
                }

                q = allbuys.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

                Buy s = q.ElementAt(r.Next(0, q.Count()));

                return s;
            }
        }

        public static void RemovePlannedFlagForAllBuys()
        {
            for (int i = 0; i < allbuys.Count; i++)
            {
                allbuys[i].IsPlanned = false;
            }
        }

        public static void SetBuyAsUsed(string Name)
        {
            for (int i = 0; i < allbuys.Count; i++)
            {
                if (allbuys[i].Name == Name)
                {
                    allbuys[i].IsUsed = true;

                    SharedHelper.LogWarning("Buy '" + Name + "' is set to used. Buy text: '" + allbuys[i].Text + "'");

                    return;
                }
            }

            SharedHelper.LogError("Buy '"+ Name +"' not found in SetBuyAsUsed!");
        }
    }
}
