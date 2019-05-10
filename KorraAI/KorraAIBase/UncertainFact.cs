using ProbCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public class UncertainFact
    {
        double[] probabilitiesForEachPossibleAnswer;

        public UncertainFact(string p_Name, bool p_IsFactAboutTheBot, string p_Question, VarRef<FiniteDist<bool>> p_ProbVariable, string[] p_AllPossibleAnswers, double[] p_ProbabilitiesForEachPossibleAnswer, UIAnswer p_UI)
        {
            Name = p_Name;
            IsFactAboutTheBot = p_IsFactAboutTheBot;
            Question = p_Question;
            ProbVariable = p_ProbVariable;
            //NumberOfPossibleAnswers = p_NumberOfPossibleAnswers;
            AllPossibleAnswers = p_AllPossibleAnswers;
            probabilitiesForEachPossibleAnswer = p_ProbabilitiesForEachPossibleAnswer;
            UI = p_UI;

            if (AllPossibleAnswers.Length != NumberOfPossibleAnswers) throw new Exception("Mismatch number of answers for: " + Name);
            if (ProbabilitiesForEachPossibleAnswer.Length != NumberOfPossibleAnswers) throw new Exception("Mismatch number of answers for: " + Name);
        }

        public UncertainFact(string p_Name, bool p_IsFactAboutTheBot, string p_Question, VarRef<FiniteDist<bool>> p_ProbVariable, string[] p_AllPossibleAnswers, UIAnswer p_UI, double p_ProbChange)
        {
            Name = p_Name;
            IsFactAboutTheBot = p_IsFactAboutTheBot;
            Question = p_Question;
            ProbVariable = p_ProbVariable;
            //NumberOfPossibleAnswers = p_NumberOfPossibleAnswers;
            AllPossibleAnswers = p_AllPossibleAnswers;
            probabilitiesForEachPossibleAnswer = new double[NumberOfPossibleAnswers];
            UI = p_UI;
            ProbChange = p_ProbChange;

            if (AllPossibleAnswers.Length != NumberOfPossibleAnswers) throw new Exception("Mismatch number of answers for: " + Name);
            if (ProbabilitiesForEachPossibleAnswer.Length != NumberOfPossibleAnswers) throw new Exception("Mismatch number of answers for: " + Name);

            //p_ProbChange has been provided without a list of probabilities, so they must be calculated the first time
            RecalculateProbForEachAnswer();
        }

        public string Name = "";

        public bool IsFactAboutTheBot = false;

        public bool IsFactAboutTheUser
        {
            get { return !IsFactAboutTheBot; }
        }

        public string Question = "";

        #region Answers
        public int NumberOfPossibleAnswers
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// Sequence is left to right
        /// </summary>
        public string[] AllPossibleAnswers;

        /// <summary>
        /// Sequence is left to right
        /// </summary>
        public double[] ProbabilitiesForEachPossibleAnswer
        {
            get
            {               
                return probabilitiesForEachPossibleAnswer;
            }
        }

        public UIAnswer UI;
        #endregion

        #region When to ask the question again
        public bool IsUsed = false;

        public float TimeOutSeconds;

        public DateTime TimeLastAsked;
        #endregion

        public VarRef<FiniteDist<bool>> ProbVariable;

        public double ProbChange = -1;


        /// <summary>
        /// This method can be executed several times, it returns the same if the probability has not been updated between the calls
        /// </summary>
        public void RecalculateProbForEachAnswer()
        {
            if (ProbChange != -1)
            {
                //UncertainFact provided only with ProbChange (not fixed list of probabilities)

                probabilitiesForEachPossibleAnswer[0] = SharedHelper.IncreaseProb(ProbVariable.RefValue.ProbOf(e => e == true), -ProbChange);

                probabilitiesForEachPossibleAnswer[1] = ProbVariable.RefValue.ProbOf(e => e == true).Value; //get the current value

                probabilitiesForEachPossibleAnswer[2] = SharedHelper.IncreaseProb(ProbVariable.RefValue.ProbOf(e => e == true), ProbChange);

                //UnityEngine.Debug.LogWarning("Recalculate answers probabilities for " + this.Name + ": " + probabilitiesForEachPossibleAnswer[0] + " " + probabilitiesForEachPossibleAnswer[1] + " " + probabilitiesForEachPossibleAnswer[2]);
            }
            else
            {
                SharedHelper.LogWarning("Prob change requested for " + Name + ", but this question is with fixed probabilities");
            }

            #region debug
            if(probabilitiesForEachPossibleAnswer.Sum() == 0)
            {
                SharedHelper.LogError("Probabilities are all 0!!! for " + Name);
            }
            #endregion
        }

        //bool IsConditionalProbability = false;

    }

    /// <summary>
    /// Used to emulate cpp like reference to an already existing object
    /// https://stackoverflow.com/questions/10726010/store-a-reference-in-another-variable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VarRef<T>
    {
        private Func<T> _get;
        private Action<T> _set;

        public VarRef(Func<T> @get, Action<T> @set)
        {
            _get = @get;
            _set = @set;
        }

        public T RefValue
        {
            get { return _get(); }
            set { _set(value); }
        }
    }
}
