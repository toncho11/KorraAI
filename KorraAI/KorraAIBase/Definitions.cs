using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public enum UIAnswer { Text, Binary, MultiAnswer };

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
    }
}
