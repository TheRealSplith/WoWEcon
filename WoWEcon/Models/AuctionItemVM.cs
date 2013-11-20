using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WoWEcon.Models
{
    public class AuctionItemVM
    {
        public String ItemName { get; set; }
        public String Faction { get; set; }
        public String Realm { get; set; }
        public KeyValuePair<Int64, Int32>[] PriceGroups { get; set; }
    }

    public class ItemData
    {
        public KeyValuePair<Int64, Int32>[] HordeItems { get; set; }
        public KeyValuePair<Int64, Int32>[] AllianceItems { get; set; }
        public IEnumerable<Int64> Categories
        {
            get
            {
                IEnumerable<Int64> cats = null;

                if (HordeItems != null && HordeItems.Length != 0)
                    cats = HordeItems.Select(a => a.Key);

                if (AllianceItems != null && AllianceItems.Length != 0)
                    cats = cats.Union(AllianceItems.Select(a => a.Key));

                cats = cats.Distinct();
                return cats;
            }
        }
    }
}