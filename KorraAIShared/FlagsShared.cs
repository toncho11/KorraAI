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
    }
}
