using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion.KorraAI
{
    public static class PureFacts
    {
        static List<PureFact> list = new List<PureFact>();

        public static void AddPureFact(PureFact fact)
        {
            //check if ID already exists
            bool existsAlready = list.Any(cus => cus.Name == fact.Name);

            if (existsAlready)
                SharedHelper.Log("Pure Fact with this Name already exists: '" + fact.Name + "'. Second one was ignored.");
            else
            list.Add(fact);
        }

        public static List<PureFact> GetList()
        {
            return list;
        }

        public static void SetAsUsed(string name)
        {
            SetAsUsed(name, true);
        }

        public static void SetAsUsed(string name, bool isUsed)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == name)
                {
                    SharedHelper.Log("Set used to " + isUsed + " : " + list[i].Name);
                    list[i].IsUsed = isUsed;

                    return;
                }
            }

            SharedHelper.LogError("Pure Fact Name '" + name + "' not found in SetAsUsed!");
        }

        public static void MarkForSaving(string name)
        {
            SharedHelper.Log("Marking for saving: " + name);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == name)
                {
                    if (list[i].Name != "UserMovieYesterday")
                        FlagsShared.RequestSavePersistentData = true;

                    return;
                }
            }

            SharedHelper.LogError("Pure Fact Name '" + name + "'not found in MarkForSaving!");
        }

        public static void SetAsPlanned(string name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == name)
                {
                    //UnityEngine.Debug.Log("Set to used true: " + list[i].Name);
                    list[i].IsPlanned = true;
                    return;
                }
            }

            SharedHelper.LogError("Pure Fact Name not found in SetAsUsed!");
        }

        public static void SetValue(string Name, string Value)
        {
            foreach (var q in list)
            {
                if (q.Name == Name)
                {
                    SharedHelper.LogWarning("Set as answered: " + q.Name + " with value: '" + Value + "'");
                    q.Value = Value;
                    q.IsAnswered = true;

                    return;
                }
            }
        }

        public static string GetValueByName(string name)
        {
            foreach (var q in list)
            {
                if (q.Name == name)
                {
                    return q.Value;
                }
            }

            SharedHelper.LogError("GetValueByName: Pure Fact Name '" + name + "'not found");

            return "";
        }

        public static PureFact GetFacfByName(string name)
        {
            foreach (var q in list)
            {
                if (q.Name == name)
                {
                    return q;
                }
            }

            SharedHelper.LogError("GetFacfByName: Pure Fact Name '" + name + "'not found");

            return null;
        }

        public static void RemovePlannedFlagForAllPureFacts()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].IsPlanned = false;
            }
        }

        /// <summary>
        /// Used to delete answers that are invalid after validation: ex. age is not a valid number
        /// </summary>
        public static void DropAnswer(string Name)
        {
            foreach (var q in list)
            {
                if (q.Name == Name)
                {
                    SharedHelper.LogWarning("DropAnswer: " + q.Name);
                    q.Value = "";
                    q.IsAnswered = false;
                    q.IsUsed = false;
                    return;
                }
            }
        }

        public static PureFact GetPureFactAbouUser()
        {
            var q = (from pf in PureFacts.GetList()
                     where pf.Type == PureFactType.AboutUser && pf.IsPlanned == false && pf.IsUsed == false
                     select pf).ToArray();

            if (q.Length > 0)
            {
                string[] group1 = { "UserName" }; //conversation should start with these
                string[] group2 = { "UserAge", "UserSex" }; //conversation should continue with these
                string[] group3 = { "UserLocation", "UserNationality", "UserIsMarried", "UserHasKids", "UserHasJob" }; //and then with these

                List<ItemProb<string>> itemProbs = new List<ItemProb<string>>();

                //assign probabilities 
                foreach (PureFact fact in q)
                {
                    if (group1.Contains(fact.Name)) //GROUP 1 
                    {
                        itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.99)));
                        break; //add only 1 item
                    }
                    else
                    if (group2.Contains(fact.Name)) //GROUP 2
                        itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.8)));
                    else

                    if (group3.Contains(fact.Name)) //GROUP 3
                        itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.20)));

                    else itemProbs.Add(new ItemProb<string>(fact.Name, Prob(0.06))); //all the other pure facts not in group1 and group 2
                }

                var pureFactDistF = CategoricalF(itemProbs.ToArray()).Normalize();

                //SharedHelper.Log("Pure fact questions to ask the user histogram:\r\n" + pureFactDistF.Histogram());

                var pureFactDist = pureFactDistF.ToSampleDist();

                var selectionName = pureFactDist.Sample();

                PureFact selectedPureFact = PureFacts.GetFacfByName(selectionName);

                //SharedHelper.Log("GetPureFactAbouUser: selectionName " + selectionName);

                return selectedPureFact;
            }
            else
            {
                SharedHelper.LogError("GetPureFactAbouUser could not supply a pure fact about the user.");
                return null;
            }
        }
    }

    public static class UncertainFacts
    {
        static List<UncertainFact> list = new List<UncertainFact>();

        static System.Random rnd = new System.Random();

        static Queue<float> allRepeatingQuestionsTimeoutsInMinutes = new Queue<float>(); //after what time the same question can be re-asked (in minutes)

        public static void AddUncertainFact(UncertainFact fact)
        {
            list.Add(fact);
        }

        public static void GenerateRepeatingQuestionsTimeouts()
        {
            allRepeatingQuestionsTimeoutsInMinutes.Clear();

            //sets a minimum time a questioned is disabled before being asked again
            var normalDist = from seconds in Normal(35, 9) //20 minutes, 9 variance (std=3) 
                             select seconds;

            string RepeatingQuestionsTimeouts = "";

            for (var i = 0; i < 300; i++)
            {
                float value = (float)normalDist.Sample(); //sampling

                RepeatingQuestionsTimeouts = RepeatingQuestionsTimeouts + "," + value; //for Debug

                allRepeatingQuestionsTimeoutsInMinutes.Enqueue(value);
            }
        }

        public static List<UncertainFact> GetList()
        {
            return list;
        }

        public static void SetProb(string Name, double newProb)
        {
            foreach (var q in list)
            {
                if (q.Name == Name)
                {
                    SharedHelper.Log("'" + Name + "' prob variable has been updated from " + q.ProbVariable.RefValue.ProbOf(e => e == true).Value + " to " + newProb);
                    q.ProbVariable.RefValue = BernoulliF(Prob(newProb));
                }
            }
        }

        public static UncertainFact GetFacfByName(string Name)
        {
            foreach (var q in list)
            {
                if (q.Name == Name)
                {
                    return q;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a fact using the uniform distribution (currently)
        /// </summary>
        public static UncertainFact GetUncertainFactFromDist()
        {
            //if sufficent time has passed enables certain questions before giving response, if not they are not supplied by GetUncertainFactFromDist 
            //if (current_time - last_time) > randomly selected time interval (around some distribution 30 min) then IsUsed = false

            foreach (var item in list)
            {
                if (item.IsUsed)
                {
                    if (Math.Abs((DateTime.Now - item.TimeLastAsked).TotalSeconds) > item.TimeOutSeconds)
                    {
                        item.IsUsed = false;
                        //UnityEngine.Debug.Log("Question RE-enabled: " + item.Name);
                    }
                    else
                    {
                        //UnityEngine.Debug.Log("Question NOT re-enabled: " + Math.Abs((DateTime.Now - item.TimeLastAsked).TotalSeconds) + " / " + item.TimeOutSeconds);
                    }
                }
            }

            UncertainFact[] availableQuestions = list.Where(item => item.IsUsed == false).ToArray();

            int count = availableQuestions.Length;

            if (count == 0)
            {
                SharedHelper.LogError("No uncertain fact questions available. This should not be happening!");
                return null;
            }

            int next = rnd.Next(0, count);

            return availableQuestions[next];
        }

        public static void SetAsUsed(string Name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == Name)
                {
                    list[i].IsUsed = true;

                    SharedHelper.Log("Uncertain question " + Name + " is set to used.");

                    list[i].TimeLastAsked = DateTime.Now;

                    if (allRepeatingQuestionsTimeoutsInMinutes.Count == 0) GenerateRepeatingQuestionsTimeouts();

                    list[i].TimeOutSeconds = allRepeatingQuestionsTimeoutsInMinutes.Dequeue() * 60;

                    return;
                }
            }

            SharedHelper.LogError("Uncertain Fact Name '"+ Name + "' not found in SetAsUsed!");
        }
    }
}
