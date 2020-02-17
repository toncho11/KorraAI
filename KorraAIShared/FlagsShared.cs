using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Companion
{
    public static class FlagsShared
    {
        public static bool InitialGreetingPerformed = false;

        public static bool RequestSavePersistentData = false;

        public static bool IsMobile
        {
            get { return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer; }
        }

        public static bool RequestSmileAfterTalkingDone; //shows after talking is done

        public static bool RequestSurpriseExpression; //shows when starts talking

        public static bool RequestFlirtingExpression; //shows after talking is done

        public static bool RequestRightEyeBlink; //shows after talking is done

        public static bool RequestRightEyeBlinkAndSmile;
    }
}
