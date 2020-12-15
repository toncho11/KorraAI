using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.KorraAI.Models
{
    /// <summary>
    /// Provides an interface to load all the items that will be used during the interaction with the user
    /// </summary>
    public interface IItemsLoader
    {
        void LoadAll(ItemManager[] providers);
        Item[] GetAll();
    }
}
