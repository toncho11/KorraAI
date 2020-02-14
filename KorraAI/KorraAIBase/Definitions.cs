using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public enum UIAnswer { Text, Binary, MultiAnswer };

    /// <summary>
    /// The different types of interactions are coverted to CommItem before being presented to the user.
    /// </summary>
    public struct CommItem
    {
        public string TextToSay;

        public string Action;

        public string Suggestion;

        public string Name; //ID of the item

        public bool IsPureFact; //false if it is Uncertain Fact

        public UIAnswer UIAnswer; //how to display the question

        public bool IsGreeting;

        public bool IsJokePureFact; //for jokes that require question/answer

        public int SpecificOutfit;

        /// <summary>
        /// KorraAI differentiates between a new interaction and a comment after the user's own response
        /// </summary>
        public bool IsReactionToUser;

        public string FacialExpression;
    }

    public class ContentType
    {
        public ContentType() { }

        public ContentType(bool isMildOffensive, bool isRomanticReferrence, bool isMildSexual)
        {
            IsMildOffensive = isMildOffensive;
            IsRomanticReferrence = isRomanticReferrence;
            IsMildSexual = isMildSexual;
        }

        public bool IsMildOffensive { get; set; }
        public bool IsRomanticReferrence { get; set; }
        public bool IsMildSexual { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }

        public ContentType ContentType;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled

    }
}
