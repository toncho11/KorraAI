using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI
{
    public enum UIAnswer { Text, Binary, MultiAnswer };

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

    public interface IItem
    {
        string Name { get; set; }
        string Category { get; set; }
        string SubCategory { get; set; }
    }

    public class Item : IItem
    {
        public string Name { get; set; }

        public ContentType ContentType;

        public bool IsUsed; //already said
        public bool IsPlanned; //already scheduled

        public string Category { get; set; }
        public string SubCategory { get; set; }
    }

    /// <summary>
    /// The different types of interactions are converted to CommItem before being presented to the user.
    /// </summary>
    public struct CommItem : IItem
    {
        public CommItem(Item item)
        {
            this.Name = item.Name;
            this.Category = item.Category;
            this.SubCategory = item.SubCategory;

            if (item is Statement)
            {
                TextToSay = ((Statement)item).Text;
            }
            else TextToSay = "";

            //some default values
            IsPureFact = false;
            IsReactionToUser = false;
            IsGreeting = false;
            IsJokePureFact = false;
            UIAnswer = UIAnswer.Text;
            SpecificOutfit = -1;
            FacialExpression = "";
        }

        public string TextToSay;

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public string Name { get; set; } //ID of the item

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

    public abstract class ItemManager
    {
        public Item example;

        protected List<Item> items = new List<Item>();
        static System.Random r = new System.Random();

        public ItemManager(Item item)
        {
            example = item;
        }

        public void Add(Item item)
        {
            items.Add(item);
        }

        public Item GetByName(string name)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == name)
                {
                    return items[i];
                }
            }

            SharedHelper.LogError("GetByName not found: " + name + " " + example.Category);
            return null;
        }

        public bool SetAsUsed(string name)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == name)
                {
                    if (items[i].IsUsed == true) SharedHelper.LogError("Already set to used = true: " + name + " " + example.Category);

                    //SharedHelper.LogError("SetAsUsed : " + name + " " + items[i].Category + " " + items[i].SubCategory);
                    items[i].IsUsed = true;

                    return true;
                }
            }

            SharedHelper.LogError("SetAsUsed not found: " + name);
            return false;
        }

        public bool SetAsPlanned(string name)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == name)
                {
                    if (items[i].IsPlanned == true) SharedHelper.LogError("Already set planned = true: " + name + " " + example.Category);
                    items[i].IsPlanned = true;
                    return true;
                }
            }

            SharedHelper.LogError("SetAsPlanned not found: " + name + " " + example.Category);
            return false;
        }

        public bool SetAsPlanned(string name, bool isPlanned)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == name)
                {
                    if (items[i].IsPlanned == isPlanned) SharedHelper.LogError("Already set planned: " + isPlanned.ToString() + " " + name + " " + example.Category);
                    items[i].IsPlanned = isPlanned;
                    return true;
                }
            }

            SharedHelper.LogError("SetAsPlanned not found: " + name + " " + example.Category);
            return false;
        }

        public void RemovePlannedFlagForAll()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].IsPlanned = false;
            }
        }

        //public void RemoveUsedFlagForAll()
        //{
        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        items[i].IsUsed = false;
        //    }
        //}

        public Item[] GetAll()
        {
            return items.ToArray();
        }

        public bool Is(IItem item)
        {
            return Type.Equals(item,this.example);
        }

        public bool Is(CommItem citem)
        {
            return (!string.IsNullOrEmpty(citem.Category) && this.example.Category == citem.Category
                     && string.IsNullOrEmpty(citem.SubCategory)
                    )
                    ||
                    (!string.IsNullOrEmpty(citem.Category) && this.example.Category == citem.Category
                      && (!string.IsNullOrEmpty(citem.SubCategory) && this.example.SubCategory == citem.SubCategory)
                    );
        }

        public Item GetItem()
        {
            Item[] q;

            q = items.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

            if (q.Length > 0)
            {
                Item s = q.ElementAt(r.Next(0, q.Count()));

                return s;
            }
            else
            {
                //SharedHelper.LogError("No items for category: " + example.Category + example.SubCategory);

                if (IsAllowedResetUsageOnEmptyCategory())
                {
                    SharedHelper.LogWarning("No items for category: " + example.Category + example.SubCategory + " have been reset. They are all set to unused now.");

                    foreach (Item item in items)
                    {
                        if (item.IsUsed == true)
                        {
                            item.IsUsed = false;
                            item.IsPlanned = false;
                        }
                    }

                    //after adding items, choose one
                    q = items.Where(j => j.IsUsed == false && j.IsPlanned == false).ToArray();

                    if (q.Length > 0)
                    {
                        Item s = q.ElementAt(r.Next(0, q.Count()));

                        return s;
                    }
                }

                return null;
            }
        }

        public bool AreAllUsed()
        {
            int count = items.Where(x => x.IsUsed).Count();
            return count == items.Count();
        }

        public bool AreAllPlanned()
        {
            int count = items.Where(x => x.IsPlanned).Count();
            return count == items.Count();
        }

        public bool AreAllUnPlanned()
        {
            int count = items.Where(x => x.IsPlanned == false).Count();
            return count == items.Count();
        }

        public int AvailableItems()
        {
            int count = items.Where(x => x.IsUsed==false && x.IsPlanned==false).Count();
            return count;
        }

        //implement if the category is empty make all items unused and star sampling again
        public virtual bool IsAllowedResetUsageOnEmptyCategory()
        {
            return false;
        }

        public int Count()
        {
            return items.Count;
        }

    }
}
