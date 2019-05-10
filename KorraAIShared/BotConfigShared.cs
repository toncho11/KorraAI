using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Companion
{
    /// <summary>
    /// Provides basic configuration
    /// </summary>
    public static class BotConfigShared
    {
        /// <summary>
        /// Does the bot memorizes used items (certain)
        /// </summary>
        public static bool EnablePersistentData = true;

        public static int DefaultOutfitIndex = 1; //starts from 1

        public static bool LogOnlyErrors = false;

        public static Lang Language = Lang.EN;

        public static bool DisableAskQuestions = false; //false per default

        public static int DefaultSmile = 0; //0,1

        public static int MinimumJokesBeforeAllowingDelayedJokes = 8;

        public static bool ForceClearPesistentDataOnStart = false;

    }
}