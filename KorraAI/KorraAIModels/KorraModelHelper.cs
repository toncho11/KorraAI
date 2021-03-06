﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using ProbCSharp;
using UnityEngine;

namespace Companion.KorraAI.Models
{
    public static class KorraModelHelper
    {
        static System.Random rndChances = new System.Random();

        public static bool GetChance(int OneInHowMany)
        {
            int selected = rndChances.Next(0, OneInHowMany);

            return (selected == 0) ? true : false; //if we hit one value ouf of 'OneInHowMany' then we return true
        }

        public static string GetChance(string[] values)
        {
            //UnityEngine.Debug.LogError("Length:" + values.Length);
            int selected = rndChances.Next(0, values.Length);

            //UnityEngine.Debug.LogError("selected:" + selected);
            return values[selected];
        }

        public static string GetChance(string[] values, string lastUsed)
        {
            int selected = -1;
            int i = -1;

            if (!string.IsNullOrEmpty(lastUsed))
            {
                i = Array.FindIndex(values, item => item == lastUsed);
            }

            if (i != -1 && values[i] != lastUsed) SharedHelper.LogError("Error locating value '" + lastUsed + "' in array!");

            if (i != -1)
            {
                SharedHelper.Swap(values, i, values.Length - 1); //disable the last used one

                if (values[values.Length - 1] != lastUsed) SharedHelper.LogError("Last value in array is incorrect!");

                selected = rndChances.Next(0, values.Length - 1); //the last one used is excluded from the selection

                if (values[selected] == lastUsed) SharedHelper.LogError("Last used is going to be used again! Not correct!");
            }
            else selected = rndChances.Next(0, values.Length);

            return values[selected];
        }

        public static string DayOfWeek()
        {
            if (BotConfigShared.Language == Lang.JA)
            {
                var culture = new System.Globalization.CultureInfo("ja-JP");
                string day = culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek);
                return day;
            }
            else
            {
                return DateTime.Now.DayOfWeek.ToString();
            }
        }

        public static bool CheckValidNationality(string nationality)
        {
            string nationalitiesString = "Afghan|Albanian|Algerian|American|Andorran|Angolan|Antiguans|Argentinean|Armenian|Australian|Austrian|Azerbaijani|Bahamian|Bahraini|Bangladeshi|Barbadian|Barbudans|Batswana|Belarusian|Belgian|Belizean|Beninese|Bhutanese|Bolivian|Bosnian|Brazilian|British|Bruneian|Bulgarian|Burkinabe|Burmese|Burundian|Cambodian|Cameroonian|Canadian|Cape Verdean|Central African|Chadian|Chilean|Chinese|Colombian|Comoran|Congolese|Costa Rican|Croatian|Cuban|Cypriot|Czech|Danish|Djibouti|Dominican|Dutch|East Timorese|Ecuadorean|Egyptian|Emirian|Equatorial Guinean|Eritrean|Estonian|Ethiopian|Fijian|Filipino|Finnish|French|Gabonese|Gambian|Georgian|German|Ghanaian|Greek|Grenadian|Guatemalan|Guinea-Bissauan|Guinean|Guyanese|Haitian|Herzegovinian|Honduran|Hungarian|I-Kiribati|Icelander|Indian|Indonesian|Iranian|Iraqi|Irish|Israeli|Italian|Ivorian|Jamaican|Japanese|Jordanian|Kazakhstani|Kenyan|Kittian and Nevisian|Kuwaiti|Kyrgyz|Laotian|Latvian|Lebanese|Liberian|Libyan|Liechtensteiner|Lithuanian|Luxembourger|Macedonian|Malagasy|Malawian|Malaysian|Maldivian|Malian|Maltese|Marshallese|Mauritanian|Mauritian|Mexican|Micronesian|Moldovan|Monacan|Mongolian|Moroccan|Mosotho|Motswana|Mozambican|Namibian|Nauruan|Nepalese|New Zealander|Ni-Vanuatu|Nicaraguan|Nigerian|Nigerien|North Korean|Northern Irish|Norwegian|Omani|Pakistani|Palauan|Panamanian|Papua New Guinean|Paraguayan|Peruvian|Polish|Portuguese|Qatari|Romanian|Russian|Rwandan|Saint Lucian|Salvadoran|Samoan|San Marinese|Sao Tomean|Saudi|Scottish|Senegalese|Serbian|Seychellois|Sierra Leonean|Singaporean|Slovakian|Slovenian|Solomon Islander|Somali|South African|South Korean|Spanish|Sri Lankan|Sudanese|Surinamer|Swazi|Swedish|Swiss|Syrian|Taiwanese|Tajik|Tanzanian|Thai|Togolese|Tongan|Trinidadian or Tobagonian|Tunisian|Turkish|Tuvaluan|Ugandan|Ukrainian|Uruguayan|Uzbekistani|Venezuelan|Vietnamese|Welsh|Yemenite|Zambian|Zimbabwean";

            if (nationalitiesString.ToLower().IndexOf(nationality.ToLower()) != -1)
                return true;
            else return false;
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                SharedHelper.LogError("FirstCharToUpper: string is null or empty");
                return "";
            }
            else return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                SharedHelper.LogError("FirstCharToLower: string is null or empty");
                return "";
            }
            else return input.First().ToString().ToLower() + input.Substring(1);
        }

        public static List<string> GetCountryList()
        {
            List<string> cultureList = new List<string>();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.LCID);

                if (!(cultureList.Contains(region.EnglishName)))
                {
                    cultureList.Add(region.EnglishName);
                }
            }
            return cultureList;
        }

        public static bool CheckValidCountryName(string name)
        {
            List<string> list = GetCountryList();

            list = list.ConvertAll(d => d.ToLower());

            if (list.Contains(name.ToLower()))
                return true;
            else return false;
        }

        public static int GetItemsLeftForSubCategory(string subCategory,ItemManager[] managers)
        {
            if (subCategory == ActionsEnum.AskPureFactQuestionAboutUser)
            {
                PureFacts pfManager = (PureFacts)managers.SingleOrDefault(x => x is PureFacts);

                if (pfManager == null)
                {
                    SharedHelper.LogError("No manager in GetPureFactAbouBot.");
                    return 0;
                }

                var q = (from item in pfManager.GetAll()
                         let pf = (PureFact)item
                         where pf.Type == PureFactType.AboutUser && pf.IsPlanned == false && pf.IsUsed == false
                         select pf).ToArray();

                return q.Length;
            }
            else
            if (subCategory == ActionsEnum.SharePureFactInfoAboutBot)
            {
                PureFacts pfManager = (PureFacts)managers.SingleOrDefault(x => x is PureFacts);

                if (pfManager == null)
                {
                    SharedHelper.LogError("No manager in GetPureFactAbouBot.");
                    return 0;
                }

                var q = (from item in pfManager.GetAll()
                         let pf = (PureFact)item
                         where pf.Type == PureFactType.AboutBot && pf.IsPlanned == false && pf.IsUsed == false
                         select pf).ToArray();

                return q.Length;
            }
            else
            {
                SharedHelper.LogError("GetItemsLeftForSubCategory: action category currently not supported");
                return 0;
            }
        }

        public static void CoupleTwoInteractionsTogether(ref List<CommItem> list, string name1, string name2)
        {
            int p = 0;
            int itemNamePos1 = -1;
            int itemNamePos2 = -1;

            for (p = 0; p < list.Count; p++)
            {
                if (list[p].IsPureFact)
                {
                    if (list[p].Name == name1) itemNamePos2 = p;
                    if (list[p].Name == name2) itemNamePos1 = p;
                    if (itemNamePos2 != -1 && itemNamePos1 != -1) break;
                }
            }

            if (itemNamePos2 != -1 && itemNamePos1 != -1 && (Math.Abs(itemNamePos2 - itemNamePos1) != 1))
            {
                int pos1 = Math.Min(itemNamePos1, itemNamePos2);
                int pos2 = Math.Max(itemNamePos1, itemNamePos2);

                var temp = list[pos2];
                list.RemoveAt(pos2);
                list.Insert(pos1 + 1, temp);

                SharedHelper.Log("Two interactions were coupled together: " + name1 + " and " + name2);

                //string DebugItem = "";
                //int i = 0;
                //foreach (string e in interactions.Select(e => e.TextToSay))
                //{
                //    i++;
                //    DebugItem += "|" + i + ". " + e;
                //}
                //SharedHelper.LogWarning("Interactions list: " + DebugItem);
            }
        }

        public static void RemoveInteraction(ref List<CommItem> list, int position, ItemManager[] managers)
        {
            if (position < list.Count)
            {
                CommItem tobeRemoved = list[position];

                ItemManager manager = managers.SingleOrDefault(x => x.Is(tobeRemoved));

                if (manager!=null)
                {
                    list.RemoveAt(position);
                    manager.SetAsPlanned(tobeRemoved.Name, false);
                    SharedHelper.Log("Interaction removed: " + tobeRemoved.Name);
                }
                else SharedHelper.LogError("Could not remove interaction from list:" + tobeRemoved.Name + " " +tobeRemoved.Category + " " + tobeRemoved.SubCategory);   
            }
            else SharedHelper.LogError("Could not remove interaction at position: " + position);

        }

        public static void SetFacialExpressionFlag(string exp)
        {
            if (!string.IsNullOrEmpty(exp))
            {
                if (exp == FaceExp.SmileAfterTalking) FlagsShared.RequestSmileAfterTalkingDone = true;
                else
                if (exp == FaceExp.SurpriseOnStartTalking) FlagsShared.RequestSurpriseExpression = true;
                else
                if (exp == FaceExp.BlinkRightEyeAfterTalking) FlagsShared.RequestRightEyeBlink = true;
                else
                if (exp == FaceExp.FlirtingAfterTalking) FlagsShared.RequestFlirtingExpression = true;
                else
                if (exp == FaceExp.BlinkRightEyeAndSmile) FlagsShared.RequestRightEyeBlinkAndSmile = true;
            }
            else SharedHelper.LogError("Unknown face expression requested: '" + exp + "'");
        }

        /// <summary>
        /// Add as first element, it will be the first interaction to be used next
        /// </summary>
        /// <param name="item"></param>
        public static void InsertFirstInteractionList(ref Queue<CommItem> interactions, CommItem item)
        {
            //if (afterReaction)
            //{
            //    var list = interactions.ToList();

            //    int i = 0; //locate the first non "ReactionToUser" item
            //    for (i = 0; i < interactions.Count; i++)
            //        if (!list[i].IsReactionToUser)
            //            break;


            //    list.Insert(i, item);
            //    interactions = new Queue<CommItem>(list);
            //}
            //else
            {
                var list = interactions.ToList();
                list.Insert(0, item);
                interactions = new Queue<CommItem>(list);
            }

            //if it is a reaction, then it should be in the item
            //CurrentTimePause = CurrentAIModel.GetCognitiveDist().GetNextInteactionPause(true);
        }

        public static bool UseAgainAlreadyUsedInteractions(DateTime? lastSessionDateTime, int thresholdDays)
        {
            if (lastSessionDateTime.HasValue)
            {
                int daysApartSessions = Convert.ToInt32((DateTime.Now - lastSessionDateTime.Value).TotalDays);

                return daysApartSessions > thresholdDays;
            }
            else
            {
                return false;
            }
        }
    }
}
