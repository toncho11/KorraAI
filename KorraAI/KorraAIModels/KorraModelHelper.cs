using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using ProbCSharp;

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
    }
}
