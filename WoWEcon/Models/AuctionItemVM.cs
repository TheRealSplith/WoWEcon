using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WoWEcon.Models
{
    public class AuctionItemVM
    {
        public String ItemName { get; set; }
        public String Faction
        {
            get
            {
                return Data.PrimaryName;
            }
            set
            {
                // Construct data sub-object
                if (Data == null)
                    Data = new ItemData();

                // Set Names
                Data.PrimaryName = value;
                Data.OffName = value != "horde" ? "horde" : "alliance";

                // Set Colors
                Data.PrimaryColor = value == "horde" ? "red" : "blue";
                Data.OffColor = value != "horde" ? "red" : "blue";
            }
        }
        public String Realm { get; set; }
        public ItemData Data { get; set; }
    }

    public class ItemData
    {
        public KeyValuePair<Int64, Int32>[] Primary { get; set; }
        public String PrimaryName { get; set; }
        public String PrimaryColor { get; set; }
        public KeyValuePair<Int64, Int32>[] Off { get; set; }
        public String OffName { get; set; }
        public String OffColor { get; set; }
        public Int64[] Categories
        {
            get
            {
                return (Primary.Select(a => a.Key).Union(Off.Select(o => o.Key))).ToArray();
            }
        }
    }
}