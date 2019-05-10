using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ProbCSharp.ProbBase;
using ProbCSharp;

namespace Companion
{
    public static class SharedHelper
    {
        public static void Log(string message) //There is a roblem here when it is compiled outside a Unity project and then the dll is put in the Unity project
        {
            #if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.Log(message);
            #else
            Console.WriteLine("INFO: " + message);
            #endif
        }

        public static void LogError(string message)
        {
            #if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.LogError(message);
            #else
            Console.WriteLine("ERROR: " + message);
            #endif
        }

        public static void LogWarning(string message)
        {
            #if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.LogWarning(message);
            #else
            Console.WriteLine("WARNING: " + message);
            #endif
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            if (indexA != indexB)
            {
                T tmp = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = tmp;
            }
        }

        /// <summary>
        /// Returns a random permutation of the input elements
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T[] GetPermutation<T>(T[] input)
        {
            T[] data = new T[input.Length];
            input.CopyTo(data, 0);

            System.Random r = new System.Random(Guid.NewGuid().GetHashCode());

            T[] permutation = new T[data.Length];

            if (data.Length > 0)
            {
                int position = r.Next(0, data.Length);
                permutation[0] = data[position];

                Swap(data, position, data.Length - 1);

            }

            for (int i = 1; i < data.Length; i++)
            {
                int position = r.Next(0, data.Length - i);
                permutation[i] = data[position];

                Swap(data, position, data.Length - i - 1);
            }

            return permutation;
        }

        /// <summary>
        /// Many permutations stiched together with every two elements being different
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T[] GeneratePermutationArray<T>(T[] input, int minElements)
        {
            List<T> result = new List<T>();
            T[] perm = GetPermutation(input);
            result.AddRange(perm);

            while (result.Count <= minElements)
            {
                T[] newPerm = GetPermutation(input);
                int i = 0;
                while (newPerm[0].Equals(result.Last()) && i < 20)
                {
                    newPerm = GetPermutation(input);
                    i++;
                }
                //result + newPerm
                result.AddRange(newPerm);
            }

            return result.ToArray();
        }

        public static T[] GeneratePermutationArray<T>(T[] input, int minElements, T lastElement)
        {
            List<T> result = new List<T>();
            T[] perm = GetPermutation(input);

            //if the permutation is not good
            int i = 0;
            while (perm[0].Equals(lastElement) && i < 20)
            {
                perm = GetPermutation(input);
                i++;
            }
            result.AddRange(perm);

            while (result.Count <= minElements)
            {
                T[] newPerm = GetPermutation(input);
                i = 0;
                while (newPerm[0].Equals(result.Last()) && i < 20)
                {
                    newPerm = GetPermutation(input);
                    i++;
                }
                //result + newPerm
                result.AddRange(newPerm);
            }

            return result.ToArray();
        }

        public static double IncreaseProb(Prob prob, double change)
        {
            double result = prob.Value + change;

            if (result >= 1) return 0.96;
            if (result <= 0) return 0.01;

            return result;
        }

        public static Prob GetProb(FiniteDist<bool> variable)
        {
            return variable.ProbOf(e => e == true);
        }
    }
}
