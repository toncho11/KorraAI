using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public delegate string PureFactResponseFunction(string value, out bool isValid, out string faceExpr);
    public delegate string PureFactQuestionFunction();

    /// <summary>
    /// A pure fact is a information that is exact such as: yes/no, age, distance, height, location ...
    /// </summary>
    public class PureFact : Item
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

        public PureFact(string p_Name, PureFactType p_type, string p_Question, string[] p_AllPossibleAnswers, PureFactResponseFunction p_response, UIAnswer p_UI, ContentType type)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            UI = p_UI;
            ContentType = type;

            ResponseFunc = p_response;
        }
       

        public PureFact(string p_Name, PureFactType p_type, string p_Question, string[] p_AllPossibleAnswers,
                        string p_Acknowledgement, UIAnswer p_UI, string p_StatementOnPositiveResponse,
                        string p_StatementOnNegativeResponse, ContentType type)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            Acknowledgement = p_Acknowledgement;
            UI = p_UI;
            ContentType = type;

            statementOnPositiveResponse = p_StatementOnPositiveResponse;
            statementOnNegativeResponse = p_StatementOnNegativeResponse;

            ResponseFunc = null;


        }

        public PureFact(string p_Name, PureFactType p_type, string p_Question, string[] p_AllPossibleAnswers,
                        string p_Acknowledgement, UIAnswer p_UI, string p_StatementOnPositiveResponse,
                        string p_StatementOnNegativeResponse, ContentType type, string FacialExpressionOnPositiveResponseFromUser, string FacialExpressionOnNegativeResponseFromUser)
        {
            Name = p_Name;
            Type = p_type;
            question = p_Question;
            AllPossibleAnswers = p_AllPossibleAnswers;
            Acknowledgement = p_Acknowledgement;
            UI = p_UI;
            ContentType = type;

            statementOnPositiveResponse = p_StatementOnPositiveResponse;
            statementOnNegativeResponse = p_StatementOnNegativeResponse;

            ResponseFunc = null;

            FacialExpressionOnPositiveResponse = FacialExpressionOnPositiveResponseFromUser;
            FacialExpressionOnNegativeResponse = FacialExpressionOnNegativeResponseFromUser;
        }

        //public string Name { get; set; }

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

        string value;
        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                LastUpdated = DateTime.Now;
            }
        }

        public UIAnswer UI;

        public bool IsAnswered = false;

        public string[] AllPossibleAnswers;

        #region Bot's speech responses

        public string Acknowledgement = ""; //used by SharePureFactInfoAboutBot

        string statementOnPositiveResponse;

        string statementOnNegativeResponse;

        public string GetReaction(bool IsPositiveResponse, bool IsNegativeResponse, out string FacialExpression, out bool IsResponseValid)
        {
            // it either returns a predefined response or
            // if available it executes the ResponseFunc to obtain one

            string result = "";
            FacialExpression = "";

            if (!string.IsNullOrEmpty(Acknowledgement))
            {
                result = Acknowledgement;
            }

            if (IsPositiveResponse && !string.IsNullOrEmpty(statementOnPositiveResponse))
            {
                result = statementOnPositiveResponse;
                FacialExpression = FacialExpressionOnPositiveResponse;
            }

            if (IsNegativeResponse && !string.IsNullOrEmpty(statementOnNegativeResponse))
            {
                result = statementOnNegativeResponse;
                FacialExpression = FacialExpressionOnNegativeResponse;
            }

            IsResponseValid = true;

            //overrides
            if (!IsAnswered && ResponseFunc != null) SharedHelper.LogError("Trying to use processing function on unanswered question.");

            if (ResponseFunc != null)
                result = ResponseFunc(Value, out IsResponseValid, out FacialExpression);

            return result;
        }

        #endregion

        public PureFactResponseFunction ResponseFunc;
        public PureFactQuestionFunction QuestionFunc;

        /// <summary>
        /// The StatementOnPositiveResponse and StatementOnNegativeResponse modify this value
        /// </summary>
        //public bool IsFuncValidationErrorDetected;

        public DateTime LastUpdated { get; private set; }

        #region Facial Expressions

        public string FacialExpressionOnPositiveResponse;

        public string FacialExpressionOnNegativeResponse;

        //public string FacialExpressionOnReaction;

        #endregion
    }
}
