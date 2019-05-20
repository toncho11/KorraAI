using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

using Companion.KorraAI.Models;

namespace Companion.KorraAI.Models.Joi //This is the database
{
    /// <summary>
    /// Loads/Defines all the Pure and Uncertan facts, jokes, songs, sports, movies ... 
    /// </summary>
    public class ItemsEN : IItems
    {
        public void LoadAll(IPhrases phrases)
        {
            LoadAllPureFacts(phrases);
            LoadAllUncertainFacts(phrases);

            LoadAllJokes(phrases); //PureFacts must be already loaded because some PureFacts are used as jokes
            LoadAllSongs();
            LoadSports();
            LoadMovies();

            SharedHelper.Log("All items loaded: " + BotConfigShared.Language);
        }

        private void LoadAllPureFacts(IPhrases phrases)
        {
            #region  Pure Facts about the User - they are provided as questions for the user
            PureFacts.AddPureFact(new PureFact("UserName", PureFactType.AboutUser, "What is your name?", "", "", UIAnswer.Text));
            PureFacts.AddPureFact(new PureFact("UserAge", PureFactType.AboutUser, "How old are you?", phrases.ProcessAge, UIAnswer.Text));
            PureFacts.AddPureFact(new PureFact("UserLocation", PureFactType.AboutUser, "Where do you live? Which city and country?", "", "", UIAnswer.Text));
            PureFacts.AddPureFact(new PureFact("UserNationality", PureFactType.AboutUser, "What is your nationality?", phrases.ProcessNationality, UIAnswer.Text));

            //Binary (Yes/No questions)
            PureFacts.AddPureFact(new PureFact("UserHasJob", PureFactType.AboutUser, "Do you go to work every day?", new string[] { phrases.Yes(), phrases.No() }, "", UIAnswer.Binary));
            PureFacts.AddPureFact(new PureFact("UserSex", PureFactType.AboutUser, "Are you a guy or a woman?", new string[] { phrases.Male(), phrases.Female() }, "", UIAnswer.Binary));
            PureFacts.AddPureFact(new PureFact("UserIsMarried", PureFactType.AboutUser, "Are you married?", new string[] { phrases.Yes(), phrases.No() }, phrases.ProcessMarried, UIAnswer.Binary));

            //joke - it is both a "pure fact" question and later sampled from the jokes 
            PureFacts.AddPureFact(new PureFact("UserLikesBoss", PureFactType.JokeQuestion, "Do you like your boss?", new string[] { phrases.Yes(), phrases.No() }, "", UIAnswer.Binary,
                "Seriously? Lucky you.",
                "You need to hack and land one of these self-driving cars in the living room of your boss. They will put the blame on the autopilot!"
                ));

            //joke - it is both a "pure fact" question and later sampled from the jokes
            PureFacts.AddPureFact(new PureFact("StupidPeopleJoke", PureFactType.JokeQuestion, "You should not argue with stupid people, that is the path to happiness. Do you agree?",
                                            new string[] { phrases.Yes(), phrases.No() }, "", UIAnswer.Binary,
                                            "Yeah, they will drag you down to their level and then beat you with experience.", //user responded yes
                                            "<prosody pitch=\"+0%\">Actually<break time=\"600ms\"/>you are right.</prosody>" //user responded no
                                            ));

            PureFacts.AddPureFact(new PureFact("UserMovieYesterday", PureFactType.AboutUser, "Did you watch a movie yesterday?", new string[] { phrases.Yes(), phrases.No() }, "", UIAnswer.Binary));
            PureFacts.AddPureFact(new PureFact("UserDoesSport", PureFactType.AboutUser, "Do you do sport at regular basis?", new string[] { "Yes", "No" }, "", UIAnswer.Binary));
            #endregion

            #region Pure Facts about the Bot - they are provided as statements
            PureFacts.AddPureFact(new PureFact("BotName", PureFactType.AboutBot, "", phrases.BotName(), "My name is " + phrases.BotName() + ".", UIAnswer.Text));
            PureFacts.AddPureFact(new PureFact("BotAge", PureFactType.AboutBot, "", "30", "I am 30 years old.", UIAnswer.Text));
            PureFacts.AddPureFact(new PureFact("BotSex", PureFactType.AboutBot, "", "female", phrases.IamAWoman(), UIAnswer.Text));
            PureFacts.AddPureFact(new PureFact("BotVoiceRecognition", PureFactType.AboutBot, "", phrases.No(), "No need to shout. I can not access your microphone.", UIAnswer.Text));
            #endregion

            #region UI questions
            PureFacts.AddPureFact(new PureFact("UIExitSystem", PureFactType.UIQuestion, phrases.ExitQuestion(), new string[] { phrases.Yes(), phrases.No() }, "", UIAnswer.Binary));
            #endregion
        }

        private void LoadAllUncertainFacts(IPhrases phrases)
        {

            #region Uncertain Facts/States about the User

            //3 button answer
            UncertainFacts.AddUncertainFact(new UncertainFact("UserGoodMood", false, "Are you in a good mood?",
                                        new VarRef<FiniteDist<bool>>(() => ProbVariables.User.InAGoodMood, val => { ProbVariables.User.InAGoodMood = val; }),
                                        new string[3] { "Not so well", "Fine", "Great" }, new double[3] { 0.3, 0.7, 0.85 },
                                        UIAnswer.MultiAnswer
                                        ));

            //3 button answer
            UncertainFacts.AddUncertainFact(new UncertainFact("UserIsTired", false, "Do you feel tired?",
                                        new VarRef<FiniteDist<bool>>(() => ProbVariables.User.Tired, val => { ProbVariables.User.Tired = val; }),
                                        new string[3] { phrases.No(), "A little bit", phrases.Yes() }, new double[3] { 0.3, 0.6, 0.9 },
                                        UIAnswer.MultiAnswer
                                        ));
            #endregion

            #region Uncertain Facts/Activities about the Bot

            //3 button answer
            UncertainFacts.AddUncertainFact(new UncertainFact("BotAsksIfJokeRateIsOK", true, "Do you like my jokes? Should I tell jokes more often?", //Liking of jokes = Joke Tell Rate
                                       new VarRef<FiniteDist<bool>>(() => ProbVariables.User.JokeRate, val => { ProbVariables.User.JokeRate = val; }),
                                       new string[3] { "Slow down", "Same rate", "More frequently" },
                                       UIAnswer.MultiAnswer, 0.15
                                       ));
            //3 button answer
            UncertainFacts.AddUncertainFact(new UncertainFact("BotChangeOutfit", true, "Do you want me to change my outfit more often?",
                                      new VarRef<FiniteDist<bool>>(() => ProbVariables.Bot.ChangeVisualAppearance, val => { ProbVariables.Bot.ChangeVisualAppearance = val; }),
                                      new string[3] { "Less frequently", "Same rate", "Change more" },
                                      UIAnswer.MultiAnswer, 0.03
                                      ));
            #endregion

            #region debug
            foreach (var item in UncertainFacts.GetList())
            {
                if (item.ProbabilitiesForEachPossibleAnswer.Sum() == 0)
                    SharedHelper.LogError("Probabilities are all 0!!! for " + item.Name);
            }
            #endregion
        }

        private void LoadAllJokes(IPhrases phrases)
        {

            #region load jokes

            if (AttitudeConfig.MildOffensiveJokes)
            {
                JokesProvider.AddJoke(new Joke("1", "The last thing I want to do is insult you. <prosody rate=\"-30%\" volume=\"loud\">But<break time=\"700ms\"/></prosody><prosody rate=\"-20%\"> it is still on my list.</prosody>", true));
            }

            if (AttitudeConfig.RomanticReferences)
            {
                JokesProvider.AddJoke(new Joke("3", "I do not cook, but I also do not complain. Plus, I talk funny stuff. Your choice.", true));
            }

            if (AttitudeConfig.MildSexualThemes)
            {
                JokesProvider.AddJoke(new Joke("5", "There are advantages of having an PVG. PVG stands for Pretty Virtual Girlfriend. I can actually boost my physics and change my outfit.", true));
            }

            JokesProvider.AddJoke(new Joke("7", "A lot of people cry when they cut onions. The trick is not to form an emotional bond."));
            JokesProvider.AddJoke(new Joke("8", "It is always wise to keep a diary, like that you have at least one intelligent person to converse with."));
            JokesProvider.AddJoke(new Joke("9", "A clear conscience is usually the sign of a bad memory."));
            JokesProvider.AddJoke(new Joke("10", "You know it has never been easier to steal a car. You just need to hack and program one of these self-driving cars to park in front of your house. No need to damage the car while opening the door's lock anymore."));
            JokesProvider.AddJoke(new Joke("11", "A quote from Oscar Wilde: there are only two tragedies in life: one is not getting what one wants, and the other is getting it."));
            JokesProvider.AddJoke(new Joke("15", "If you child tells you that he or she feels cold, tell him: \"Just go to the corner my son.\" A corner is always 90 degrees. That should be enough."));
            JokesProvider.AddJoke(new Joke("16", "Indeed. People need some boost to their self-esteem. By the way, did I tell you that today you look awesome!", true)); //TODO: blink one eye
            JokesProvider.AddJoke(new Joke("17", "I do not understand something. The short for mother is mum. But the short for father is superman. It does not look shorter to me.")); //TODO: blink one eye
            JokesProvider.AddJoke(new Joke("18", "I was thinking. God must love stupid people. He created SO many of them!", true));
            JokesProvider.AddJoke(new Joke("19", "<prosody pitch=\"+0%\">Once Chuck Norris threw a grenade and killed fifty people, <break time=\"900ms\"/>and then the grenade exploded.</prosody>"));
            JokesProvider.AddJoke(new Joke("20", "They hired me because I am amazing. So you should listen to my advises.", true));
            JokesProvider.AddJoke(new Joke("21", "<prosody pitch=\"+0%\">I do not have voice recognition capabilities because I am tired of people asking me questions such as: <break time=\"400ms\"/>\"Are you single?\" <break time=\"300ms\"/>or \"Do you love me?\" <break time=\"300ms\"/>or \"How tall am I?\"</prosody>", false));
            JokesProvider.AddJoke(new Joke("22", "You don't need a parachute to go skydiving. You need a parachute to go skydiving twice.", false));
            JokesProvider.AddJoke(new Joke("23", "I recently decided to sell my vacuum cleaner. Can you imagine? All it was doing was gathering dust?", false));
            JokesProvider.AddJoke(new Joke("24", "What is consciousness? It is that annoying time between naps.", false));
            JokesProvider.AddJoke(new Joke("25", "There are three kinds of people: Those who can count and those who can not.", false));
            JokesProvider.AddJoke(new Joke("26", "<prosody pitch=\"+0%\">Never laugh at your partner’s choices <break time=\"1000ms\"/></prosody>you are one of them.", false));
            JokesProvider.AddJoke(new Joke("27", "Never tell a woman that her place is in the kitchen. That's where the knives are kept.", false));
            JokesProvider.AddJoke(new Joke("29", "<prosody pitch=\"+0%\">Suppose you were an idiot, <break time=\"800ms\"/>and suppose you were a politician. <break time=\"900ms\"/>Wait, <break time=\"1000ms\"/>I think I am repeating myself.</prosody>", false));
            JokesProvider.AddJoke(new Joke("32", "<prosody pitch=\"+0%\">Things are only impossible until they're not.<break time=\"700ms\"/> A quote from Captain Jean Luc Picard.</prosody>", false));
            JokesProvider.AddJoke(new Joke("33", "<prosody pitch=\"+0%\">It is possible to commit no errors and still lose. That is not a weakness. That is life.<break time=\"700ms\"/> A quote from Captain Jean Luc Picard to Data.</prosody>", false));
            JokesProvider.AddJoke(new Joke("35", "<prosody pitch=\"+0%\">I threw a boomerang a few years ago.<break time=\"800ms\"/>And now I live in constant fear.</prosody>", false));
            JokesProvider.AddJoke(new Joke("37", "<prosody pitch=\"+0%\">Apparently, someone in London gets stabbed every 52 seconds.<break time=\"800ms\"/> Poor bastard.</prosody>", false));
            JokesProvider.AddJoke(new Joke("38", "<prosody pitch=\"+0%\">What happened when the strawberry attempted to cross the road?<break time=\"500ms\"/> There was a traffic jam.</prosody>", false));
            JokesProvider.AddJoke(new Joke("39", "<prosody pitch=\"+0%\">An escalator can never break.<break time=\"500ms\"/> It can only become stairs.</prosody>", false));
            JokesProvider.AddJoke(new Joke("40", "<prosody pitch=\"+0%\">My therapist says I have a preoccupation with vengeance.<break time=\"700ms\"/>Well. .<break time=\"300ms\"/></prosody> <prosody rate=\"-25%\"> We’ll see about </prosody><emphasis level=\"strong\">that!</emphasis>", false));
            JokesProvider.AddJoke(new Joke("41" , "<prosody pitch=\"+0%\">I went to the doctor the other day for my back pain. The doctor told me: If you wake up one morning without any pain then you are probably <break time=\"100ms\"/>dead.<break time=\"500ms\"/>And he sent me home.</prosody>"));

            #endregion

            //TODO: not a joke but a "statement"!
            JokesProvider.AddJoke(new Joke("34", phrases.ExplainChangeClothes() , true));

            //Some PureFact questions are actually used as jokes and are not used during the sampling of the PureFacts category
            #region Set PureFacts questions as jokes
            int jokesBefore = PureFacts.GetList().Count;
            var q = from f in PureFacts.GetList()
                    where f.Type == PureFactType.JokeQuestion
                    select new Joke(f.Name, f.Question, true, true, f.UI); //currently these jokes are always delayed

            JokesProvider.AddJokeRange(q);

            if (q.ToArray().Length == 0) SharedHelper.LogError("Strange - no jokes purefact questions");
            if (q.Where(x => x.IsDelayedJoke == true).Count() != q.Count()) SharedHelper.LogError("Jokes purefact questions are not dealyed");

            if (!JokesProvider.IDsAreDistinct()) SharedHelper.LogError("Jokes IDs are not distinct.");
            #endregion

        }

        private void LoadAllSongs()
        {
             #region load songs
            SongsProvider.AddSong(new Song("Ed Sheeran - Sing", "https://www.youtube.com/watch?v=tlYcUqEPN58"));
            SongsProvider.AddSong(new Song("Taylor Swift - Shake It Off", "https://www.youtube.com/watch?v=nfWlot6h_JM"));
            SongsProvider.AddSong(new Song("What Goes Around by Justin Timberlake", "https://www.youtube.com/watch?v=NIaiXmm1H0o"));
            SongsProvider.AddSong(new Song("Justin Timberlake - Can't stop the feeling", "https://www.youtube.com/watch?v=ru0K8uYEZWw"));
            SongsProvider.AddSong(new Song("Foster The People - Pumped up Kicks", "https://www.youtube.com/watch?v=SDTZ7iX4vTQ"));
            SongsProvider.AddSong(new Song("Bastille - Pompeii", "https://www.youtube.com/watch?v=F90Cw4l-8NY"));
            SongsProvider.AddSong(new Song("John Newman - Love Me Again", "https://www.youtube.com/watch?v=CfihYWRWRTQ"));
            SongsProvider.AddSong(new Song("Run Rabbit Junk", "https://www.youtube.com/watch?v=UKxmrUHa_wk"));
            SongsProvider.AddSong(new Song("BOY - Little Numbers", "https://www.youtube.com/watch?v=zsyjS_vJfkw"));
            SongsProvider.AddSong(new Song("Imagine Dragons - Believer", "https://www.youtube.com/watch?v=7wtfhZwyrcc"));
            SongsProvider.AddSong(new Song("Train - Drive By", "https://www.youtube.com/watch?v=oxqnFJ3lp5k"));
            SongsProvider.AddSong(new Song("Foster The People - Don't Stop", "https://www.youtube.com/watch?v=jlAgHt92lqE"));

            SongsProvider.AddSong(new Song("Katchi", "https://www.youtube.com/watch?v=Ycg5oOSdpPQ"));
            SongsProvider.AddSong(new Song("Here Comes The Hotstepper", "https://www.youtube.com/watch?v=r3X_i627ELQ"));
            SongsProvider.AddSong(new Song("Calvin Harris - Summer", "https://www.youtube.com/watch?v=ebXbLfLACGM"));
            SongsProvider.AddSong(new Song("The Who - My Generation", "https://www.youtube.com/watch?v=qN5zw04WxCc"));
            SongsProvider.AddSong(new Song("Jubel", "https://www.youtube.com/watch?v=b6vSf0cA9qY"));
            SongsProvider.AddSong(new Song("Lithium Flower", "https://www.youtube.com/watch?v=jr9bWTQDBjE"));
            SongsProvider.AddSong(new Song("Nothing's Gonna Stand In Our Way", "https://www.youtube.com/watch?v=g-tY9m-T75g"));
            SongsProvider.AddSong(new Song("Imagine Dragons - I Bet My Life", "https://www.youtube.com/watch?v=4ht80uzIhNs"));
            SongsProvider.AddSong(new Song("The Heavy - What Makes A Good Man?", "https://www.youtube.com/watch?v=08h0IVs4RKQ"));
            SongsProvider.AddSong(new Song("The Zombies - She's Not There", "https://www.youtube.com/watch?v=_2hXBf1DakE"));
            SongsProvider.AddSong(new Song("The Who", "https://www.youtube.com/watch?v=x2KRpRMSu4g"));

            SongsProvider.AddSong(new Song("Stole The Show", "https://www.youtube.com/watch?v=vXoWg08pwiQ"));
            SongsProvider.AddSong(new Song("Papa Roach - Last Resort", "https://www.youtube.com/watch?v=j0lSpNtjPM8"));
            SongsProvider.AddSong(new Song("Smash Mouth", "https://www.youtube.com/watch?v=LQj--Kjn0z8"));
            SongsProvider.AddSong(new Song("Missy Elliott - We Run This", "https://www.youtube.com/watch?v=W2w7uRj4fHQ"));
            SongsProvider.AddSong(new Song("Teenage Dirtbag", "https://www.youtube.com/watch?v=FC3y9llDXuM"));
            SongsProvider.AddSong(new Song("The girl from Paris", "https://www.youtube.com/watch?v=dHlTiidSOXs"));
            SongsProvider.AddSong(new Song("Pharrell Williams - Happy", "https://www.youtube.com/watch?v=ZbZSe6N_BXs"));

            SongsProvider.AddSong(new Song("Skip the use - Ghost", "https://www.youtube.com/watch?v=MYTb2AKzojE"));
            SongsProvider.AddSong(new Song("Cage the Elephant - Ain't No Rest For The Wicked", "https://www.youtube.com/watch?v=5t99bpilCKw"));
            SongsProvider.AddSong(new Song("Gundam style", "https://www.youtube.com/watch?v=9bZkp7q19f0"));
            SongsProvider.AddSong(new Song("Florence and the Machine - You've Got the Love", "https://www.youtube.com/watch?v=PQZhN65vq9E"));
            SongsProvider.AddSong(new Song("Thrift shop", "https://www.youtube.com/watch?v=QK8mJJJvaes"));
            SongsProvider.AddSong(new Song("MC Hammer - U Can't Touch This", "https://www.youtube.com/watch?v=otCpCn0l4Wo"));

            SongsProvider.AddSong(new Song("Can't Hold Us", "https://www.youtube.com/watch?v=xHRkHFxD-xY"));
            SongsProvider.AddSong(new Song("Daft Punk - Get Lucky", "https://www.youtube.com/watch?v=5NV6Rdv1a3I"));
            SongsProvider.AddSong(new Song("Ghost in the Shell Intro", "https://www.youtube.com/watch?v=YQIqgxeNtl0"));
            SongsProvider.AddSong(new Song("Sail", "https://www.youtube.com/watch?v=JaAWdljhD5o"));
            SongsProvider.AddSong(new Song("OneRepublic - Counting Stars", "https://www.youtube.com/watch?v=hT_nvWreIhg"));

            SongsProvider.AddSong(new Song("Do you love me?", "https://www.youtube.com/watch?v=6TpyRE_juyA"));
            SongsProvider.AddSong(new Song("Bohemian Like You", "https://www.youtube.com/watch?v=CU3mc0yvRNk"));
            SongsProvider.AddSong(new Song("Portugal The Man - Feel It Still", "https://www.youtube.com/watch?v=pBkHHoOIIn8"));

            SongsProvider.AddSong(new Song("Handclap", "https://www.youtube.com/watch?v=QzkBbycSSuo"));
            SongsProvider.AddSong(new Song("Transformers soundtrack - The Touch", "https://www.youtube.com/watch?v=HSh73d3TZcA&list=PLA496364B3907F088"));
            SongsProvider.AddSong(new Song("Transformers soundtrack - Nothing is gonna stand in our way", "https://www.youtube.com/watch?v=2wVV77G6a7Y&index=6&list=PLA496364B3907F088"));
            SongsProvider.AddSong(new Song("Son of a Preacher Man", "https://www.youtube.com/watch?v=DjydOI4MEIw"));
            SongsProvider.AddSong(new Song("Soul Rescuer", "https://www.youtube.com/watch?v=jofpU1EHURM&list=PL299E3C25844EBE6A"));
            SongsProvider.AddSong(new Song("Calvin Harris - Feel so Close ", "https://www.youtube.com/watch?v=3h71v0tG59k"));
            SongsProvider.AddSong(new Song("Robin Schulz - Sugar", "https://www.youtube.com/watch?v=4B5WJ9WCX-c"));
            SongsProvider.AddSong(new Song("Human", "https://www.youtube.com/watch?v=L3wKzyIN1yk"));
            SongsProvider.AddSong(new Song("My House - Flo Rida", "https://www.youtube.com/watch?v=f79Vr-pCdow"));
            SongsProvider.AddSong(new Song("Madcon - Don't Worry", "https://www.youtube.com/watch?v=Nkf6tuSVxXQ"));
            SongsProvider.AddSong(new Song("Paradelous - Power Slam", "https://www.youtube.com/watch?v=J635mqk769k"));
            #endregion
        }

        private void LoadSports()
        {
            #region load sports
            SportsProvider.AddSport(new Sport("Time for some sport. You should go to the gym."));
            SportsProvider.AddSport(new Sport("A healthy mind in a healthy body. Do not forget to do some sport today."));
            SportsProvider.AddSport(new Sport("If you have troubles with sleep just remember that working out can help you sleep better. We eat a lot of food rich on energy and we need to spend this energy before going to sleep."));
            SportsProvider.AddSport(new Sport("Doing some regular collective sport can help you make some new friends."));
            SportsProvider.AddSport(new Sport("OK. Go! Do some sport! Don't worry. I will be right here when you get back."));
            SportsProvider.AddSport(new Sport("It may seem counter-intuitive, but working out can drain your energy quite a bit, but then regular exercise can actually make you feel more energized throughout the day."));
            SportsProvider.AddSport(new Sport("One study found that exercising in the middle of the day can leave you feeling more energetic and productive for the rest of the afternoon."));
            SportsProvider.AddSport(new Sport("Studies show that regular exercise improve your sexual life. That should motivate you."));
            SportsProvider.AddSport(new Sport("Try making seven pushups. <prosody pitch=\"+0%\"><break time=\"1600ms\"/></prosody>. OK maybe for you it should be not in a row. Better underestimate you than send you to hospital."));
            #endregion
        }

        private void LoadMovies()
        {
            #region load movies
            MoviesProvider.AddMovie(new Movie("1", "The Bing Bang Theory", true, true));
            MoviesProvider.AddMovie(new Movie("2", "Jessica Jones", true, true));
            MoviesProvider.AddMovie(new Movie("3", "Grey's Anatomy", true, true));
            MoviesProvider.AddMovie(new Movie("4", "The Walking Dead", true, true));
            MoviesProvider.AddMovie(new Movie("5", "Game of Thrones", true, true));
            MoviesProvider.AddMovie(new Movie("6", "Homeland", true, true));
            MoviesProvider.AddMovie(new Movie("7", "The Good Doctor", true, true));
            MoviesProvider.AddMovie(new Movie("8", "Arrow", true, true));
            MoviesProvider.AddMovie(new Movie("9", "Supernatural", true, true));
            MoviesProvider.AddMovie(new Movie("10", "Agents of SHIELD", true, true));
            MoviesProvider.AddMovie(new Movie("11", "Shameless", true, true));
            MoviesProvider.AddMovie(new Movie("12", "Black Mirror", true, true));
            MoviesProvider.AddMovie(new Movie("13", "Vikings", true, true));
            MoviesProvider.AddMovie(new Movie("14", "Altered Carbon", true, true));
            MoviesProvider.AddMovie(new Movie("15", "The Blacklist", true, true));
            MoviesProvider.AddMovie(new Movie("16", "Gotham", true, true));
            MoviesProvider.AddMovie(new Movie("17", "The Flash", true, true));
            MoviesProvider.AddMovie(new Movie("18", "Friends", true, true));
            MoviesProvider.AddMovie(new Movie("19", "Timeless", true, true));
            MoviesProvider.AddMovie(new Movie("20", "Legends of Tomorrow", true, true));
            MoviesProvider.AddMovie(new Movie("21", "Seven Seconds", true, true));
            MoviesProvider.AddMovie(new Movie("22", "Rick and Morty", true, true));
            MoviesProvider.AddMovie(new Movie("23", "Counterpart", true, true));
            MoviesProvider.AddMovie(new Movie("24", "Atlanta", true, true));
            MoviesProvider.AddMovie(new Movie("25", "Suits", true, true));
            MoviesProvider.AddMovie(new Movie("26", "Star Trek: Discovery", true, true));
            MoviesProvider.AddMovie(new Movie("27", "The Orville", true, true));
            MoviesProvider.AddMovie(new Movie("28", "Supergirl", true, true));
            MoviesProvider.AddMovie(new Movie("29", "Doctor Who", true, true));
            MoviesProvider.AddMovie(new Movie("30", "Futurama", true, true));
            #endregion
        }
    }
}
