using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public delegate string PureFactResponseFunction(string value,out bool isValid);
    public delegate string PureFactQuestionFunction();

    /// <summary>
    /// Facts that are true for a long time
    /// </summary>
    public class PureFact
    {
        public PureFact(string p_Name, PureFactType p_type, PureFactQuestionFunction p_question, string[] p_AllPossibleAnswers, UIAnswer p_UI)
        {
            Name = p_Name;
            Type = p_type;
            QuestionFunc = p_question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            UI = p_UI;

            ResponseFunc = null;
        }

        public PureFact(string p_Name, PureFactType p_type, string p_Question, string p_Value, string p_Acknowledgement, UIAnswer p_UI)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            Value = p_Value;
            Acknowledgement = p_Acknowledgement;
            UI = p_UI;

            ResponseFunc = null;
        }

        /// <summary>
        /// Mainly for questions with 2 or 3 possible answers
        /// </summary>
        /// <param name="p_Name"></param>
        /// <param name="p_IsFactAboutTheBot"></param>
        /// <param name="p_Question"></param>
        /// <param name="p_AllPossibleAnswers"></param>
        /// <param name="p_Acknowledgement"></param>
        /// <param name="p_UI"></param>
        public PureFact(string p_Name, PureFactType p_type, string p_Question, string[] p_AllPossibleAnswers, string p_Acknowledgement, UIAnswer p_UI)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            Acknowledgement = p_Acknowledgement;
            UI = p_UI;

            ResponseFunc = null;
        }

        public PureFact(string p_Name, PureFactType p_type, string p_Question, PureFactResponseFunction pf, UIAnswer p_UI)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            UI = p_UI;

            ResponseFunc = pf;
        }

        public PureFact(string p_Name, PureFactType p_type, string p_Question, string[] p_AllPossibleAnswers, PureFactResponseFunction p_response, UIAnswer p_UI)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            UI = p_UI;

            ResponseFunc = p_response;
        }

        public PureFact(string p_Name, PureFactType p_type, string p_Question, string[] p_AllPossibleAnswers, string p_Acknowledgement, UIAnswer p_UI, string p_StatementOnPositiveResponse, string p_StatementOnNegativeResponse)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            Acknowledgement = p_Acknowledgement;
            UI = p_UI;

            statementOnPositiveResponse = p_StatementOnPositiveResponse;
            statementOnNegativeResponse = p_StatementOnNegativeResponse;

            ResponseFunc = null;
        }

        public string Name = "";

        public bool IsFactAboutTheBot = false;

        //public bool IsFactAboutTheUser
        //{
        //    get { return !IsFactAboutTheBot; }
        //}
        public PureFactType Type;

        string question;
        public string Question
        {
            get
            {
                if (QuestionFunc != null)
                    return QuestionFunc();
                else
                {
                    if (string.IsNullOrEmpty(question))
                        SharedHelper.LogError("Question for fact '" + this.Name + "' is empty.");

                    return question;
                }
            }
        }

        public string Value = "";

        public string Acknowledgement = ""; //not used?

        public bool IsUsed = false;

        public bool IsPlanned = false;

        public UIAnswer UI;

        public bool IsAnswered = false;

        public string[] AllPossibleAnswers;

        string statementOnPositiveResponse;
        public string StatementOnPositiveResponse
        {
            get
            {
                bool isValid = true;

                if (!IsAnswered && ResponseFunc != null) SharedHelper.LogError("Trying to use processing function on unanswered question.");

                if (ResponseFunc != null)
                    statementOnPositiveResponse = ResponseFunc(Value, out isValid);

                IsFuncValidationError = !isValid;

                return statementOnPositiveResponse;
            }

            set
            {
                if (ResponseFunc != null) SharedHelper.LogError("Trying to set a value even if a custom processing function exists.");

                statementOnPositiveResponse = value;
            }
        }

        string statementOnNegativeResponse;
        public string StatementOnNegativeResponse
        {
            get
            {
                bool isValid = true;

                if (!IsAnswered && ResponseFunc != null) SharedHelper.LogError("Trying to use processing function on unanswered question.");

                if (ResponseFunc != null)
                    statementOnNegativeResponse = ResponseFunc(Value, out isValid);

                IsFuncValidationError = !isValid;
                return statementOnNegativeResponse;
            }

            set
            {
                if (ResponseFunc != null) SharedHelper.LogError("Trying to set a value even if a custom processing function exists.");

                statementOnNegativeResponse = value;
            }
        }

        public bool IsDelayed; //currently not used

        public PureFactResponseFunction ResponseFunc;
        public PureFactQuestionFunction QuestionFunc;

        /// <summary>
        /// The StatementOnPositiveResponse and StatementOnNegativeResponse modify this value
        /// </summary>
        public bool IsFuncValidationError;

    }
}
