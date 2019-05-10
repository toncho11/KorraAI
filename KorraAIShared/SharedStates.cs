using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion
{
    /// <summary>
    /// These are boolean on/off states
    /// </summary>
    public static class StatesShared
    {
        //public static bool NewEvidenceAcquired = true;
        public static bool IsEvening
        {
            get
            {
                DateTime now = DateTime.Now;

                if (now.Hour >= 18 && now.Hour < 23)
                    return true;
                else return false;
            }
        }
        public static bool IsMorning
        {
            get
            {
                DateTime now = DateTime.Now;

                if (now.Hour >= 6 && now.Hour < 12)
                    return true;
                else return false;
            }
        }
        public static bool IsMorningOrEvening
        {
            get { return IsMorning || IsEvening; }
        }

        public static bool IsLocationProvided = false;

        public static bool IsWeekend
        {
            get
            {
                DateTime now = DateTime.Now;

                return now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;
            }
        }

        public static bool OneSongAlreadyPlayed = false;

        public static int ActualJokesProvided;
    }
}
